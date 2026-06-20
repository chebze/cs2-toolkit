import { useEffect, useState } from "react";
import { useProfile } from "@/components/layout/AppLayout";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { TriggerbotLayerSettings } from "@/types";

const weaponTypes = ["Global", "Sniper", "Smg", "Pistol", "Rifle", "Shotgun"];

type Scope = { kind: "global" } | { kind: "type"; key: string } | { kind: "weapon"; key: string };

export function TriggerbotPage() {
  const { profile, saveProfile } = useProfile();
  const [scope, setScope] = useState<Scope>({ kind: "global" });
  const [weapons, setWeapons] = useState<Array<{ id: number; name: string; category: string }>>([]);

  useEffect(() => {
    import("@/api/client").then(({ api }) => api.getWeapons().then(setWeapons));
  }, []);

  if (!profile) return <div>Loading...</div>;

  const getSettings = (): TriggerbotLayerSettings => {
    const layered = profile.settings.triggerbot;
    if (scope.kind === "global") return layered.global;
    if (scope.kind === "type") return layered.byWeaponType[scope.key] ?? {};
    return layered.byWeapon[scope.key] ?? {};
  };

  const updateSettings = async (patch: Partial<TriggerbotLayerSettings>) => {
    const layered = structuredClone(profile.settings.triggerbot);
    const current = getSettings();
    const next = { ...current, ...patch };

    if (scope.kind === "global") layered.global = next;
    else if (scope.kind === "type") layered.byWeaponType[scope.key] = next;
    else layered.byWeapon[scope.key] = next;

    await saveProfile({
      ...profile,
      settings: { ...profile.settings, triggerbot: layered },
    });
  };

  const settings = getSettings();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Triggerbot</h1>
        <p className="text-[var(--color-muted-foreground)]">
          Priority: specific weapon → weapon type → global.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Scope</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-3">
          <div>
            <Label>Layer</Label>
            <Select
              value={scope.kind === "global" ? "global" : scope.kind === "type" ? `type:${scope.key}` : `weapon:${scope.key}`}
              onValueChange={(value) => {
                if (value === "global") setScope({ kind: "global" });
                else if (value.startsWith("type:")) setScope({ kind: "type", key: value.slice(5) });
                else setScope({ kind: "weapon", key: value.slice(7) });
              }}
            >
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {weaponTypes.map((type) => (
                  <SelectItem key={type} value={type === "Global" ? "global" : `type:${type}`}>
                    {type}
                  </SelectItem>
                ))}
                {weapons.map((w) => (
                  <SelectItem key={w.id} value={`weapon:${w.id}`}>
                    {w.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Settings</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div className="flex items-center justify-between">
            <Label>Enabled default</Label>
            <Switch
              checked={settings.enabled ?? false}
              onCheckedChange={(checked) => void updateSettings({ enabled: checked })}
            />
          </div>
          <div className="flex items-center justify-between">
            <Label>Auto-stop</Label>
            <Switch
              checked={settings.autoStopEnabled ?? false}
              onCheckedChange={(checked) => void updateSettings({ autoStopEnabled: checked })}
            />
          </div>
          <div>
            <Label>Pre-fire FOV (°)</Label>
            <Input
              type="number"
              step="0.05"
              value={settings.preFireFovDegrees ?? ""}
              onChange={(e) => void updateSettings({ preFireFovDegrees: Number(e.target.value) })}
            />
          </div>
          <div>
            <Label>Min reaction delay (ms)</Label>
            <Input
              type="number"
              value={settings.minReactionDelayMs ?? ""}
              onChange={(e) => void updateSettings({ minReactionDelayMs: Number(e.target.value) })}
            />
          </div>
          <div>
            <Label>Max reaction delay (ms)</Label>
            <Input
              type="number"
              value={settings.maxReactionDelayMs ?? ""}
              onChange={(e) => void updateSettings({ maxReactionDelayMs: Number(e.target.value) })}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
