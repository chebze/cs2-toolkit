const MAP_SIZE = 1024;
let mapsConfig = { maps: [] };
let showFov = true;
let showBomb = true;

const els = {
  status: document.getElementById("status-text"),
  idlePanel: document.getElementById("idle-panel"),
  matchPanel: document.getElementById("match-panel"),
  idleTitle: document.getElementById("idle-title"),
  idleSubtitle: document.getElementById("idle-subtitle"),
  mapTitle: document.getElementById("map-title"),
  mapImage: document.getElementById("map-image"),
  overlay: document.getElementById("radar-overlay"),
  playersT: document.getElementById("players-t"),
  playersCt: document.getElementById("players-ct"),
};

document.getElementById("show-fov").addEventListener("change", (e) => {
  showFov = e.target.checked;
});
document.getElementById("show-bomb").addEventListener("change", (e) => {
  showBomb = e.target.checked;
});

function worldToRadar(worldX, worldY, posX, posY, scale, width, height) {
  const px = (worldX - posX) / scale;
  const py = (posY - worldY) / scale;
  const x = Math.max(0, Math.min(width, (px / MAP_SIZE) * width));
  const y = Math.max(0, Math.min(height, (py / MAP_SIZE) * height));
  return { x, y };
}

function getMapConfig(mapName) {
  return mapsConfig.maps.find((m) => m.map_name === mapName) ?? null;
}

function weaponIcon(weaponName) {
  return `/radar/weapons/${weaponName.toLowerCase()}.svg`;
}

function setIdle(title, subtitle) {
  els.idleTitle.textContent = title;
  els.idleSubtitle.textContent = subtitle;
  els.idlePanel.classList.remove("hidden");
  els.matchPanel.classList.add("hidden");
}

function renderPlayerList(players) {
  const tPlayers = players.filter((p) => p.team === 2).sort((a, b) => a.id - b.id);
  const ctPlayers = players.filter((p) => p.team === 3).sort((a, b) => a.id - b.id);

  const renderTeam = (listEl, teamPlayers) => {
    listEl.innerHTML = "";
    for (const p of teamPlayers) {
      const li = document.createElement("li");
      li.className = `player-card${p.isLocalPlayer ? " local" : ""}`;
      li.innerHTML = `
        <img src="${weaponIcon(p.activeWeapon)}" alt="" onerror="this.style.visibility='hidden'" />
        <div class="player-info">
          <div class="player-name">${escapeHtml(p.name)}</div>
          <div class="player-meta">${p.health} HP · ${p.activeWeapon}</div>
        </div>`;
      listEl.appendChild(li);
    }
  };

  renderTeam(els.playersT, tPlayers);
  renderTeam(els.playersCt, ctPlayers);
}

function escapeHtml(text) {
  const d = document.createElement("div");
  d.textContent = text;
  return d.innerHTML;
}

function renderOverlay(snapshot, mapConfig) {
  const svg = els.overlay;
  svg.innerHTML = "";

  const w = MAP_SIZE;
  const h = MAP_SIZE;
  const { pos_x: posX, pos_y: posY, scale } = mapConfig;

  if (showBomb && snapshot.bomb?.planted && snapshot.bomb.x !== undefined) {
    const pos = worldToRadar(snapshot.bomb.x, snapshot.bomb.y, posX, posY, scale, w, h);
    const bomb = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    bomb.setAttribute("cx", String(pos.x));
    bomb.setAttribute("cy", String(pos.y));
    bomb.setAttribute("class", "bomb-marker");
    svg.appendChild(bomb);
  }

  for (const player of snapshot.players) {
    const pos = worldToRadar(player.x, player.y, posX, posY, scale, w, h);
    const color = player.team === 2 ? "#ef4444" : "#38bdf8";

    if (showFov) {
      const yaw = (player.yaw * Math.PI) / 180;
      const coneLen = 80;
      const halfAngle = (30 * Math.PI) / 180;
      const x1 = pos.x + Math.cos(yaw - halfAngle) * coneLen;
      const y1 = pos.y - Math.sin(yaw - halfAngle) * coneLen;
      const x2 = pos.x + Math.cos(yaw + halfAngle) * coneLen;
      const y2 = pos.y - Math.sin(yaw + halfAngle) * coneLen;
      const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
      path.setAttribute(
        "d",
        `M ${pos.x} ${pos.y} L ${x1} ${y1} A ${coneLen} ${coneLen} 0 0 1 ${x2} ${y2} Z`
      );
      path.setAttribute("fill", color);
      path.setAttribute("opacity", "0.25");
      svg.appendChild(path);
    }

    const circle = document.createElementNS("http://www.w3.org/2000/svg", "circle");
    circle.setAttribute("cx", String(pos.x));
    circle.setAttribute("cy", String(pos.y));
    circle.setAttribute("r", player.isLocalPlayer ? "8" : "6");
    circle.setAttribute("fill", color);
    circle.setAttribute("stroke", player.isLocalPlayer ? "#fff" : "rgba(0,0,0,0.4)");
    circle.setAttribute("stroke-width", player.isLocalPlayer ? "2" : "1");
    svg.appendChild(circle);
  }
}

function renderSnapshot(snapshot) {
  if (!snapshot.attached) {
    els.status.textContent = "Waiting for game connection…";
    setIdle("Not connected to CS2", "Launch the toolkit and press F9 to attach when in-game.");
    return;
  }

  if (!snapshot.inMatch) {
    els.status.textContent = "Attached — not in a live match";
    setIdle("Not currently in a match", "Enter a competitive, casual, or deathmatch game to see the radar.");
    return;
  }

  els.idlePanel.classList.add("hidden");
  els.matchPanel.classList.remove("hidden");
  els.status.textContent = `Live — ${snapshot.map} · ${snapshot.players.length} players`;

  const mapConfig = getMapConfig(snapshot.map);
  els.mapTitle.textContent = snapshot.map ?? "Unknown map";

  if (mapConfig) {
    els.mapImage.src = `/radar/maps/${snapshot.map}_radar_psd.png`;
    els.mapImage.style.display = "block";
    renderOverlay(snapshot, mapConfig);
  } else {
    els.mapImage.style.display = "none";
    els.overlay.innerHTML = `<text x="512" y="512" text-anchor="middle" fill="#94a3b8" font-size="24">No radar image for ${snapshot.map}</text>`;
  }

  renderPlayerList(snapshot.players);
}

async function loadMapsConfig() {
  const res = await fetch("/radar/maps.json");
  mapsConfig = await res.json();
}

function connectStream() {
  const source = new EventSource("/api/radar/stream");
  source.onmessage = (event) => {
    try {
      const snapshot = JSON.parse(event.data);
      renderSnapshot(snapshot);
    } catch (err) {
      console.error("Failed to parse radar snapshot", err);
    }
  };
  source.onerror = () => {
    els.status.textContent = "Connection lost — retrying…";
    source.close();
    setTimeout(connectStream, 2000);
  };
}

loadMapsConfig().then(() => {
  fetch("/api/radar/snapshot")
    .then((r) => r.json())
    .then(renderSnapshot)
    .catch(() => setIdle("Not currently in a match", "Unable to reach radar API."));
  connectStream();
});
