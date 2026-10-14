use serde::{Deserialize, Serialize};
use tauri::{Manager, Window};
use tauri_plugin_global_shortcut::ShortcutState;
use std::sync::Mutex;
use std::collections::HashMap;
use std::sync::LazyLock;

#[derive(Debug, Clone, Serialize, Deserialize)]
struct HotspotConfig {
    id: String,
    name: String,
    x: f64,
    y: f64,
    w: f64,
    h: f64,
    #[serde(rename = "type")]
    content_type: String,
    #[serde(default, rename = "items")]
    items: Vec<String>,
    #[serde(default, rename = "target")]
    target: String,
    #[serde(rename = "displayMode", default = "default_display_mode")]
    display_mode: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
struct AppState {
    show_edit_mode: bool,
}

#[tauri::command]
fn get_hotspots(hotspots: tauri::State<'_, Mutex<Vec<HotspotConfig>>>) -> Result<Vec<HotspotConfig>, String> {
    let list = hotspots.lock().map_err(|e| e.to_string())?;
    Ok(list.clone())
}

#[tauri::command]
fn update_hotspot(
    hotspots: tauri::State<'_, Mutex<Vec<HotspotConfig>>>,
    id: String,
    x: f64,
    y: f64,
    w: f64,
    h: f64,
) -> Result<(), String> {
    let mut list = hotspots.lock().map_err(|e| e.to_string())?;
    if let Some(hs) = list.iter_mut().find(|h| h.id == id) {
        hs.x = x;
        hs.y = y;
        hs.w = w;
        hs.h = h;
        Ok(())
    } else {
        Err("Hotspot not found".to_string())
    }
}

#[tauri::command]
fn export_config(hotspots: tauri::State<'_, Mutex<Vec<HotspotConfig>>>) -> Result<String, String> {
    let list = hotspots.lock().map_err(|e| e.to_string())?;
    serde_json::to_string_pretty(&*list).map_err(|e| e.to_string())
}

#[tauri::command]
fn import_config(
    hotspots: tauri::State<'_, Mutex<Vec<HotspotConfig>>>,
    config_json: String,
) -> Result<(), String> {
    let parsed: Vec<HotspotConfig> = serde_json::from_str(&config_json).map_err(|e| e.to_string())?;
    let mut list = hotspots.lock().map_err(|e| e.to_string())?;
    *list = parsed;
    Ok(())
}

#[tauri::command]
fn delete_hotspot(
    hotspots: tauri::State<'_, Mutex<Vec<HotspotConfig>>>,
    id: String,
) -> Result<(), String> {
    let mut list = hotspots.lock().map_err(|e| e.to_string())?;
    list.retain(|h| h.id != id);
    Ok(())
}

#[tauri::command]
fn launch_file(path: String) -> Result<(), String> {
    if path.trim().is_empty() {
        return Err("Path kosong".into());
    }
    #[cfg(target_os = "windows")]
    {
        std::process::Command::new("cmd")
            .args(["/C", "start", "", &path])
            .spawn()
            .map_err(|e| e.to_string())?;
    }
    #[cfg(not(target_os = "windows"))]
    {
        std::process::Command::new("xdg-open")
            .arg(&path)
            .spawn()
            .map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn set_always_on_top(window: tauri::Window, state: bool) -> Result<(), String> {
    window.set_always_on_top(state).map_err(|e| e.to_string())?;
    Ok(())
}

#[tauri::command]
fn import_photo(app: tauri::AppHandle, src: String) -> Result<String, String> {
    let src_path = std::path::PathBuf::from(&src);
    if !src_path.is_file() {
        return Err("File sumber tidak ditemukan".into());
    }
    let base = app.path().app_data_dir().map_err(|e| e.to_string())?;
    let dir = base.join("photos");
    std::fs::create_dir_all(&dir).map_err(|e| e.to_string())?;

    let stem = src_path.file_stem().and_then(|s| s.to_str()).unwrap_or("photo").to_string();
    let ext = src_path.extension().and_then(|s| s.to_str()).unwrap_or("").to_string();

    let mut dest = dir.join(src_path.file_name().ok_or("Nama file tidak valid")?);
    if dest.exists() {
        let mut i = 1;
        loop {
            let cand = if ext.is_empty() { format!("{stem}_{i}") } else { format!("{stem}_{i}.{ext}") };
            let p = dir.join(cand);
            if !p.exists() { dest = p; break; }
            i += 1;
        }
    }
    std::fs::copy(&src_path, &dest).map_err(|e| e.to_string())?;
    Ok(dest.to_string_lossy().to_string())
}

#[tauri::command]
fn toggle_window(window: Window) -> Result<(), String> {
    if window.is_visible().unwrap_or(false) {
        let _ = window.hide();
    } else {
        let _ = window.show();
        let _ = window.set_focus();
    }
    Ok(())
}

#[tauri::command]
fn get_edit_mode(state: tauri::State<'_, Mutex<AppState>>) -> Result<bool, String> {
    let s = state.lock().map_err(|e| e.to_string())?;
    Ok(s.show_edit_mode)
}

#[tauri::command]
fn set_edit_mode(
    state: tauri::State<'_, Mutex<AppState>>,
    value: bool,
) -> Result<(), String> {
    let mut s = state.lock().map_err(|e| e.to_string())?;
    s.show_edit_mode = value;
    Ok(())
}

// Cache: path -> Some(base64 png) | None (gagal)
static ICON_CACHE: LazyLock<Mutex<HashMap<String, Option<String>>>> =
    LazyLock::new(|| Mutex::new(HashMap::new()));

fn extract_icons(paths: Vec<String>) -> HashMap<String, Option<String>> {
    use std::io::Write;
    let mut result: HashMap<String, Option<String>> = paths.iter().map(|p| (p.clone(), None)).collect();

    let local: Vec<&String> = paths
        .iter()
        .filter(|p| !p.starts_with("http") && std::path::Path::new(p.as_str()).exists())
        .collect();
    if local.is_empty() {
        return result;
    }

    const SCRIPT: &str = r#"
$ErrorActionPreference = 'SilentlyContinue'
Add-Type -AssemblyName System.Drawing
foreach ($p in ([Console]::In.ReadToEnd() | ConvertFrom-Json)) {
  $b64 = ''
  try {
    $t = $p
    if ($t -like '*.lnk') {
      $sh = New-Object -ComObject WScript.Shell
      $t = $sh.CreateShortcut($t).TargetPath
    }
    if ($t -and (Test-Path -LiteralPath $t)) {
      $ico = [System.Drawing.Icon]::ExtractAssociatedIcon($t)
      if ($ico) {
        $ms = New-Object System.IO.MemoryStream
        $ico.ToBitmap().Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $b64 = [Convert]::ToBase64String($ms.ToArray())
        $ms.Dispose()
      }
    }
  } catch {}
  Write-Output ($p + [char]9 + $b64)
}
"#;

    let child = std::process::Command::new("powershell")
        .args([
            "-NoProfile",
            "-NonInteractive",
            "-ExecutionPolicy",
            "Bypass",
            "-Command",
            SCRIPT,
        ])
        .stdin(std::process::Stdio::piped())
        .stdout(std::process::Stdio::piped())
        .spawn();

    let mut child = match child {
        Ok(c) => c,
        Err(_) => return result,
    };
    let local_owned: Vec<String> = local.into_iter().cloned().collect();
    {
        if let Some(si) = child.stdin.as_mut() {
            if let Ok(json) = serde_json::to_string(&local_owned) {
                let _ = si.write_all(json.as_bytes());
            }
        }
    }
    child.stdin = None; // close stdin pipe
    let out = match child.wait_with_output() {
        Ok(o) => o,
        Err(_) => return result,
    };

    let stdout = String::from_utf8_lossy(&out.stdout);
    for line in stdout.lines() {
        let line = line.trim_end_matches(['\r', '\n']);
        if line.is_empty() {
            continue;
        }
        if let Some((p, b64)) = line.split_once('\t') {
            let b64 = b64.trim();
            if !b64.is_empty() {
                result.insert(p.to_string(), Some(b64.to_string()));
            }
        }
    }
    result
}

#[tauri::command]
async fn get_icons(paths: Vec<String>) -> Result<HashMap<String, Option<String>>, String> {
    let mut out: HashMap<String, Option<String>> = HashMap::new();
    let mut todo: Vec<String> = Vec::new();
    {
        let cache = ICON_CACHE.lock().map_err(|e| e.to_string())?;
        for p in &paths {
            match cache.get(p) {
                Some(v) => { out.insert(p.clone(), v.clone()); }
                None => {
                    if !todo.contains(p) { todo.push(p.clone()); }
                }
            }
        }
    }
    if !todo.is_empty() {
        let found = tauri::async_runtime::spawn_blocking(move || extract_icons(todo))
            .await
            .map_err(|e| e.to_string())?;
        let mut cache = ICON_CACHE.lock().map_err(|e| e.to_string())?;
        for (k, v) in found {
            cache.insert(k.clone(), v.clone());
            out.insert(k, v);
        }
    }
    Ok(out)
}

fn default_display_mode() -> String {
    "popup-grid".to_string()
}

fn default_hotspots() -> Vec<HotspotConfig> {
    vec![
        HotspotConfig { id: "rack".into(), name: "Rak Buku".into(), x: 3.2, y: 18.5, w: 21.0, h: 66.5, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "popup-list".into() },
        HotspotConfig { id: "calendar".into(), name: "Kalender".into(), x: 26.9, y: 36.1, w: 5.3, h: 11.9, content_type: "calendar".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
        HotspotConfig { id: "scroll".into(), name: "Gulungan (Tool AI)".into(), x: 26.4, y: 52.0, w: 6.6, h: 7.4, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "popup-list".into() },
        HotspotConfig { id: "frame".into(), name: "Bingkai (Foto)".into(), x: 40.5, y: 23.9, w: 19.2, h: 44.0, content_type: "photo".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
        HotspotConfig { id: "note".into(), name: "Note".into(), x: 67.4, y: 36.9, w: 6.0, h: 10.6, content_type: "note".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
        HotspotConfig { id: "crystal".into(), name: "Bola Kristal (Browser)".into(), x: 68.8, y: 47.9, w: 4.4, h: 9.5, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
        HotspotConfig { id: "locker".into(), name: "Loker (Berkas)".into(), x: 78.0, y: 29.3, w: 19.8, h: 56.0, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "popup-grid".into() },
        HotspotConfig { id: "shield".into(), name: "Perisai".into(), x: 45.2, y: 4.4, w: 9.9, h: 16.1, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
        HotspotConfig { id: "raven".into(), name: "Gagak".into(), x: 32.1, y: 52.4, w: 7.4, h: 32.4, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
        HotspotConfig { id: "chest".into(), name: "Peti Harta Karun".into(), x: 65.4, y: 66.9, w: 10.1, h: 16.7, content_type: "list".into(), items: vec![], target: "".into(), display_mode: "direct".into() },
    ]
}

fn main() {
    tauri::Builder::default()
        .plugin(
            tauri_plugin_global_shortcut::Builder::new()
                .with_shortcuts(["ctrl+alt+d"])
                .expect("Failed to parse shortcut")
                .with_handler(|app, _shortcut, event| {
                    if event.state == ShortcutState::Pressed {
                        let windows = app.webview_windows();
                        if let Some(window) = windows.values().next() {
                            let _ = if window.is_visible().unwrap_or(false) {
                                window.hide()
                            } else {
                                window.show().and_then(|_| window.set_focus())
                            };
                        }
                    }
                })
                .build(),
        )
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_shell::init())
        .manage(Mutex::new(default_hotspots()))
        .manage(Mutex::new(AppState { show_edit_mode: false }))
        .invoke_handler(tauri::generate_handler![
            get_hotspots,
            update_hotspot,
            export_config,
            import_config,
            delete_hotspot,
            toggle_window,
            get_edit_mode,
            set_edit_mode,
            launch_file,
            set_always_on_top,
            import_photo,
            get_icons,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}