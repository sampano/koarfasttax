export type AdminUser = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isActive: boolean;
};

export type CreateAdminUserRequest = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
};
