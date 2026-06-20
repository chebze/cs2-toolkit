import { Link } from "react-router-dom";
import { useState } from "react";
import { useRadarStream } from "@/hooks/useRadarStream";
import { getMapConfig, mapDisplayName, normalizeMapName, resolveRadarLevel } from "@/lib/radar";
import { RadarMap } from "@/components/radar/RadarMap";
import { RadarPlayerList } from "@/components/radar/RadarPlayerList";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";

function IdleState({ title, subtitle }: { title: string; subtitle: string }) {
  return (
    <div className="flex min-h-[60vh] items-center justify-center">
      <Card className="max-w-md text-center">
        <CardHeader>
          <CardTitle>{title}</CardTitle>
          <CardDescription>{subtitle}</CardDescription>
        </CardHeader>
      </Card>
    </div>
  );
}

function getStatusText(
  snapshot: NonNullable<ReturnType<typeof useRadarStream>["snapshot"]>,
  connectionStatus: string,
  mapsConfig: ReturnType<typeof useRadarStream>["mapsConfig"]
) {
  if (!snapshot.attached) return "Waiting for game connection…";
  if (!snapshot.inMatch) return "Attached — not in a live match";
  if (connectionStatus !== "connected") return connectionStatus;
  const map = snapshot.map ? normalizeMapName(snapshot.map) : "Unknown";
  const mapConfig = snapshot.map ? getMapConfig(mapsConfig.maps, snapshot.map) : null;
  const localPlayer = snapshot.players.find((player) => player.isLocalPlayer);
  const referenceZ = localPlayer?.z ?? snapshot.players[0]?.z ?? 0;
  const level = mapConfig ? resolveRadarLevel(mapConfig, referenceZ) : "upper";
  return `Live — ${mapDisplayName(map, level)} · ${snapshot.players.length} players`;
}

export function RadarView({ showBackLink = true }: { showBackLink?: boolean }) {
  const { snapshot, mapsConfig, connectionStatus } = useRadarStream();
  const [showFov, setShowFov] = useState(true);
  const [showBomb, setShowBomb] = useState(true);

  const statusText = snapshot ? getStatusText(snapshot, connectionStatus, mapsConfig) : connectionStatus;

  return (
    <div className="mx-auto max-w-[1400px] space-y-4 p-4 md:p-6">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Live Radar</h1>
          <p className="text-sm text-[var(--color-muted-foreground)]">{statusText}</p>
        </div>
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2">
            <Switch id="show-fov" checked={showFov} onCheckedChange={setShowFov} />
            <Label htmlFor="show-fov" className="text-sm text-[var(--color-muted-foreground)]">
              FOV cones
            </Label>
          </div>
          <div className="flex items-center gap-2">
            <Switch id="show-bomb" checked={showBomb} onCheckedChange={setShowBomb} />
            <Label htmlFor="show-bomb" className="text-sm text-[var(--color-muted-foreground)]">
              Bomb
            </Label>
          </div>
          {showBackLink && (
            <Button variant="secondary" size="sm" asChild>
              <Link to="/">Back to config</Link>
            </Button>
          )}
        </div>
      </header>

      {!snapshot ? (
        <Card>
          <CardContent className="py-10 text-center text-[var(--color-muted-foreground)]">
            Loading radar…
          </CardContent>
        </Card>
      ) : !snapshot.attached ? (
        <IdleState
          title="Not connected to CS2"
          subtitle="Launch the toolkit and press F9 to attach when in-game."
        />
      ) : !snapshot.inMatch ? (
        <IdleState
          title="Not currently in a match"
          subtitle="Enter a competitive, casual, or deathmatch game to see the radar."
        />
      ) : (
        <div className="grid gap-4 lg:grid-cols-[1fr_320px]">
          <RadarMap
            mapName={normalizeMapName(snapshot.map ?? "unknown")}
            mapConfig={snapshot.map ? getMapConfig(mapsConfig.maps, snapshot.map) : null}
            players={snapshot.players}
            bomb={snapshot.bomb}
            referenceZ={snapshot.players.find((player) => player.isLocalPlayer)?.z ?? snapshot.players[0]?.z ?? 0}
            showFov={showFov}
            showBomb={showBomb}
          />
          <RadarPlayerList
            players={snapshot.players}
            mapConfig={snapshot.map ? getMapConfig(mapsConfig.maps, snapshot.map) : null}
            referenceZ={snapshot.players.find((player) => player.isLocalPlayer)?.z ?? snapshot.players[0]?.z ?? 0}
          />
        </div>
      )}
    </div>
  );
}
