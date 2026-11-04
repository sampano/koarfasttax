export type Client = {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  createdAt: string;
  userId: string;
  portalUserId: string | null;
  goHighLevelContactId: string | null;
  notificationsEnabled: boolean;
};

export type PagedClients = {
  items: Client[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type GoHighLevelContact = {
  id: string;
  firstName: string;
  lastName: string;
  email: string | null;
  phone: string | null;
  isImported: boolean;
};
