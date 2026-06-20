import { useCallback, useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import { MobileHeader, Sidebar } from "@/components/layout/Sidebar";
import { api } from "@/api/client";
import type { ConfigProfile } from "@/types";

export interface ProfileContextValue {
  profile: ConfigProfile | null;
  refresh: () => Promise<void>;
  saveProfile: (profile: ConfigProfile) => Promise<void>;
}

import { createContext, useContext } from "react";

const ProfileContext = createContext<ProfileContextValue | null>(null);

export function useProfile() {
  const ctx = useContext(ProfileContext);
  if (!ctx) throw new Error("useProfile must be used within AppLayout");
  return ctx;
}

export function AppLayout() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [profile, setProfile] = useState<ConfigProfile | null>(null);

  const refresh = useCallback(async () => {
    const store = await api.getStore();
    const activeId = store.activeProfileId ?? store.defaultProfileId;
    const active = store.profiles.find((p) => p.id === activeId) ?? store.profiles[0];
    setProfile(active ?? null);
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const saveProfile = async (updated: ConfigProfile) => {
    const saved = await api.updateProfile(updated);
    setProfile(saved);
  };

  return (
    <ProfileContext.Provider value={{ profile, refresh, saveProfile }}>
      <div className="flex min-h-screen">
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        <div className="flex min-h-screen flex-1 flex-col">
          <MobileHeader onOpen={() => setSidebarOpen(true)} />
          <main className="flex-1 p-4 md:p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </ProfileContext.Provider>
  );
}
