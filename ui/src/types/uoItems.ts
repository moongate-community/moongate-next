export type UoItemSummary = {
  itemId: number;
  itemIdHex: string;
  name: string;
  imageUrl: string;
  flags: string[];
  weight: number;
  quality: number;
  animation: number;
  quantity: number;
  value: number;
  height: number;
  container: boolean;
  weapon: boolean;
  armor: boolean;
  wearable: boolean;
  door: boolean;
  surface: boolean;
  background: boolean;
  wall: boolean;
};

export type UoItemDetail = UoItemSummary & {
  rawFlags: number;
};

export type UoItemFilters = {
  page: number;
  pageSize: number;
  search: string;
  flag: string;
};
