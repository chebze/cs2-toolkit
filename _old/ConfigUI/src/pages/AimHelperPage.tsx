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
import type { AimHelperLayerSettings } from "@/types";

const weaponTypes = ["Global", "Sniper", "Smg", "Pistol", "Rifle", "Shotgun"];
type Scope = { kind: "global" } | { kind: "type"; key: string } | { kind: "weapon"; key: string };

export function AimHelperPage() {
  const { profile, saveProfile } = useProfile();
  const [scope, setScope] = useState<Scope>({ kind: "global" });
  const [weapons, setWeapons] = useState<Array<{ id: number; name: string }>>([]);

  useEffect(() => {
    import("@/api/client").then(({ api }) => api.getWeapons().then(setWeapons));
  }, []);

  if (!profile) return <div>Loading...</div>;

  const getSettings = (): AimHelperLayerSettings => {
    const layered = profile.settings.aimHelper;
    if (scope.kind === "global") return layered.global;
    if (scope.kind === "type") return layered.byWeaponType[scope.key] ?? {};
    return layered.byWeapon[scope.key] ?? {};
  };

  const updateSettings = async (patch: Partial<AimHelperLayerSettings>) => {
    const layered = structuredClone(profile.settings.aimHelper);
    const next = { ...getSettings(), ...patch };
    if (scope.kind === "global") layered.global = next;
    else if (scope.kind === "type") layered.byWeaponType[scope.key] = next;
    else layered.byWeapon[scope.key] = next;
    await saveProfile({ ...profile, settings: { ...profile.settings, aimHelper: layered } });
  };

  const settings = getSettings();

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Aim Helper</h1>
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
            <Label>FOV (°)</Label>
            <Input type="number" step="0.1" value={settings.fovDegrees ?? ""} onChange={(e) => void updateSettings({ fovDegrees: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Preferred bone</Label>
            <Select value={settings.preferredBone ?? "Head"} onValueChange={(v) => void updateSettings({ preferredBone: v })}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {["Head", "Neck", "Body"].map((bone) => (
                  <SelectItem key={bone} value={bone}>{bone}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
