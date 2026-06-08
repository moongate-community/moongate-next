import { Activity, Database, ServerCog, ShieldCheck } from "lucide-react";

const systemMetrics = [
  {
    label: "Shard state",
    value: "Online",
    detail: "HTTP and game loop active"
  },
  {
    label: "Sessions",
    value: "0",
    detail: "No active players"
  },
  {
    label: "Data services",
    value: "Lazy",
    detail: "Loaded on first query"
  },
  {
    label: "Auth",
    value: "JWT",
    detail: "Refresh rotation enabled"
  }
];

const operations = [
  "World data seeding",
  "Persistence journal",
  "Lua command registry",
  "Map and item image cache"
];

export function AdminDashboard() {
  return (
    <section className="workspace admin-dashboard">
      <header className="dashboard-header">
        <div>
          <h2>Admin dashboard</h2>
          <p>Operational control surface for server health, data pipelines, auth, and shard runtime state.</p>
        </div>
        <div className="status-pill">
          <Activity size={16} aria-hidden />
          Live
        </div>
      </header>

      <div className="admin-metrics">
        {systemMetrics.map((metric) => (
          <article key={metric.label}>
            <span>{metric.label}</span>
            <strong>{metric.value}</strong>
            <small>{metric.detail}</small>
          </article>
        ))}
      </div>

      <div className="admin-grid">
        <article className="ops-panel">
          <header>
            <ServerCog size={20} aria-hidden />
            <h3>Runtime services</h3>
          </header>
          <div className="ops-list">
            {operations.map((item) => (
              <div key={item} className="ops-row">
                <ShieldCheck size={16} aria-hidden />
                <span>{item}</span>
                <strong>Ready</strong>
              </div>
            ))}
          </div>
        </article>

        <article className="data-panel">
          <header>
            <Database size={20} aria-hidden />
            <h3>Data pipeline</h3>
          </header>
          <dl>
            <div>
              <dt>World assets</dt>
              <dd>Embedded YAML seeded into runtime data</dd>
            </div>
            <div>
              <dt>Image cache</dt>
              <dd>`cache/images/maps` and `cache/images/items`</dd>
            </div>
            <div>
              <dt>Config source</dt>
              <dd>`moongate.yaml` with web JWT section</dd>
            </div>
          </dl>
        </article>
      </div>
    </section>
  );
}
