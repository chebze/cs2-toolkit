import { useEffect, useState } from "react";
import type { MapsConfig, RadarSnapshot } from "@/types";

export function useRadarStream() {
  const [snapshot, setSnapshot] = useState<RadarSnapshot | null>(null);
  const [mapsConfig, setMapsConfig] = useState<MapsConfig>({ maps: [] });
  const [connectionStatus, setConnectionStatus] = useState("Connecting…");

  useEffect(() => {
    let cancelled = false;
    let source: EventSource | null = null;
    let retryTimer: number | undefined;

    const connect = () => {
      source = new EventSource("/api/radar/stream");
      source.onmessage = (event) => {
        try {
          const next = JSON.parse(event.data) as RadarSnapshot;
          if (!cancelled) {
            setSnapshot(next);
            setConnectionStatus("connected");
          }
        } catch (err) {
          console.error("Failed to parse radar snapshot", err);
        }
      };
      source.onerror = () => {
        if (cancelled) return;
        setConnectionStatus("Connection lost — retrying…");
        source?.close();
        retryTimer = window.setTimeout(connect, 2000);
      };
    };

    void (async () => {
      try {
        const [mapsRes, snapshotRes] = await Promise.all([
          fetch("/radar/maps.json"),
          fetch("/api/radar/snapshot"),
        ]);
        if (cancelled) return;
        setMapsConfig((await mapsRes.json()) as MapsConfig);
        setSnapshot((await snapshotRes.json()) as RadarSnapshot);
        setConnectionStatus("connected");
      } catch {
        if (!cancelled) setConnectionStatus("Unable to reach radar API");
      }
      if (!cancelled) connect();
    })();

    return () => {
      cancelled = true;
      source?.close();
      if (retryTimer !== undefined) window.clearTimeout(retryTimer);
    };
  }, []);

  return { snapshot, mapsConfig, connectionStatus };
}
