import { ServerCog } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { RuntimeServiceStatus } from "../../types/admin";
import { statusAccentClass } from "./adminUi";
import { Panel } from "./Panel";

type AdminRuntimePanelProps = {
  services: RuntimeServiceStatus[];
};

export function AdminRuntimePanel({ services }: AdminRuntimePanelProps) {
  return (
    <Panel title="Runtime services" icon={ServerCog}>
      <div className="grid gap-2">
        {services.map((service) => (
          <div
            key={service.id}
            className="flex min-h-[48px] items-center gap-3 rounded-md bg-muted px-3 py-2 transition-colors duration-150 hover:bg-border/40"
          >
            <span className={`h-2 w-2 shrink-0 rounded-full ${statusAccentClass[service.status]}`} aria-hidden />
            <div className="min-w-0 flex-1">
              <strong className="block truncate text-sm font-semibold text-fg">{service.label}</strong>
              <small className="block truncate text-xs text-fg-muted">{service.secondary}</small>
            </div>
            <Badge variant="outline" className="shrink-0 rounded-md border-border bg-surface font-mono text-xs font-semibold text-fg">
              {service.primary}
            </Badge>
          </div>
        ))}
      </div>
    </Panel>
  );
}
