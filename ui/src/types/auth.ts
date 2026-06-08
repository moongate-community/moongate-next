export type AuthUser = {
  id: string;
  username: string;
  level: string;
  isActive: boolean;
};

export type AuthTokenResponse = {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  user: AuthUser;
};
