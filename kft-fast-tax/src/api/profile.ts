import { apiFetch } from "./apiClient";

export type Profile = {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string | null;
  profileImageUrl: string | null;
  role: string;
};

export type UpdateProfileRequest = {
  firstName: string;
  lastName: string;
  phone: string | null;
  email: string;
};

export async function getProfile(): Promise<Profile> {
  const response = await apiFetch("/Profile");

  if (!response.ok) {
    throw new Error("Failed to load profile.");
  }

  return response.json();
}

export async function updateProfile(
  request: UpdateProfileRequest,
): Promise<Profile> {
  const response = await apiFetch("/Profile", {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error("Failed to update profile.");
  }

  return response.json();
}

export async function uploadProfilePhoto(file: File): Promise<Profile> {
  const formData = new FormData();

  formData.append("file", file);

  const response = await apiFetch("/Profile/photo", {
    method: "POST",
    body: formData,
  });

  if (!response.ok) {
    throw new Error("Failed to upload profile photo.");
  }

  return response.json();
}

export async function deleteProfilePhoto(): Promise<void> {
  const response = await apiFetch("/Profile/photo", {
    method: "DELETE",
  });

  if (!response.ok) {
    throw new Error("Failed to delete profile photo.");
  }
}
