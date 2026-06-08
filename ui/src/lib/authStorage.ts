import type { AuthTokenResponse } from "../types/auth";

const storageKey = "moongate.auth";

export function readStoredAuth(): AuthTokenResponse | null {
  const raw = window.localStorage.getItem(storageKey);

  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as AuthTokenResponse;
  } catch {
    window.localStorage.removeItem(storageKey);

    return null;
  }
}

export function writeStoredAuth(value: AuthTokenResponse): void {
  window.localStorage.setItem(storageKey, JSON.stringify(value));
}

export function clearStoredAuth(): void {
  window.localStorage.removeItem(storageKey);
}
