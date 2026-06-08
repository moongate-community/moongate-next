import type { AdminActivityEvent } from "../../types/admin";

type AdminActivityTimelineProps = {
  events: AdminActivityEvent[];
};

export function AdminActivityTimeline({ events }: AdminActivityTimelineProps) {
  return (
    <article className="admin-panel admin-activity-panel">
      <header>
        <h3>Activity</h3>
      </header>
      <div className="admin-activity-list">
        {events.length === 0 ? (
          <p className="admin-empty-state">No dashboard activity yet.</p>
        ) : (
          events.map((event) => (
            <div key={event.id} className={`admin-activity-row admin-status-${event.status}`}>
              <span className="admin-status-dot" aria-hidden />
              <div>
                <strong>{event.label}</strong>
                <small>{event.detail}</small>
              </div>
              <time dateTime={event.at}>{new Date(event.at).toLocaleTimeString()}</time>
            </div>
          ))
        )}
      </div>
    </article>
  );
}
