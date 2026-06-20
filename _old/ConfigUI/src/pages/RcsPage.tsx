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
import type { RcsLayerSettings } from "@/types";

const weaponTypes = ["Global", "Sniper", "Smg", "Pistol", "Rifle", "Shotgun"];
type Scope = { kind: "global" } | { kind: "type"; key: string } | { kind: "weapon"; key: string };

export function RcsPage() {
  const { profile, saveProfile } = useProfile();
  const [scope, setScope] = useState<Scope>({ kind: "global" });
  const [weapons, setWeapons] = useState<Array<{ id: number; name: string }>>([]);

  useEffect(() => {
    import("@/api/client").then(({ api }) => api.getWeapons().then(setWeapons));
  }, []);

  if (!profile) return <div>Loading...</div>;

  const getSettings = (): RcsLayerSettings => {
    const layered = profile.settings.rcs;
    if (scope.kind === "global") return layered.global;
    if (scope.kind === "type") return layered.byWeaponType[scope.key] ?? {};
    return layered.byWeapon[scope.key] ?? {};
  };

  const updateSettings = async (patch: Partial<RcsLayerSettings>) => {
    const layered = structuredClone(profile.settings.rcs);
    const next = { ...getSettings(), ...patch };
    if (scope.kind === "global") layered.global = next;
    else if (scope.kind === "type") layered.byWeaponType[scope.key] = next;
    else layered.byWeapon[scope.key] = next;
    await saveProfile({ ...profile, settings: { ...profile.settings, rcs: layered } });
  };

  const settings = getSettings();

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Recoil Control (RCS)</h1>
      <Card>
        <CardHeader><CardTitle>Scope</CardTitle></CardHeader>
        <CardContent>
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
                <SelectItem key={w.id} value={`weapon:${w.id}`}>{w.name}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>
      <Card>
        <CardHeader><CardTitle>Settings</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div className="flex items-center justify-between">
            <Label>Enabled default</Label>
            <Switch checked={settings.enabled ?? false} onCheckedChange={(c) => void updateSettings({ enabled: c })} />
          </div>
          <div>
            <Label>Sensitivity</Label>
            <Input type="number" step="0.05" value={settings.sensitivity ?? ""} onChange={(e) => void updateSettings({ sensitivity: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Pitch scale</Label>
            <Input type="number" step="0.1" value={settings.pitchScale ?? ""} onChange={(e) => void updateSettings({ pitchScale: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Yaw scale</Label>
            <Input type="number" step="0.1" value={settings.yawScale ?? ""} onChange={(e) => void updateSettings({ yawScale: Number(e.target.value) })} />
          </div>
          <div>
            <Label>First bullet compensate chance</Label>
            <Input type="number" step="0.05" min="0" max="1" value={settings.firstBulletCompensateChance ?? ""} onChange={(e) => void updateSettings({ firstBulletCompensateChance: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Subsequent bullet skip chance</Label>
            <Input type="number" step="0.05" min="0" max="1" value={settings.subsequentBulletSkipChance ?? ""} onChange={(e) => void updateSettings({ subsequentBulletSkipChance: Number(e.target.value) })} />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
