import type { AuthTokenResponse, AuthUser } from "../types/auth";

export async function readJson<T>(response: Response): Promise<T> {
  if (!response.ok) {
    throw new Error(`${response.status} ${response.statusText}`);
  }

  return (await response.json()) as T;
}

export async function login(username: string, password: string): Promise<AuthTokenResponse> {
  const response = await fetch("/api/auth/login", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ username, password })
  });

  return readJson<AuthTokenResponse>(response);
}

export async function refresh(refreshToken: string): Promise<AuthTokenResponse> {
  const response = await fetch("/api/auth/refresh", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ refreshToken })
  });

  return readJson<AuthTokenResponse>(response);
}

export async function logout(refreshToken: string): Promise<void> {
  await fetch("/api/auth/logout", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ refreshToken })
  });
}

export async function me(accessToken: string): Promise<AuthUser> {
  const response = await fetch("/api/auth/me", {
    headers: {
      Authorization: `Bearer ${accessToken}`
    }
  });

  return readJson<AuthUser>(response);
}
