import type { AdminUser } from "./users";

export type AdminCommandTarget =
  | {
      kind: "itemTemplate";
      id: string;
      sequence: number;
    }
  | {
      kind: "mobileTemplate";
      id: string;
      sequence: number;
    }
  | {
      kind: "lootTemplate";
      id: string;
      sequence: number;
    }
  | {
      kind: "user";
      user: AdminUser;
      sequence: number;
    };
