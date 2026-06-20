import type { MapConfig, RadarBombSnapshot, RadarPlayerSnapshot } from "@/types";
import {
  MAP_SIZE,
  mapDisplayName,
  mapImageUrl,
  resolveRadarLevel,
  isOnRadarLevel,
  worldToRadar,
  type RadarLevel,
} from "@/lib/radar";

interface RadarMapProps {
  mapName: string;
  mapConfig: MapConfig | null;
  players: RadarPlayerSnapshot[];
  bomb: RadarBombSnapshot;
  referenceZ: number;
  showFov: boolean;
  showBomb: boolean;
}

function FovCone({
  x,
  y,
  yaw,
  color,
}: {
  x: number;
  y: number;
  yaw: number;
  color: string;
}) {
  const yawRad = (yaw * Math.PI) / 180;
  const coneLen = 80;
  const halfAngle = (30 * Math.PI) / 180;
  const x1 = x + Math.cos(yawRad - halfAngle) * coneLen;
  const y1 = y - Math.sin(yawRad - halfAngle) * coneLen;
  const x2 = x + Math.cos(yawRad + halfAngle) * coneLen;
  const y2 = y - Math.sin(yawRad + halfAngle) * coneLen;

  return (
    <path
      d={`M ${x} ${y} L ${x1} ${y1} A ${coneLen} ${coneLen} 0 0 1 ${x2} ${y2} Z`}
      fill={color}
      opacity={0.25}
    />
  );
}

export function RadarMap({
  mapName,
  mapConfig,
  players,
  bomb,
  referenceZ,
  showFov,
  showBomb,
}: RadarMapProps) {
  const level: RadarLevel = resolveRadarLevel(mapConfig, referenceZ);
  const visiblePlayers = mapConfig
    ? players.filter((player) => isOnRadarLevel(mapConfig, player.z, level))
    : players;
  const showBombMarker =
    showBomb &&
    bomb.planted &&
    (mapConfig ? isOnRadarLevel(mapConfig, bomb.z, level) : true);

  return (
    <div className="min-w-0">
      <div className="mb-2 text-xs font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
        {mapDisplayName(mapName, level)}
      </div>
      <div className="relative aspect-square w-full overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)]">
        {mapConfig ? (
          <>
            <img
              src={mapImageUrl(mapName, level)}
              alt={`${mapDisplayName(mapName, level)} radar`}
              className="block h-full w-full bg-[#0a1018] object-contain"
            />
            <svg
              className="pointer-events-none absolute inset-0 h-full w-full"
              viewBox={`0 0 ${MAP_SIZE} ${MAP_SIZE}`}
              preserveAspectRatio="none"
            >
              {showBombMarker && (() => {
                const pos = worldToRadar(
                  bomb.x,
                  bomb.y,
                  mapConfig.pos_x,
                  mapConfig.pos_y,
                  mapConfig.scale,
                  MAP_SIZE,
                  MAP_SIZE
                );
                return (
                  <circle
                    cx={pos.x}
                    cy={pos.y}
                    r={10}
                    className="animate-pulse fill-[#fbbf24] stroke-[#ef4444] stroke-2"
                  />
                );
              })()}
              {visiblePlayers.map((player) => {
                const pos = worldToRadar(
                  player.x,
                  player.y,
                  mapConfig.pos_x,
                  mapConfig.pos_y,
                  mapConfig.scale,
                  MAP_SIZE,
                  MAP_SIZE
                );
                const color = player.team === 2 ? "#ef4444" : "#38bdf8";

                return (
                  <g key={player.id}>
                    {showFov && <FovCone x={pos.x} y={pos.y} yaw={player.yaw} color={color} />}
                    <circle
                      cx={pos.x}
                      cy={pos.y}
                      r={player.isLocalPlayer ? 8 : 6}
                      fill={color}
                      stroke={player.isLocalPlayer ? "#fff" : "rgba(0,0,0,0.4)"}
                      strokeWidth={player.isLocalPlayer ? 2 : 1}
                    />
                  </g>
                );
              })}
            </svg>
          </>
        ) : (
          <div className="flex h-full items-center justify-center p-6 text-center text-[var(--color-muted-foreground)]">
            No radar image for {mapName}
          </div>
        )}
      </div>
    </div>
  );
}
