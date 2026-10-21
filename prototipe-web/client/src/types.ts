export interface BoxPercent {
  x: number
  y: number
  w: number
  h: number
}

export type FixedType = 'calendar' | 'note' | 'photo'
export type ItemType = 'app' | 'file' | 'folder' | 'shortcut' | 'url'

export interface Item {
  id: string
  type: ItemType
  label: string
  target: string
  icon?: string
}

export interface Group {
  id: string
  name: string
  items: Item[]
}

export interface Hotspot {
  id: string
  label: string
  kind: 'configurable' | 'fixed'
  fixedType?: FixedType
  box: BoxPercent
  displayMode?: 'popup-grid' | 'popup-list' | 'direct'
  groups?: Group[]
  target?: string
}

export interface Reference {
  width: number
  height: number
}

export interface Settings {
  minimizeAfterLaunch: boolean
  hotkey: string
  location: {
    name: string
    lat: number | null
    lon: number | null
  }
}

export interface DataStore {
  version: number
  reference: Reference
  settings: Settings
  hotspots: Hotspot[]
  widgets: unknown[]
}

export interface SystemStats {
  os_name: string
  os_version: string
  kernel_version: string
  cpu_usage: number
  total_memory: number
  used_memory: number
  total_swap: number
  used_swap: number
}

export interface WeatherData {
  temperature: number
  code: number
}