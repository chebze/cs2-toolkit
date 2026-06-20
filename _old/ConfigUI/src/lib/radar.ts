import type { MapConfig } from "@/types";

export const MAP_SIZE = 1024;

export function worldToRadar(
  worldX: number,
  worldY: number,
  posX: number,
  posY: number,
  scale: number,
  width: number,
  height: number
) {
  const px = (worldX - posX) / scale;
  const py = (posY - worldY) / scale;
  const x = Math.max(0, Math.min(width, (px / MAP_SIZE) * width));
  const y = Math.max(0, Math.min(height, (py / MAP_SIZE) * height));
  return { x, y };
}

export function normalizeMapName(mapName: string) {
  const printable = mapName.replace(/[^\x20-\x7E]/g, "");
  const match = printable.match(/(?:cs_|de_|ar_)[a-z0-9_]+/i);
  return match ? match[0].toLowerCase() : printable.trim();
}

export type RadarLevel = "upper" | "lower";

export function getLowerAltitudeThreshold(mapConfig: MapConfig | null | undefined) {
  if (!mapConfig || mapConfig.lower_altitude === undefined) return null;
  return mapConfig.lower_altitude;
}

export function resolveRadarLevel(mapConfig: MapConfig | null | undefined, z: number): RadarLevel {
  const threshold = getLowerAltitudeThreshold(mapConfig);
  if (threshold === null) return "upper";
  return z <= threshold ? "lower" : "upper";
}

export function isOnRadarLevel(mapConfig: MapConfig | null | undefined, z: number, level: RadarLevel) {
  return resolveRadarLevel(mapConfig, z) === level;
}

export function mapSupportsLowerLevel(mapName: string) {
  const normalized = normalizeMapName(mapName);
  return normalized === "de_nuke" || normalized === "de_vertigo";
}

export function getMapConfig(maps: MapConfig[], mapName: string) {
  const normalized = normalizeMapName(mapName);
  return maps.find((m) => m.map_name === normalized) ?? null;
}

export function weaponIconUrl(weaponName: string) {
  return `/radar/weapons/${weaponName.toLowerCase()}.svg`;
}

export function mapImageUrl(mapName: string, level: RadarLevel = "upper") {
  const normalized = normalizeMapName(mapName);
  const suffix = level === "lower" && mapSupportsLowerLevel(normalized) ? "_lower" : "";
  return `/radar/maps/${normalized}${suffix}_radar_psd.png`;
}

export function mapDisplayName(mapName: string, level: RadarLevel) {
  const normalized = normalizeMapName(mapName);
  return level === "lower" && mapSupportsLowerLevel(normalized)
    ? `${normalized} (lower)`
    : normalized;
}
