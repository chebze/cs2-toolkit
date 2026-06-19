export type WeaponCategory = "Sniper" | "Smg" | "Pistol" | "Rifle" | "Shotgun" | "Other";

export interface WeaponDefinition {
  id: number;
  name: string;
  category: WeaponCategory;
}

export interface TriggerbotLayerSettings {
  enabled?: boolean | null;
  autoStopEnabled?: boolean | null;
  preFireFovDegrees?: number | null;
  minReactionDelayMs?: number | null;
  maxReactionDelayMs?: number | null;
}

export interface RcsLayerSettings {
  enabled?: boolean | null;
  sensitivity?: number | null;
  pitchScale?: number | null;
  yawScale?: number | null;
  firstBulletCompensateChance?: number | null;
  subsequentBulletSkipChance?: number | null;
}

export interface AimHelperLayerSettings {
  enabled?: boolean | null;
  preferredBone?: string | null;
  fovDegrees?: number | null;
}

export interface LayeredWeaponSettings<T> {
  global: T;
  byWeaponType: Record<string, T>;
  byWeapon: Record<string, T>;
}

export interface EnemyEspProfileOptions {
  mode: string;
  showPlayerName: boolean;
  showPlayerHealth: boolean;
  showBoundingBox: boolean;
  skeletonColor: string;
  skeletonLineWidth: number;
  boundingBoxColor: string;
}

export interface SoundEspProfileOptions {
  enabled: boolean;
  animation: "waves" | "staticBox";
  waveColor: string;
  waveLineWidth: number;
  waveDurationMs: number;
  minWorldRadius: number;
  maxWorldRadius: number;
  ringCount: number;
  ringSpacing: number;
}

export interface GrenadeVisualOptions {
  enabled: boolean;
  arcColor: string;
  pointColor: string;
  impactColor: string;
  landingColor: string;
  arcLineWidth: number;
  landingLineWidth: number;
}

export interface ProfileSettings {
  triggerbot: LayeredWeaponSettings<TriggerbotLayerSettings>;
  rcs: LayeredWeaponSettings<RcsLayerSettings>;
  aimHelper: LayeredWeaponSettings<AimHelperLayerSettings>;
  enemyEsp: EnemyEspProfileOptions;
  soundEsp: SoundEspProfileOptions;
  visuals: { grenade: GrenadeVisualOptions };
}

export interface ConfigProfile {
  id: string;
  name: string;
  switchHotkey?: string | null;
  settings: ProfileSettings;
}

export interface GlobalKeybinds {
  injectKey: string;
  menuToggleKey: string;
  panicKey: string;
  saveSettingsKey: string;
  rcsToggleKey: string;
  tbToggleKey: string;
  enemyEspToggleKey: string;
  soundEspToggleKey: string;
  aimHelperToggleKey: string;
  aimHelperActivationKey: string;
  tbAutoStrafeKey: string;
}

export interface ConfigStore {
  defaultProfileId: string;
  activeProfileId?: string | null;
  keybinds: GlobalKeybinds;
  profiles: ConfigProfile[];
  webPort: number;
}

export interface DashboardData {
  activeProfile: { id: string; name: string; switchHotkey?: string | null };
  defaultProfileId: string;
  accessUrls: string[];
  webPort: number;
  publicTunnel: {
    enabled: boolean;
    status: string;
    url?: string | null;
    error?: string | null;
    server: string;
    subdomain?: string | null;
  };
}
