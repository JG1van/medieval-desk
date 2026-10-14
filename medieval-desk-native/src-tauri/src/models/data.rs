use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct Reference {
    pub width: f32,
    pub height: f32,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct Settings {
    pub minimize_after_launch: bool,
    pub hotkey: String,
    pub location: Location,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct Location {
    pub name: String,
    pub lat: Option<f64>,
    pub lon: Option<f64>,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct DataStore {
    pub version: u32,
    pub reference: Reference,
    pub settings: Settings,
    pub hotspots: Vec<crate::models::hotspot::Hotspot>,
    pub widgets: Vec<serde_json::Value>,
}

#[derive(Serialize)]
pub struct SystemStats {
    pub os_name: String,
    pub os_version: String,
    pub kernel_version: String,
    pub cpu_usage: f32,
    pub total_memory: u64,
    pub used_memory: u64,
    pub total_swap: u64,
    pub used_swap: u64,
}

#[derive(Serialize, Clone, Debug)]
pub struct WeatherData {
    pub temperature: f32,
    pub code: i32,
}

impl Default for DataStore {
    fn default() -> Self {
        Self {
            version: 1,
            reference: Reference { width: 1376.0, height: 768.0 },
            settings: Settings {
                minimize_after_launch: true,
                hotkey: "Ctrl+Alt+D".to_string(),
                location: Location { name: String::new(), lat: None, lon: None },
            },
            hotspots: crate::models::hotspot::default_hotspots(),
            widgets: vec![],
        }
    }
}