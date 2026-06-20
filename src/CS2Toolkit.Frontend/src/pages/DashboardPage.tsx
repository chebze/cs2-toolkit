import { useEffect, useState } from "react";
import { api } from "@/api/client";
import type { DashboardData } from "@/types";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = () =>
      api.getDashboard()
        .then(setData)
        .catch((err) => setError(err instanceof Error ? err.message : "Failed to load dashboard"));

    void load();
    const timer = window.setInterval(() => void load(), 3000);
    return () => window.clearInterval(timer);
  }, []);

  if (error) {
    return <div className="text-[var(--color-destructive)]">{error}</div>;
  }

  if (!data) {
    return <div className="text-[var(--color-muted-foreground)]">Loading dashboard...</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="text-[var(--color-muted-foreground)]">
          Live configuration for the CS2 Toolkit. Changes apply instantly in-game.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Active Profile</CardTitle>
            <CardDescription>Currently loaded configuration</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-semibold">{data.activeProfile.name}</div>
            {data.activeProfile.switchHotkey && (
              <p className="mt-2 text-sm text-[var(--color-muted-foreground)]">
                Switch hotkey: {data.activeProfile.switchHotkey}
              </p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Web Port</CardTitle>
            <CardDescription>Config UI listening port</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-semibold">{data.webPort}</div>
          </CardContent>
        </Card>

        <Card className="md:col-span-2 xl:col-span-2">
          <CardHeader>
            <CardTitle>Live Radar</CardTitle>
            <CardDescription>Shareable minimap with player positions, health, and weapons</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="flex items-center gap-2">
              <code className="flex-1 rounded bg-[var(--color-muted)] px-2 py-1 text-xs">
                {window.location.origin}{data.radarUrl}
              </code>
              <Button
                variant="secondary"
                size="sm"
                onClick={() => navigator.clipboard.writeText(`${window.location.origin}${data.radarUrl}`)}
              >
                Copy
              </Button>
              <Button variant="secondary" size="sm" asChild>
                <a href={data.radarUrl} target="_blank" rel="noreferrer">Open</a>
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card className="md:col-span-2 xl:col-span-2">
          <CardHeader>
            <CardTitle>Local Network URLs</CardTitle>
            <CardDescription>Use these from your phone or another device on the same network</CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            {data.accessUrls.map((url) => (
              <div key={url} className="flex items-center gap-2">
                <code className="flex-1 rounded bg-[var(--color-muted)] px-2 py-1 text-xs">{url}</code>
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => navigator.clipboard.writeText(url)}
                >
                  Copy
                </Button>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
