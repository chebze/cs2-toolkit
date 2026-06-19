import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "@/components/layout/AppLayout";
import { DashboardPage } from "@/pages/DashboardPage";
import { ConfigsPage } from "@/pages/ConfigsPage";
import { TriggerbotPage } from "@/pages/TriggerbotPage";
import { AimHelperPage } from "@/pages/AimHelperPage";
import { RcsPage } from "@/pages/RcsPage";
import { EspPage } from "@/pages/EspPage";
import { VisualsPage, SoundEspPage } from "@/pages/VisualsPage";
import { KeybindsPage } from "@/pages/KeybindsPage";
import { RadarPage } from "@/pages/RadarPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route index element={<DashboardPage />} />
          <Route path="configs" element={<ConfigsPage />} />
          <Route path="triggerbot" element={<TriggerbotPage />} />
          <Route path="aimhelper" element={<AimHelperPage />} />
          <Route path="rcs" element={<RcsPage />} />
          <Route path="esp" element={<EspPage />} />
          <Route path="visuals" element={<VisualsPage />} />
          <Route path="sound" element={<SoundEspPage />} />
          <Route path="keybinds" element={<KeybindsPage />} />
          <Route path="radar" element={<RadarPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
