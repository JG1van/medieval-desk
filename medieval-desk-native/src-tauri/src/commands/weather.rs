use crate::models::data::WeatherData;
use std::sync::{LazyLock, Mutex};
use std::time::{Duration, Instant};

static WEATHER_CACHE: LazyLock<Mutex<Option<(Instant, WeatherData)>>> = LazyLock::new(|| Mutex::new(None));

#[tauri::command]
pub async fn get_weather(lat: f64, lon: f64) -> Result<Option<WeatherData>, String> {
    let cached = {
    let mut g = WEATHER_CACHE.lock().map_err(|e| e.to_string())?;
    match g.as_ref() {
        Some((t, w)) if t.elapsed() < Duration::from_secs(15 * 60) => Some(w.clone()),
        Some(_) => { *g = None; None }
        None => None,
    }
};
    if let Some(w) = cached {
        return Ok(Some(w));
    }

    let url = format!(
        "https://api.open-meteo.com/v1/forecast?latitude={}&longitude={}&current=temperature_2m,weather_code",
        lat, lon
    );

    let resp = reqwest::get(&url).await.map_err(|e| e.to_string())?;
    let json: serde_json::Value = resp.json().await.map_err(|e| e.to_string())?;

    let current = json.get("current").ok_or("Cuaca tidak tersedia")?;
    let temp = current.get("temperature_2m").and_then(|v| v.as_f64()).ok_or("Suhu tidak ada")? as f32;
    let code = current.get("weather_code").and_then(|v| v.as_i64()).ok_or("Kode tidak ada")? as i32;

    let w = WeatherData { temperature: temp, code };
    *WEATHER_CACHE.lock().map_err(|e| e.to_string())? = Some((Instant::now(), w.clone()));
    Ok(Some(w))
}