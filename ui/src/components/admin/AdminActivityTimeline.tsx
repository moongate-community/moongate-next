import { History } from "lucide-react";
import type { AdminActivityEvent } from "../../types/admin";
import { statusAccentClass } from "./adminUi";
import { Panel } from "./Panel";

type AdminActivityTimelineProps = {
  events: AdminActivityEvent[];
};

export function AdminActivityTimeline({ events }: AdminActivityTimelineProps) {
  return (
    <Panel title="Activity" icon={History}>
      {events.length === 0 ? (
        <p className="m-0 rounded-md bg-muted p-3 text-[13px] leading-relaxed text-fg-muted">
          No dashboard activity yet.
        </p>
      ) : (
        <ol className="m-0 grid list-none gap-0 p-0">
          {events.map((event, index) => {
            const isLast = index === events.length - 1;

            return (
              <li key={event.id} className="relative grid grid-cols-[14px_minmax(0,1fr)] gap-3 pb-4 last:pb-0">
                {!isLast && <span className="absolute bottom-0 left-[6px] top-5 w-px bg-border" aria-hidden />}
                <span
                  className={`relative z-10 mt-1 h-3.5 w-3.5 rounded-full ring-2 ring-surface ${statusAccentClass[event.status]}`}
                  aria-hidden
                />
                <div className="min-w-0">
                  <div className="flex items-baseline justify-between gap-2">
                    <strong className="truncate text-sm font-semibold text-fg">{event.label}</strong>
                    <time className="shrink-0 font-mono text-[11px] text-fg-subtle" dateTime={event.at}>
                      {new Date(event.at).toLocaleTimeString()}
                    </time>
                  </div>
                  <small className="block text-xs leading-relaxed text-fg-muted">{event.detail}</small>
                </div>
              </li>
            );
          })}
        </ol>
      )}
    </Panel>
  );
}
