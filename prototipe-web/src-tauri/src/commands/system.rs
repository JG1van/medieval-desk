use crate::models::data::SystemStats;
use sysinfo::System;

#[tauri::command]
pub fn get_system_stats() -> SystemStats {
    let mut sys = System::new_all();
    sys.refresh_all();

    let os_name = System::name().unwrap_or_default();
    let os_version = System::os_version().unwrap_or_default();
    let kernel_version = System::kernel_version().unwrap_or_default();

    let cpu_usage = sys.global_cpu_usage();

    let total_memory = sys.total_memory();
    let used_memory = sys.used_memory();

    let total_swap = sys.total_swap();
    let used_swap = sys.used_swap();

    SystemStats {
        os_name,
        os_version,
        kernel_version,
        cpu_usage,
        total_memory,
        used_memory,
        total_swap,
        used_swap,
    }
}

#[tauri::command]
pub fn now() -> i64 {
    std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .unwrap()
        .as_millis() as i64
}

#[tauri::command]
pub fn window_minimize(window: tauri::Window) -> Result<(), String> {
    window.minimize().map_err(|e| e.to_string())
}

#[tauri::command]
pub fn window_maximize(window: tauri::Window) -> Result<(), String> {
    window.maximize().map_err(|e| e.to_string())
}

#[tauri::command]
pub fn window_close(window: tauri::Window) -> Result<(), String> {
    window.close().map_err(|e| e.to_string())
}

#[tauri::command]
pub fn launch_file(path: String) -> Result<(), String> {
    let p = path.trim();
    if p.is_empty() { return Err("Path kosong".into()); }
    if p.starts_with("http://") || p.starts_with("https://") {
        tauri_plugin_opener::open_url(p, None::<&str>).map_err(|e| e.to_string())
    } else {
        std::process::Command::new("cmd")
            .args(["/C", "start", "", p])
            .spawn()
            .map_err(|e| e.to_string())?;
        Ok(())
    }
}

#[tauri::command]
pub fn set_always_on_top(window: tauri::Window, state: bool) -> Result<(), String> {
    window.set_always_on_top(state).map_err(|e| e.to_string())
}