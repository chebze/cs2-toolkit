import type { MapConfig, RadarPlayerSnapshot } from "@/types";
import { isOnRadarLevel, resolveRadarLevel, weaponIconUrl } from "@/lib/radar";

function PlayerCard({ player }: { player: RadarPlayerSnapshot }) {
  return (
    <li
      className={`flex items-center gap-2 rounded-lg border bg-[#0f1520] px-2 py-1.5 text-sm ${
        player.isLocalPlayer ? "border-[var(--color-primary)]" : "border-transparent"
      }`}
    >
      <img
        src={weaponIconUrl(player.activeWeapon)}
        alt=""
        className="h-7 w-7 shrink-0 object-contain"
        onError={(e) => {
          e.currentTarget.style.visibility = "hidden";
        }}
      />
      <div className="min-w-0 flex-1">
        <div className="truncate font-semibold">{player.name}</div>
        <div className="text-xs text-[var(--color-muted-foreground)]">
          {player.health} HP · {player.activeWeapon}
        </div>
      </div>
    </li>
  );
}

function TeamSection({
  title,
  className,
  players,
}: {
  title: string;
  className: string;
  players: RadarPlayerSnapshot[];
}) {
  if (players.length === 0) return null;

  return (
    <section className="mb-5">
      <h3 className={`mb-2 text-xs font-semibold uppercase tracking-wide ${className}`}>{title}</h3>
      <ul className="flex flex-col gap-1.5">
        {players.map((player) => (
          <PlayerCard key={player.id} player={player} />
        ))}
      </ul>
    </section>
  );
}

export function RadarPlayerList({
  players,
  mapConfig,
  referenceZ,
}: {
  players: RadarPlayerSnapshot[];
  mapConfig?: MapConfig | null;
  referenceZ?: number;
}) {
  const level = mapConfig && referenceZ !== undefined ? resolveRadarLevel(mapConfig, referenceZ) : null;
  const visiblePlayers =
    mapConfig && level
      ? players.filter((player) => isOnRadarLevel(mapConfig, player.z, level))
      : players;
  const tPlayers = visiblePlayers.filter((p) => p.team === 2).sort((a, b) => a.id - b.id);
  const ctPlayers = visiblePlayers.filter((p) => p.team === 3).sort((a, b) => a.id - b.id);

  return (
    <aside className="max-h-[80vh] overflow-y-auto rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4">
      <h2 className="mb-4 text-base font-semibold">Players</h2>
      <TeamSection title="Terrorists" className="text-[#ef4444]" players={tPlayers} />
      <TeamSection title="Counter-Terrorists" className="text-[#38bdf8]" players={ctPlayers} />
    </aside>
  );
}
