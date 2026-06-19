import { NavLink } from "react-router-dom";
import {
  Crosshair,
  Eye,
  Gauge,
  Keyboard,
  LayoutDashboard,
  Menu,
  Palette,
  Radar,
  Settings2,
  Target,
  Volume2,
  X,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

const navItems = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/configs", label: "Profiles", icon: Settings2 },
  { to: "/triggerbot", label: "Triggerbot", icon: Target },
  { to: "/aimhelper", label: "Aim Helper", icon: Crosshair },
  { to: "/rcs", label: "RCS", icon: Gauge },
  { to: "/esp", label: "ESP", icon: Eye },
  { to: "/visuals", label: "Visuals", icon: Palette },
  { to: "/radar", label: "Radar", icon: Radar },
  { to: "/keybinds", label: "Keybinds", icon: Keyboard },
  { to: "/sound", label: "Sound ESP", icon: Volume2 },
];

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export function Sidebar({ open, onClose }: SidebarProps) {
  return (
    <>
      <div
        className={cn(
          "fixed inset-0 z-40 bg-black/50 lg:hidden",
          open ? "block" : "hidden"
        )}
        onClick={onClose}
      />
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex w-72 flex-col border-r border-[var(--color-border)] bg-[var(--color-card)] transition-transform lg:static lg:translate-x-0",
          open ? "translate-x-0" : "-translate-x-full"
        )}
      >
        <div className="flex items-center justify-between border-b border-[var(--color-border)] p-4">
          <div>
            <div className="text-xs uppercase tracking-wider text-[var(--color-muted-foreground)]">
              CS2 Toolkit
            </div>
            <div className="text-lg font-semibold">Configuration</div>
          </div>
          <Button variant="ghost" size="icon" className="lg:hidden" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        </div>
        <nav className="flex-1 space-y-1 p-3">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              end={to === "/"}
              onClick={onClose}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition-colors",
                  isActive
                    ? "bg-[var(--color-primary)] text-[var(--color-primary-foreground)]"
                    : "text-[var(--color-muted-foreground)] hover:bg-[var(--color-accent)] hover:text-[var(--color-foreground)]"
                )
              }
            >
              <Icon className="h-4 w-4" />
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>
    </>
  );
}

export function MobileHeader({ onOpen }: { onOpen: () => void }) {
  return (
    <header className="flex items-center gap-3 border-b border-[var(--color-border)] bg-[var(--color-card)] p-4 lg:hidden">
      <Button variant="ghost" size="icon" onClick={onOpen}>
        <Menu className="h-5 w-5" />
      </Button>
      <div className="font-semibold">CS2 Toolkit Config</div>
    </header>
  );
}
