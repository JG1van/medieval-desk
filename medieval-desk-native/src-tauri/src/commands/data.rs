use crate::models::data::DataStore;
use std::sync::{LazyLock, Mutex};
use tauri::Manager;
use tauri_plugin_dialog::DialogExt;

static STORE: LazyLock<Mutex<Option<DataStore>>> = LazyLock::new(|| Mutex::new(None));

fn db_path(app: &tauri::AppHandle) -> Result<std::path::PathBuf, String> {
    let base = app.path().app_data_dir().map_err(|e| e.to_string())?;
    Ok(base.join("medieval-desk.db"))
}

fn conn(app: &tauri::AppHandle) -> Result<rusqlite::Connection, String> {
    let path = db_path(app)?;
    if let Some(parent) = path.parent() {
        std::fs::create_dir_all(parent).map_err(|e| e.to_string())?;
    }
    let c = rusqlite::Connection::open(&path).map_err(|e| e.to_string())?;
    c.execute_batch(
        "PRAGMA journal_mode=WAL;
         CREATE TABLE IF NOT EXISTS kv (k TEXT PRIMARY KEY, v TEXT NOT NULL);
         CREATE TABLE IF NOT EXISTS migrations (version INTEGER PRIMARY KEY, applied_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP);",
    ).map_err(|e| e.to_string())?;
    Ok(c)
}

fn validate(data: &DataStore) -> Result<(), String> {
    if data.version != 1 { return Err("version harus 1".into()); }
    if data.reference.width <= 0.0 || data.reference.height <= 0.0 { return Err("reference width/height harus >0".into()); }
    if data.settings.hotkey.trim().is_empty() { return Err("settings.hotkey wajib".into()); }
    if data.hotspots.is_empty() { return Err("hotspots minimal 1".into()); }
    for (i, hs) in data.hotspots.iter().enumerate() {
        if hs.id.trim().is_empty() { return Err(format!("hotspots[{}].id wajib", i)); }
        if hs.label.trim().is_empty() { return Err(format!("hotspots[{}].label wajib", i)); }
        if hs.bounds.x < 0.0 || hs.bounds.x > 100.0 || hs.bounds.y < 0.0 || hs.bounds.y > 100.0 { return Err(format!("hotspots[{}].box x/y 0-100", i)); }
        if hs.bounds.w <= 0.0 || hs.bounds.w > 100.0 || hs.bounds.h <= 0.0 || hs.bounds.h > 100.0 { return Err(format!("hotspots[{}].box w/h 0-100", i)); }
        if hs.kind == crate::models::hotspot::HotspotKind::Fixed && hs.fixed_type.is_none() { return Err(format!("hotspots[{}].fixed_type wajib", i)); }
        if hs.kind == crate::models::hotspot::HotspotKind::Configurable {
            for (gi, g) in hs.groups.iter().enumerate() {
                if g.id.trim().is_empty() { return Err(format!("hotspots[{}].groups[{}].id wajib", i, gi)); }
                if g.name.trim().is_empty() { return Err(format!("hotspots[{}].groups[{}].name wajib", i, gi)); }
                for (ii, it) in g.items.iter().enumerate() {
                    if it.id.trim().is_empty() { return Err(format!("items[{}].id wajib", ii)); }
                    if it.label.trim().is_empty() { return Err(format!("items[{}].label wajib", ii)); }
                    if it.target.trim().is_empty() { return Err(format!("items[{}].target wajib", ii)); }
                }
            }
        }
    }
    Ok(())
}

fn store_guard() -> Result<std::sync::MutexGuard<'static, Option<DataStore>>, String> {
    STORE.lock().map_err(|e| e.to_string())
}

fn default_data() -> DataStore {
    serde_json::from_str(include_str!("../../data/medieval-desk.json")).unwrap_or_default()
}

#[tauri::command]
pub async fn load_data(app: tauri::AppHandle) -> Result<DataStore, String> {
    if let Some(d) = store_guard()?.clone() { return Ok(d); }
    let d = {
        let c = conn(&app)?;
        c.query_row("SELECT v FROM kv WHERE k='data'", [], |r| r.get::<_, String>(0))
            .ok()
            .and_then(|raw| serde_json::from_str::<DataStore>(&raw).ok())
    };
    let d = match d {
        Some(d) => d,
        None => {
            let d = default_data();
            let c = conn(&app)?;
            let json = serde_json::to_string_pretty(&d).map_err(|e| e.to_string())?;
            c.execute("INSERT OR REPLACE INTO kv (k, v) VALUES ('data', ?1)", [&json]).map_err(|e| e.to_string())?;
            d
        }
    };
    *store_guard()? = Some(d.clone());
    Ok(d)
}

#[tauri::command]
pub async fn save_data(app: tauri::AppHandle, data: DataStore) -> Result<(), String> {
    validate(&data)?;
    let c = conn(&app)?;
    let json = serde_json::to_string_pretty(&data).map_err(|e| e.to_string())?;
    c.execute("INSERT OR REPLACE INTO kv (k, v) VALUES ('data', ?1)", [&json]).map_err(|e| e.to_string())?;
    *store_guard()? = Some(data);
    Ok(())
}

#[tauri::command]
pub async fn export_data(app: tauri::AppHandle) -> Result<Option<String>, String> {
    let data = {
        let g = store_guard()?;
        g.clone()
    };
    let data = match data {
        Some(d) => d,
        None => load_data(app.clone()).await?,
    };
    let json = serde_json::to_string_pretty(&data).map_err(|e| e.to_string())?;
    let (tx, rx) = tokio::sync::oneshot::channel();
    app.dialog()
        .file()
        .add_filter("JSON", &["json"])
        .set_file_name("medieval-desk-backup.json")
        .pick_file(move |path| { let _ = tx.send(path); });
    let path = rx.await.map_err(|e| e.to_string())?;
    match path.and_then(|p| p.into_path().ok()) {
        Some(p) => { std::fs::write(p, json).map_err(|e| e.to_string())?; Ok(Some("Data diekspor".into())) }
        None => Ok(None),
    }
}

#[tauri::command]
pub async fn import_data(app: tauri::AppHandle) -> Result<DataStore, String> {
    let (tx, rx) = tokio::sync::oneshot::channel();
    app.dialog()
        .file()
        .add_filter("JSON", &["json"])
        .pick_file(move |path| { let _ = tx.send(path); });
    let path = rx.await.map_err(|e| e.to_string())?;
    let path = path.and_then(|p| p.into_path().ok()).ok_or("Tidak ada file")?;
    let raw = std::fs::read_to_string(&path).map_err(|e| e.to_string())?;
    let data: DataStore = serde_json::from_str(&raw).map_err(|_| String::from("JSON tidak valid"))?;
    validate(&data)?;
    save_data(app, data.clone()).await?;
    Ok(data)
}