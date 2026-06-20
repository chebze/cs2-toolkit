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

export function VisualsPage() {
  const { profile, saveProfile } = useProfile();
  if (!profile) return <div>Loading...</div>;

  const grenade = profile.settings.visuals.grenade;

  const update = async (patch: Partial<typeof grenade>) => {
    await saveProfile({
      ...profile,
      settings: {
        ...profile.settings,
        visuals: { grenade: { ...grenade, ...patch } },
      },
    });
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Visuals</h1>
      <Card>
        <CardHeader><CardTitle>Grenade Trajectory</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div className="flex items-center justify-between md:col-span-2">
            <Label>Enabled</Label>
            <Switch checked={grenade.enabled} onCheckedChange={(c) => void update({ enabled: c })} />
          </div>
          <div>
            <Label>Arc color</Label>
            <Input type="color" value={grenade.arcColor.slice(0, 7)} onChange={(e) => void update({ arcColor: e.target.value })} />
          </div>
          <div>
            <Label>Point color</Label>
            <Input type="color" value={grenade.pointColor.slice(0, 7)} onChange={(e) => void update({ pointColor: e.target.value })} />
          </div>
          <div>
            <Label>Impact / bounce color</Label>
            <Input type="color" value={grenade.impactColor.slice(0, 7)} onChange={(e) => void update({ impactColor: e.target.value })} />
          </div>
          <div>
            <Label>Landing color</Label>
            <Input type="color" value={grenade.landingColor.slice(0, 7)} onChange={(e) => void update({ landingColor: e.target.value })} />
          </div>
          <div>
            <Label>Arc line width</Label>
            <Input type="number" step="0.1" value={grenade.arcLineWidth} onChange={(e) => void update({ arcLineWidth: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Landing line width</Label>
            <Input type="number" step="0.1" value={grenade.landingLineWidth} onChange={(e) => void update({ landingLineWidth: Number(e.target.value) })} />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export function SoundEspPage() {
  const { profile, saveProfile } = useProfile();
  if (!profile) return <div>Loading...</div>;

  const sound = profile.settings.soundEsp;

  const update = async (patch: Partial<typeof sound>) => {
    await saveProfile({
      ...profile,
      settings: {
        ...profile.settings,
        soundEsp: { ...sound, ...patch },
      },
    });
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Sound ESP</h1>
      <Card>
        <CardHeader><CardTitle>Sound Wave Rendering</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div className="flex items-center justify-between">
            <Label>Enabled</Label>
            <Switch checked={sound.enabled} onCheckedChange={(c) => void update({ enabled: c })} />
          </div>
          <div>
            <Label>Animation type</Label>
            <Select value={sound.animation} onValueChange={(v: "waves" | "staticBox") => void update({ animation: v })}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="waves">Waves</SelectItem>
                <SelectItem value="staticBox">Static box</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div>
            <Label>Wave color</Label>
            <Input type="color" value={sound.waveColor.slice(0, 7)} onChange={(e) => void update({ waveColor: e.target.value })} />
          </div>
          <div>
            <Label>Line width</Label>
            <Input type="number" step="0.1" value={sound.waveLineWidth} onChange={(e) => void update({ waveLineWidth: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Duration (ms)</Label>
            <Input type="number" value={sound.waveDurationMs} onChange={(e) => void update({ waveDurationMs: Number(e.target.value) })} />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
