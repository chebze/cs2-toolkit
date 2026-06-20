import type {
  ConfigProfile,
  ConfigStore,
  DashboardData,
  GlobalKeybinds,
} from "@/types";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: { "Content-Type": "application/json", ...(init?.headers ?? {}) },
    ...init,
  });

  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `Request failed: ${response.status}`);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export const api = {
  getDashboard: () => request<DashboardData>("/api/dashboard"),
  getStore: () => request<ConfigStore>("/api/configs"),
  getProfile: (id: string) => request<ConfigProfile>(`/api/configs/${id}`),
  createProfile: (name: string) =>
    request<ConfigProfile>("/api/configs", {
      method: "POST",
      body: JSON.stringify({ name }),
    }),
  updateProfile: (profile: ConfigProfile) =>
    request<ConfigProfile>(`/api/configs/${profile.id}`, {
      method: "PUT",
      body: JSON.stringify(profile),
    }),
  deleteProfile: (id: string) =>
    request<void>(`/api/configs/${id}`, { method: "DELETE" }),
  activateProfile: (id: string) =>
    request<void>(`/api/configs/${id}/activate`, { method: "POST" }),
  setDefaultProfile: (id: string) =>
    request<void>(`/api/configs/${id}/default`, { method: "POST" }),
  exportProfile: async (id: string) => {
    const response = await fetch(`/api/configs/${id}/export`);
    if (!response.ok) throw new Error("Export failed");
    return response.text();
  },
  importProfile: (json: string, name?: string) => {
    const query = name ? `?name=${encodeURIComponent(name)}` : "";
    return request<ConfigProfile>(`/api/configs/import${query}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: json,
    });
  },
  getKeybinds: () => request<GlobalKeybinds>("/api/keybinds"),
  updateKeybinds: (keybinds: GlobalKeybinds) =>
    request<void>("/api/keybinds", {
      method: "PUT",
      body: JSON.stringify(keybinds),
    }),
  getWeapons: () =>
    request<Array<{ id: number; name: string; category: string }>>("/api/weapons"),
};
