import { apiFetch } from "./apiClient";
import type { AdminUser, CreateAdminUserRequest } from "../types/adminUser";

export async function getAdminUsers(): Promise<AdminUser[]> {
  const response = await apiFetch("/AdminUsers");

  if (!response.ok) {
    const text = await response.text();

    let message = "Unable to load admin users.";

    if (text) {
      try {
        const error = JSON.parse(text);
        message = error.message || message;
      } catch {
        message = text;
      }
    }

    throw new Error(message);
  }

  const text = await response.text();

  if (!text) {
    return [];
  }

  return JSON.parse(text) as AdminUser[];
}

export async function createAdminUser(
  request: CreateAdminUserRequest,
): Promise<AdminUser> {
  const response = await apiFetch("/AdminUsers", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  const text = await response.text();

  if (!response.ok) {
    let message = "Unable to create admin account.";

    if (text) {
      try {
        const error = JSON.parse(text);
        message = error.message || message;
      } catch {
        message = text;
      }
    }

    throw new Error(message);
  }

  return JSON.parse(text) as AdminUser;
}

export async function resetAdminPassword(
  adminUserId: string,
  password: string,
): Promise<void> {
  const response = await apiFetch(`/AdminUsers/${adminUserId}/password`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ password }),
  });

  const text = await response.text();

  if (!response.ok) {
    let message = "Unable to reset admin password.";

    if (text) {
      try {
        const error = JSON.parse(text);
        message = error.message || message;
      } catch {
        message = text;
      }
    }

    throw new Error(message);
  }
}

export async function updateAdminStatus(
  adminUserId: string,
  isActive: boolean,
): Promise<void> {
  const response = await apiFetch(`/AdminUsers/${adminUserId}/status`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ isActive }),
  });

  const text = await response.text();

  if (!response.ok) {
    let message = "Unable to update admin status.";

    if (text) {
      try {
        const error = JSON.parse(text);
        message = error.message || message;
      } catch {
        message = text;
      }
    }

    throw new Error(message);
  }
}
