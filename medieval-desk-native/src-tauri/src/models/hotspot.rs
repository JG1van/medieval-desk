use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize, Clone, Debug, PartialEq)]
#[serde(rename_all = "lowercase")]
pub enum HotspotKind {
    Configurable,
    Fixed,
}

#[derive(Serialize, Deserialize, Clone, Debug, PartialEq)]
#[serde(rename_all = "lowercase")]
pub enum FixedType {
    Calendar,
    Note,
    Photo,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
pub struct BoxPercent {
    pub x: f32,
    pub y: f32,
    pub w: f32,
    pub h: f32,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "lowercase")]
pub enum ItemType {
    App,
    File,
    Folder,
    Shortcut,
    Url,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
pub struct Item {
    pub id: String,
    #[serde(rename = "type")]
    pub item_type: ItemType,
    pub label: String,
    pub target: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub icon: Option<String>,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
pub struct Group {
    pub id: String,
    pub name: String,
    pub items: Vec<Item>,
}

#[derive(Serialize, Deserialize, Clone, Debug)]
#[serde(rename_all = "camelCase")]
pub struct Hotspot {
    pub id: String,
    pub label: String,
    pub kind: HotspotKind,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub fixed_type: Option<FixedType>,
    #[serde(rename = "box")]
    pub bounds: BoxPercent,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub display_mode: Option<String>,
    #[serde(default, skip_serializing_if = "Vec::is_empty", deserialize_with = "de_groups")]
    pub groups: Vec<Group>,
    #[serde(default, skip_serializing_if = "Option::is_none")]
    pub target: Option<String>,
}

fn de_groups<'de, D>(d: D) -> Result<Vec<Group>, D::Error>
where D: serde::Deserializer<'de> {
    Ok(Option::<Vec<Group>>::deserialize(d)?.unwrap_or_default())
}

pub static LAST_TARGET: std::sync::Mutex<Option<String>> = std::sync::Mutex::new(None);

pub fn default_hotspots() -> Vec<Hotspot> {
    vec![
        Hotspot {
            id: "rak_buku".into(),
            label: "Rak Buku".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 2.9, y: 19.5, w: 21.0, h: 67.5 },
            display_mode: Some("popup-list".into()),
            groups: vec![
                Group { id: "g1".into(), name: "Rak 1".into(), items: vec![] },
                Group { id: "g2".into(), name: "Rak 2".into(), items: vec![] },
                Group { id: "g3".into(), name: "Rak 3".into(), items: vec![] },
                Group { id: "g4".into(), name: "Rak 4".into(), items: vec![] },
            ],
            target: None,
        },
        Hotspot {
            id: "kalender".into(),
            label: "Kalender".into(),
            kind: HotspotKind::Fixed,
            fixed_type: Some(FixedType::Calendar),
            bounds: BoxPercent { x: 26.9, y: 36.1, w: 5.3, h: 11.9 },
            display_mode: Some("direct".into()),
            groups: vec![],
            target: None,
        },
        Hotspot {
            id: "gulungan".into(),
            label: "Gulungan".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 26.4, y: 52.0, w: 6.6, h: 7.4 },
            display_mode: Some("popup-list".into()),
            groups: vec![Group { id: "g_gulungan".into(), name: "Umum".into(), items: vec![] }],
            target: None,
        },
        Hotspot {
            id: "bingkai".into(),
            label: "Bingkai".into(),
            kind: HotspotKind::Fixed,
            fixed_type: Some(FixedType::Photo),
            bounds: BoxPercent { x: 40.5, y: 23.9, w: 19.2, h: 44.0 },
            display_mode: Some("direct".into()),
            groups: vec![],
            target: None,
        },
        Hotspot {
            id: "note".into(),
            label: "Note".into(),
            kind: HotspotKind::Fixed,
            fixed_type: Some(FixedType::Note),
            bounds: BoxPercent { x: 67.4, y: 36.9, w: 6.0, h: 10.6 },
            display_mode: Some("direct".into()),
            groups: vec![],
            target: None,
        },
        Hotspot {
            id: "bola_kristal".into(),
            label: "Bola Kristal".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 68.8, y: 47.9, w: 4.4, h: 9.5 },
            display_mode: Some("direct".into()),
            groups: vec![Group { id: "g_kristal".into(), name: "Umum".into(), items: vec![] }],
            target: None,
        },
        Hotspot {
            id: "loker".into(),
            label: "Loker".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 78.0, y: 29.3, w: 19.8, h: 56.0 },
            display_mode: Some("popup-grid".into()),
            groups: vec![
                Group { id: "g_loker_kiri".into(), name: "Pintu Kiri".into(), items: vec![] },
                Group { id: "g_loker_kanan".into(), name: "Pintu Kanan".into(), items: vec![] },
            ],
            target: None,
        },
        Hotspot {
            id: "perisai".into(),
            label: "Perisai".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 45.2, y: 4.4, w: 9.9, h: 16.1 },
            display_mode: Some("direct".into()),
            groups: vec![Group { id: "g_perisai".into(), name: "Umum".into(), items: vec![] }],
            target: None,
        },
        Hotspot {
            id: "burung_gagak".into(),
            label: "Burung Gagak".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 32.1, y: 52.4, w: 7.4, h: 32.4 },
            display_mode: Some("direct".into()),
            groups: vec![Group { id: "g_gagak".into(), name: "Umum".into(), items: vec![] }],
            target: None,
        },
        Hotspot {
            id: "peti".into(),
            label: "Peti".into(),
            kind: HotspotKind::Configurable,
            fixed_type: None,
            bounds: BoxPercent { x: 65.4, y: 66.9, w: 10.1, h: 16.7 },
            display_mode: Some("direct".into()),
            groups: vec![Group { id: "g_peti".into(), name: "Downloads".into(), items: vec![] }],
            target: None,
        },
    ]
}