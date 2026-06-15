import { useCallback, useEffect, useState } from "react";
import { Bot, CalendarClock, Code2, Play, RefreshCw } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { listJobs, runJob } from "../../../lib/adminJobsClient";
import type { JobSummary } from "../../../types/jobs";
import { Panel } from "../Panel";

type JobsPanelProps = {
  accessToken: string;
};

function formatInterval(ms: number): string {
  if (ms >= 60000) {
    return `${(ms / 60000).toFixed(ms % 60000 === 0 ? 0 : 1)} min`;
  }

  if (ms >= 1000) {
    return `${(ms / 1000).toFixed(ms % 1000 === 0 ? 0 : 1)} s`;
  }

  return `${ms} ms`;
}

function formatDate(value: string | null): string {
  return value ? new Date(value).toLocaleString() : "—";
}

const statusColor: Record<JobSummary["lastStatus"], string> = {
  NeverRun: "text-fg-muted",
  Success: "text-success",
  Failed: "text-danger"
};

export function JobsPanel({ accessToken }: JobsPanelProps) {
  const [jobs, setJobs] = useState<JobSummary[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [runningId, setRunningId] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      setJobs(await listJobs(accessToken));
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to load jobs");
      setJobs([]);
    } finally {
      setLoading(false);
    }
  }, [accessToken]);

  useEffect(() => {
    void load();
  }, [load]);

  async function handleRun(id: string) {
    setRunningId(id);

    try {
      await runJob(accessToken, id);
      await load();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Failed to run job");
    } finally {
      setRunningId(null);
    }
  }

  return (
    <Panel
      title="Jobs"
      icon={CalendarClock}
      action={
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => { void load(); }}
          disabled={loading}
          className="min-h-[32px] gap-1.5 border-border bg-surface font-semibold text-fg hover:bg-muted"
        >
          <RefreshCw size={14} aria-hidden />
          Refresh
        </Button>
      }
    >
      <div className="grid gap-4">
        {error && (
          <p className="m-0 rounded-md bg-danger/10 p-3 text-[13px] font-semibold text-danger">{error}</p>
        )}

        {jobs.length === 0 && !loading ? (
          <p className="m-0 rounded-md bg-muted p-4 text-[13px] leading-relaxed text-fg-muted">
            No jobs registered.
          </p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow className="border-border text-left hover:bg-transparent">
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Name</TableHead>
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Source</TableHead>
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Schedule</TableHead>
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Next run</TableHead>
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Last run</TableHead>
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Status</TableHead>
                <TableHead className="px-3 py-2 text-[11px] font-bold text-fg-subtle">Runs</TableHead>
                <TableHead className="px-3 py-2 text-right text-[11px] font-bold text-fg-subtle">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {jobs.map((job) => (
                <TableRow key={job.id} className="border-border/60">
                  <TableCell className="px-3 py-2">
                    <span className="font-semibold text-fg">{job.name}</span>
                    {job.description && (
                      <p className="m-0 mt-0.5 text-[11px] text-fg-muted">{job.description}</p>
                    )}
                  </TableCell>
                  <TableCell className="px-3 py-2">
                    <Badge
                      variant="outline"
                      className="gap-1 border-transparent bg-muted px-2 py-0.5 text-[11px] font-semibold text-fg-muted"
                    >
                      {job.source === "Lua" ? (
                        <>
                          <Bot size={11} aria-hidden />
                          Lua
                        </>
                      ) : (
                        <>
                          <Code2 size={11} aria-hidden />
                          C#
                        </>
                      )}
                    </Badge>
                  </TableCell>
                  <TableCell className="px-3 py-2 text-[13px] text-fg-muted">
                    {formatInterval(job.intervalMs)}
                    <span className="ml-1.5 text-[11px] text-fg-subtle">
                      {job.repeat ? "repeat" : "once"}
                    </span>
                  </TableCell>
                  <TableCell className="px-3 py-2 font-mono text-[11px] text-fg-muted">
                    {formatDate(job.nextRunAt)}
                  </TableCell>
                  <TableCell className="px-3 py-2 font-mono text-[11px] text-fg-muted">
                    {formatDate(job.lastRunAt)}
                  </TableCell>
                  <TableCell className="px-3 py-2">
                    <span
                      className={`text-[13px] font-semibold ${statusColor[job.lastStatus]}`}
                      title={job.lastError ?? undefined}
                    >
                      {job.lastStatus === "NeverRun" ? "Never run" : job.lastStatus}
                    </span>
                  </TableCell>
                  <TableCell className="px-3 py-2 font-mono text-[13px] text-fg-muted">
                    {job.runCount}
                  </TableCell>
                  <TableCell className="px-3 py-2">
                    <div className="flex items-center justify-end">
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon-sm"
                        aria-label={`Run ${job.name} now`}
                        disabled={runningId === job.id}
                        onClick={() => { void handleRun(job.id); }}
                        className="text-fg-muted hover:bg-muted hover:text-fg"
                      >
                        <Play size={16} aria-hidden />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>
    </Panel>
  );
}
