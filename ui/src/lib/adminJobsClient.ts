import { readJson } from "./authClient";
import type { JobSummary } from "../types/jobs";

function authHeaders(accessToken: string): HeadersInit {
  return { Authorization: `Bearer ${accessToken}` };
}

export async function listJobs(accessToken: string): Promise<JobSummary[]> {
  const response = await fetch("/api/admin/jobs", { headers: authHeaders(accessToken) });

  return readJson<JobSummary[]>(response);
}

export async function runJob(accessToken: string, id: string): Promise<void> {
  const response = await fetch(`/api/admin/jobs/${encodeURIComponent(id)}/run`, {
    method: "POST",
    headers: authHeaders(accessToken)
  });

  if (!response.ok) {
    throw new Error(`Failed to run job (${response.status})`);
  }
}
