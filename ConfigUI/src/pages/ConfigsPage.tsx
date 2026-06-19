import { useEffect, useState } from "react";
import { api } from "@/api/client";
import type { ConfigProfile, ConfigStore } from "@/types";
import { useProfile } from "@/components/layout/AppLayout";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export function ConfigsPage() {
  const { refresh } = useProfile();
  const [store, setStore] = useState<ConfigStore | null>(null);
  const [newName, setNewName] = useState("");

  const load = async () => {
    setStore(await api.getStore());
    await refresh();
  };

  useEffect(() => {
    void load();
  }, []);

  if (!store) return <div>Loading profiles...</div>;

  const activeId = store.activeProfileId ?? store.defaultProfileId;

  const handleCreate = async () => {
    if (!newName.trim()) return;
    await api.createProfile(newName.trim());
    setNewName("");
    await load();
  };

  const handleActivate = async (id: string) => {
    await api.activateProfile(id);
    await load();
  };

  const handleDefault = async (id: string) => {
    await api.setDefaultProfile(id);
    await load();
  };

  const handleDelete = async (id: string) => {
    await api.deleteProfile(id);
    await load();
  };

  const handleExport = async (profile: ConfigProfile) => {
    const json = await api.exportProfile(profile.id);
    const blob = new Blob([json], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `${profile.name.replace(/\s+/g, "-").toLowerCase()}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const handleImport = async (file: File) => {
    const json = await file.text();
    await api.importProfile(json, file.name.replace(/\.json$/i, ""));
    await load();
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Configuration Profiles</h1>
        <p className="text-[var(--color-muted-foreground)]">
          Manage multiple configs, set defaults, and assign per-profile switch hotkeys.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Create Profile</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col gap-3 sm:flex-row">
          <Input
            placeholder="Profile name"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
          />
          <Button onClick={() => void handleCreate()}>Create</Button>
          <label className="inline-flex">
            <input
              type="file"
              accept="application/json"
              className="hidden"
              onChange={(e) => {
                const file = e.target.files?.[0];
                if (file) void handleImport(file);
              }}
            />
            <Button variant="secondary" asChild>
              <span>Import JSON</span>
            </Button>
          </label>
        </CardContent>
      </Card>

      <div className="grid gap-4">
        {store.profiles.map((profile) => (
          <Card key={profile.id}>
            <CardHeader>
              <CardTitle>{profile.name}</CardTitle>
              <CardDescription>
                {profile.id === store.defaultProfileId ? "Default on startup · " : ""}
                {profile.id === activeId ? "Active now" : "Inactive"}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid gap-2 sm:grid-cols-2">
                <div>
                  <Label>Switch hotkey</Label>
                  <Input
                    value={profile.switchHotkey ?? ""}
                    placeholder="e.g. F1"
                    onChange={async (e) => {
                      const updated = { ...profile, switchHotkey: e.target.value };
                      await api.updateProfile(updated);
                      await load();
                    }}
                  />
                </div>
              </div>
              <div className="flex flex-wrap gap-2">
                <Button
                  variant={profile.id === activeId ? "default" : "secondary"}
                  onClick={() => void handleActivate(profile.id)}
                >
                  Activate
                </Button>
                <Button variant="secondary" onClick={() => void handleDefault(profile.id)}>
                  Set Default
                </Button>
                <Button variant="secondary" onClick={() => void handleExport(profile)}>
                  Export
                </Button>
                <Button
                  variant="destructive"
                  disabled={store.profiles.length <= 1}
                  onClick={() => void handleDelete(profile.id)}
                >
                  Delete
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
