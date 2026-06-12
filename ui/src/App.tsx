import { Navigate, Route, Routes } from "react-router-dom";
import { TooltipProvider } from "@/components/ui/tooltip";
import { SessionProvider } from "./lib/SessionContext";
import { CommandPaletteHost } from "./components/CommandPaletteHost";
import { RequireAuth } from "./components/routing/RequireAuth";
import { LoginRoute } from "./components/routing/LoginRoute";
import { AdminLayout } from "./layouts/AdminLayout";
import { PlayerLayout } from "./layouts/PlayerLayout";

export default function App() {
  return (
    <SessionProvider>
      <TooltipProvider delayDuration={200}>
        <CommandPaletteHost>
          <Routes>
            <Route path="/" element={<Navigate to="/admin/overview" replace />} />
            <Route path="/login" element={<LoginRoute />} />
            <Route path="/admin/*" element={<RequireAuth><AdminLayout /></RequireAuth>} />
            <Route path="/player/*" element={<RequireAuth><PlayerLayout /></RequireAuth>} />
            <Route path="*" element={<Navigate to="/admin/overview" replace />} />
          </Routes>
        </CommandPaletteHost>
      </TooltipProvider>
    </SessionProvider>
  );
}
