import { readJson } from "./authClient";
import type { AdminUser, CreateUserPayload, PagedResult, UpdateUserPayload } from "../types/users";

function authHeaders(accessToken: string, json = false): HeadersInit {
  const headers: Record<string, string> = { Authorization: `Bearer ${accessToken}` };

  if (json) {
    headers["Content-Type"] = "application/json";
  }

  return headers;
}

export async function listUsers(
  accessToken: string,
  page: number,
  pageSize: number,
  search: string
): Promise<PagedResult<AdminUser>> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });

  if (search.trim().length > 0) {
    params.set("search", search.trim());
  }

  const response = await fetch(`/api/admin/users?${params.toString()}`, {
    headers: authHeaders(accessToken)
  });

  return readJson<PagedResult<AdminUser>>(response);
}

export async function createUser(accessToken: string, payload: CreateUserPayload): Promise<AdminUser> {
  const response = await fetch("/api/admin/users", {
    method: "POST",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify(payload)
  });

  return readJson<AdminUser>(response);
}

export async function updateUser(accessToken: string, id: string, payload: UpdateUserPayload): Promise<AdminUser> {
  const response = await fetch(`/api/admin/users/${encodeURIComponent(id)}`, {
    method: "PUT",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify(payload)
  });

  return readJson<AdminUser>(response);
}

export async function setUserActive(accessToken: string, id: string, isActive: boolean): Promise<AdminUser> {
  const response = await fetch(`/api/admin/users/${encodeURIComponent(id)}/active`, {
    method: "POST",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify({ isActive })
  });

  return readJson<AdminUser>(response);
}

export async function resetUserPassword(accessToken: string, id: string, password: string): Promise<void> {
  const response = await fetch(`/api/admin/users/${encodeURIComponent(id)}/password`, {
    method: "POST",
    headers: authHeaders(accessToken, true),
    body: JSON.stringify({ password })
  });

  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}`);
  }
}

export async function deleteUser(accessToken: string, id: string): Promise<void> {
  const response = await fetch(`/api/admin/users/${encodeURIComponent(id)}`, {
    method: "DELETE",
    headers: authHeaders(accessToken)
  });

  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}`);
  }
}
