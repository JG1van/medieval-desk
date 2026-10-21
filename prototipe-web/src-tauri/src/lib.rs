pub mod commands;
pub mod models;

use commands::{data, hotspot, system, weather};

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_global_shortcut::Builder::default().build())
        .plugin(tauri_plugin_clipboard_manager::init())
        .invoke_handler(tauri::generate_handler![
            data::load_data,
            data::save_data,
            data::export_data,
            data::import_data,
            system::get_system_stats,
            system::now,
            system::window_minimize,
            system::window_maximize,
            system::window_close,
            system::launch_file,
            system::set_always_on_top,
            hotspot::open_item,
            hotspot::list_hotspots,
            hotspot::copy_last_target,
            weather::get_weather,
        ])
        .run(tauri::generate_context!())
        .expect("error running tauri application");
}