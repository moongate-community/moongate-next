import { readJson } from "./authClient";
import type {
  PluginCatalogEntry,
  PluginConfigForm,
  PluginConfigSaveResult,
  PluginConfigValue,
  PluginConfigView,
  PluginTestResult
} from "../types/plugins";

function authHeaders(accessToken: string, json = false): HeadersInit {
  const headers: Record<string, string> = { Authorization: `Bearer ${accessToken}` };

  if (json) {
    headers["Content-Type"] = "application/json";
  }

  return headers;
}

export async function listPlugins(accessToken: string): Promise<PluginCatalogEntry[]> {
  const response = await fetch("/api/admin/plugins/", {
    headers: authHeaders(accessToken)
  });

  return readJson<PluginCatalogEntry[]>(response);
}

export async function getPluginConfig(accessToken: string, id: string): Promise<PluginConfigView> {
  const response = await fetch(`/api/admin/plugins/${encodeURIComponent(id)}/config`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PluginConfigView>(response);
}

export async function getPluginConfigForm(accessToken: string, id: string): Promise<PluginConfigForm | null> {
  const response = await fetch(`/api/admin/plugins/${encodeURIComponent(id)}/config/form`, {
    headers: authHeaders(accessToken)
  });

  if (response.status === 404) {
    return null;
  }

  return readJson<PluginConfigForm>(response);
}

export async function savePluginConfig(
  accessToken: string,
  id: string,
  values: Record<string, PluginConfigValue>
): Promise<PluginConfigSaveResult> {
  const response = await fetch(`/api/admin/plugins/${encodeURIComponent(id)}/config`, {
    method: "PUT",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify({ values })
  });

  return readJson<PluginConfigSaveResult>(response);
}

export async function testPluginConfig(accessToken: string, id: string): Promise<PluginTestResult> {
  const response = await fetch(`/api/admin/plugins/${encodeURIComponent(id)}/test`, {
    method: "POST",
    headers: authHeaders(accessToken)
  });

  return readJson<PluginTestResult>(response);
}
