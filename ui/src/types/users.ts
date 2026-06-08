export type AdminUserLevel = "Player" | "GameMaster" | "Administrator";

export type AdminUser = {
  id: string;
  username: string;
  email: string;
  level: AdminUserLevel;
  isActive: boolean;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type CreateUserPayload = {
  username: string;
  email: string;
  password: string;
  level: AdminUserLevel;
  isActive: boolean;
};

export type UpdateUserPayload = {
  email: string;
  level: AdminUserLevel;
};
