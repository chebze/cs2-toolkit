import { useEffect, useState } from "react";
import { api } from "@/api/client";
import type { GlobalKeybinds } from "@/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";

const fields: Array<{ key: keyof GlobalKeybinds; label: string }> = [
  { key: "injectKey", label: "Inject / attach" },
  { key: "menuToggleKey", label: "Menu toggle" },
  { key: "panicKey", label: "Panic shutdown" },
  { key: "saveSettingsKey", label: "Save settings" },
  { key: "rcsToggleKey", label: "RCS toggle" },
  { key: "tbToggleKey", label: "Triggerbot toggle" },
  { key: "enemyEspToggleKey", label: "Enemy ESP toggle" },
  { key: "soundEspToggleKey", label: "Sound ESP toggle" },
  { key: "bulletTracersToggleKey", label: "Bullet tracers toggle" },
  { key: "aimHelperToggleKey", label: "Aim helper toggle" },
  { key: "aimHelperActivationKey", label: "Aim helper activation (optional)" },
  { key: "tbAutoStrafeKey", label: "Triggerbot auto-strafe key" },
];

export function KeybindsPage() {
  const [keybinds, setKeybinds] = useState<GlobalKeybinds | null>(null);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    api.getKeybinds().then(setKeybinds);
  }, []);

  if (!keybinds) return <div>Loading keybinds...</div>;

  const update = (key: keyof GlobalKeybinds, value: string) => {
    setKeybinds({ ...keybinds, [key]: value });
    setSaved(false);
  };

  const save = async () => {
    await api.updateKeybinds(keybinds);
    setSaved(true);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Global Keybinds</h1>
        <p className="text-[var(--color-muted-foreground)]">
          Keybinds are shared across all configuration profiles.
        </p>
      </div>
      <Card>
        <CardHeader><CardTitle>Hotkeys</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          {fields.map(({ key, label }) => (
            <div key={key}>
              <Label>{label}</Label>
              <Input value={keybinds[key]} onChange={(e) => update(key, e.target.value)} />
            </div>
          ))}
          <div className="md:col-span-2 flex items-center gap-3">
            <Button onClick={() => void save()}>Save keybinds</Button>
            {saved && <span className="text-sm text-[var(--color-primary)]">Saved — applied live in-game</span>}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
