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

export function EspPage() {
  const { profile, saveProfile } = useProfile();
  if (!profile) return <div>Loading...</div>;

  const esp = profile.settings.enemyEsp;

  const update = async (patch: Partial<typeof esp>) => {
    await saveProfile({
      ...profile,
      settings: {
        ...profile.settings,
        enemyEsp: { ...esp, ...patch },
      },
    });
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Enemy ESP</h1>
      <Card>
        <CardHeader><CardTitle>Display</CardTitle></CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div>
            <Label>Mode</Label>
            <Select value={esp.mode} onValueChange={(v) => void update({ mode: v })}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                {["Disabled", "LastSeen", "Full"].map((mode) => (
                  <SelectItem key={mode} value={mode}>{mode}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex items-center justify-between">
            <Label>Player name</Label>
            <Switch checked={esp.showPlayerName} onCheckedChange={(c) => void update({ showPlayerName: c })} />
          </div>
          <div className="flex items-center justify-between">
            <Label>Player health</Label>
            <Switch checked={esp.showPlayerHealth} onCheckedChange={(c) => void update({ showPlayerHealth: c })} />
          </div>
          <div className="flex items-center justify-between">
            <Label>Bounding box</Label>
            <Switch checked={esp.showBoundingBox} onCheckedChange={(c) => void update({ showBoundingBox: c })} />
          </div>
          <div>
            <Label>Skeleton color</Label>
            <Input type="color" value={esp.skeletonColor.slice(0, 7)} onChange={(e) => void update({ skeletonColor: e.target.value })} />
          </div>
          <div>
            <Label>Bounding box color</Label>
            <Input type="color" value={esp.boundingBoxColor.slice(0, 7)} onChange={(e) => void update({ boundingBoxColor: e.target.value })} />
          </div>
          <div>
            <Label>Skeleton line width</Label>
            <Input type="number" step="0.1" value={esp.skeletonLineWidth} onChange={(e) => void update({ skeletonLineWidth: Number(e.target.value) })} />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
