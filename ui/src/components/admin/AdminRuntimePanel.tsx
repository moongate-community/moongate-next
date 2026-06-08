import { ServerCog } from "lucide-react";
import type { RuntimeServiceStatus } from "../../types/admin";

type AdminRuntimePanelProps = {
  services: RuntimeServiceStatus[];
};

export function AdminRuntimePanel({ services }: AdminRuntimePanelProps) {
  return (
    <article className="admin-panel admin-runtime-panel">
      <header>
        <ServerCog size={20} aria-hidden />
        <h3>Runtime services</h3>
      </header>
      <div className="admin-service-list">
        {services.map((service) => (
          <div key={service.id} className={`admin-service-row admin-status-${service.status}`}>
            <span className="admin-status-dot" aria-hidden />
            <div>
              <strong>{service.label}</strong>
              <small>{service.secondary}</small>
            </div>
            <b>{service.primary}</b>
          </div>
        ))}
      </div>
    </article>
  );
}
