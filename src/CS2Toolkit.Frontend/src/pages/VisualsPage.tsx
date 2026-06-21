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
  const bulletTracers = profile.settings.visuals.bulletTracers ?? {
    enabled: false,
    showLocal: true,
    showTeammates: true,
    showEnemies: true,
    localColor: "#FF4444",
    teammateColor: "#44AAFF",
    enemyColor: "#FF8800",
    lineWidth: 1.5,
    durationMs: 800,
    maxDistanceUnits: 5000,
    maxActiveTracers: 64,
  };

  const updateGrenade = async (patch: Partial<typeof grenade>) => {
    await saveProfile({
      ...profile,
      settings: {
        ...profile.settings,
        visuals: {
          ...profile.settings.visuals,
          grenade: { ...grenade, ...patch },
        },
      },
    });
  };

  const updateBulletTracers = async (patch: Partial<typeof bulletTracers>) => {
    await saveProfile({
      ...profile,
      settings: {
        ...profile.settings,
        visuals: {
          ...profile.settings.visuals,
          bulletTracers: { ...bulletTracers, ...patch },
        },
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
            <Switch checked={grenade.enabled} onCheckedChange={(c) => void updateGrenade({ enabled: c })} />
          </div>
          <div>
            <Label>Arc color</Label>
            <Input type="color" value={grenade.arcColor.slice(0, 7)} onChange={(e) => void updateGrenade({ arcColor: e.target.value })} />
          </div>
          <div>
            <Label>Point color</Label>
            <Input type="color" value={grenade.pointColor.slice(0, 7)} onChange={(e) => void updateGrenade({ pointColor: e.target.value })} />
          </div>
          <div>
            <Label>Impact / bounce color</Label>
            <Input type="color" value={grenade.impactColor.slice(0, 7)} onChange={(e) => void updateGrenade({ impactColor: e.target.value })} />
          </div>
          <div>
            <Label>Landing color</Label>
            <Input type="color" value={grenade.landingColor.slice(0, 7)} onChange={(e) => void updateGrenade({ landingColor: e.target.value })} />
          </div>
          <div>
            <Label>Arc line width</Label>
            <Input type="number" step="0.1" value={grenade.arcLineWidth} onChange={(e) => void updateGrenade({ arcLineWidth: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Landing line width</Label>
            <Input type="number" step="0.1" value={grenade.landingLineWidth} onChange={(e) => void updateGrenade({ landingLineWidth: Number(e.target.value) })} />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><CardTitle>Bullet Tracers</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div className="flex items-center justify-between md:col-span-2">
            <Label>Enabled (default on profile load)</Label>
            <Switch checked={bulletTracers.enabled} onCheckedChange={(c) => void updateBulletTracers({ enabled: c })} />
          </div>
          <div className="flex items-center justify-between">
            <Label>Show local player</Label>
            <Switch checked={bulletTracers.showLocal} onCheckedChange={(c) => void updateBulletTracers({ showLocal: c })} />
          </div>
          <div className="flex items-center justify-between">
            <Label>Show teammates</Label>
            <Switch checked={bulletTracers.showTeammates} onCheckedChange={(c) => void updateBulletTracers({ showTeammates: c })} />
          </div>
          <div className="flex items-center justify-between md:col-span-2">
            <Label>Show enemies</Label>
            <Switch checked={bulletTracers.showEnemies} onCheckedChange={(c) => void updateBulletTracers({ showEnemies: c })} />
          </div>
          <div>
            <Label>Local color</Label>
            <Input type="color" value={bulletTracers.localColor.slice(0, 7)} onChange={(e) => void updateBulletTracers({ localColor: e.target.value })} />
          </div>
          <div>
            <Label>Teammate color</Label>
            <Input type="color" value={bulletTracers.teammateColor.slice(0, 7)} onChange={(e) => void updateBulletTracers({ teammateColor: e.target.value })} />
          </div>
          <div>
            <Label>Enemy color</Label>
            <Input type="color" value={bulletTracers.enemyColor.slice(0, 7)} onChange={(e) => void updateBulletTracers({ enemyColor: e.target.value })} />
          </div>
          <div>
            <Label>Line width</Label>
            <Input type="number" step="0.1" value={bulletTracers.lineWidth} onChange={(e) => void updateBulletTracers({ lineWidth: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Duration (ms)</Label>
            <Input type="number" value={bulletTracers.durationMs} onChange={(e) => void updateBulletTracers({ durationMs: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Max distance (units)</Label>
            <Input type="number" value={bulletTracers.maxDistanceUnits} onChange={(e) => void updateBulletTracers({ maxDistanceUnits: Number(e.target.value) })} />
          </div>
          <div>
            <Label>Max active tracers</Label>
            <Input type="number" value={bulletTracers.maxActiveTracers} onChange={(e) => void updateBulletTracers({ maxActiveTracers: Number(e.target.value) })} />
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
