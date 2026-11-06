import { API_URL } from "./config";

type TokenResponse = {
  accessToken: string;
  refreshToken: string;
};

export async function refreshAccessToken(): Promise<string | null> {
  const userId = localStorage.getItem("userId");
  const refreshToken = localStorage.getItem("refreshToken");

  if (!userId || !refreshToken) {
    return null;
  }

  const response = await fetch(`${API_URL}/Auth/refresh-token`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      userId,
      refreshToken,
    }),
  });

  if (!response.ok) {
    return null;
  }

  const data: TokenResponse = await response.json();

  localStorage.setItem("accessToken", data.accessToken);
  localStorage.setItem("refreshToken", data.refreshToken);

  return data.accessToken;
}

export async function login(
  email: string,
  password: string,
): Promise<TokenResponse> {
  const response = await fetch(`${API_URL}/Auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      email,
      password,
    }),
  });

  if (!response.ok) {
    const text = await response.text();

    let message = "Login failed.";

    if (text) {
      try {
        const data = JSON.parse(text);
        message = data.message ?? text;
      } catch {
        message = text;
      }
    }

    throw new Error(message);
  }

  return response.json();
}

export async function forgotPassword(email: string) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/Auth/forgot-password`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email }),
    },
  );

  const data = await response.json();

  if (!response.ok) {
    throw new Error(data.message || "Unable to process password reset.");
  }

  return data;
}

export async function resetPassword(token: string, newPassword: string) {
  const response = await fetch(
    `${import.meta.env.VITE_API_URL}/api/Auth/reset-password`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        token,
        newPassword,
      }),
    },
  );

  const data = await response.json();

  if (!response.ok) {
    throw new Error(data.message || "Unable to reset password.");
  }

  return data;
}
