import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export function RadarPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Live Radar</h1>
        <p className="text-[var(--color-muted-foreground)]">
          Share the radar with friends using your public tunnel URL + <code>/radar</code>
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Web Radar</CardTitle>
          <CardDescription>
            Full-screen live minimap with player names, health, weapons, and bomb position.
            Updates automatically when you enter a match.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-3">
          <Button asChild>
            <a href="/radar/index.html" target="_blank" rel="noreferrer">
              Open radar
            </a>
          </Button>
          <Button variant="secondary" asChild>
            <a href="/radar/index.html">Open in this tab</a>
          </Button>
        </CardContent>
      </Card>

      <div className="overflow-hidden rounded-lg border border-[var(--color-border)]" style={{ height: "70vh" }}>
        <iframe
          src="/radar/index.html"
          title="Live radar"
          className="h-full w-full border-0 bg-[var(--color-background)]"
        />
      </div>
    </div>
  );
}
