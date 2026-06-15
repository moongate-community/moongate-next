export type JobSummary = {
  id: string;
  name: string;
  description: string | null;
  source: "CSharp" | "Lua";
  intervalMs: number;
  repeat: boolean;
  nextRunAt: string | null;
  lastRunAt: string | null;
  lastDurationMs: number | null;
  lastStatus: "NeverRun" | "Success" | "Failed";
  lastError: string | null;
  runCount: number;
};
