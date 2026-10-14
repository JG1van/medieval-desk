use crate::models::hotspot::{Hotspot, default_hotspots, LAST_TARGET};
use tauri_plugin_clipboard_manager::ClipboardExt;

#[tauri::command]
pub fn open_item(target: Option<String>) -> Result<String, String> {
    let t = target.unwrap_or_default();
    if !t.is_empty() {
        *LAST_TARGET.lock().unwrap() = Some(t.clone());
        Ok(t)
    } else {
        Err("no target provided".into())
    }
}

#[tauri::command]
pub fn list_hotspots() -> Result<Vec<Hotspot>, String> {
    Ok(default_hotspots())
}

#[tauri::command]
pub fn copy_last_target(app: tauri::AppHandle, target: String) -> Result<(), String> {
    if target.is_empty() {
        return Err("Target kosong".into());
    }
    app.clipboard().write_text(target.clone()).map_err(|e| e.to_string())?;
    *LAST_TARGET.lock().unwrap() = Some(target);
    Ok(())
}