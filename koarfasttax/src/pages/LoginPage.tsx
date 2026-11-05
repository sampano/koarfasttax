import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { Eye, EyeOff, LockKeyhole, Mail } from "lucide-react";
import { getUserIdFromToken, getUserRoleFromToken } from "../auth/jwt";
import { login } from "../api/auth";

function LoginPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");

  const navigate = useNavigate();

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setError("");

      const data = await login(email, password);

      localStorage.setItem("accessToken", data.accessToken);
      localStorage.setItem("refreshToken", data.refreshToken);

      const userId = getUserIdFromToken(data.accessToken);
      const role = getUserRoleFromToken(data.accessToken);

      if (role) {
        localStorage.setItem("role", role);
      }

      if (userId) {
        localStorage.setItem("userId", userId);
      }

      if (role === "Admin") {
        navigate("/dashboard");
      } else if (role === "Client") {
        navigate("/client/dashboard");
      } else {
        setError("Your account does not have a valid role.");
      }
    } catch (error) {
      console.error("Login error:", error);

      setError(
        error instanceof Error ? error.message : "Invalid email or password.",
      );
    }
  }

  return (
    <main className="min-h-screen bg-gray-50">
      <div className="grid min-h-screen lg:grid-cols-2">
        {/* Left side */}
        <section className="relative hidden overflow-hidden lg:block">
          <img
            src="/login-background.webp"
            alt="Koar Fast Tax background"
            className="absolute inset-0 h-full w-full object-cover"
          />

          <div className="absolute inset-0 bg-gradient-to-br from-green-950/60 via-green-900/25 to-transparent" />

          <div className="relative z-10 flex h-full flex-col justify-between p-12">
            <div className="inline-flex flex-col">
              {/* Top text */}
              <div className="text-5xl font-black uppercase leading-none tracking-tight text-white">
                KOAR FAST
              </div>

              {/* Bottom row */}
              <div className="mt-3 flex items-center gap-3">
                <span
                  className="uppercase leading-none text-white"
                  style={{
                    fontSize: "32px",
                    fontWeight: 300,
                  }}
                >
                  TAX
                </span>

                <span
                  className="rounded-xl bg-green-600 px-4 py-2 font-black italic text-white"
                  style={{
                    fontSize: "24px",
                  }}
                >
                  SERVICES
                </span>
              </div>
            </div>

            <div className="max-w-lg">
              <p className="text-sm font-semibold uppercase tracking-[0.25em] text-green-200">
                Secure Client Portal
              </p>

              <h2 className="mt-4 text-4xl font-bold leading-tight text-white">
                Simple, secure tax management for you and your business.
              </h2>

              <p className="mt-4 text-base leading-7 text-white/80">
                Access your tax cases, upload documents, review requests, and
                stay connected with your tax professional.
              </p>
            </div>
          </div>
        </section>

        {/* Right side */}
        <section className="flex items-center justify-center px-6 py-12 sm:px-10 lg:px-16">
          <div className="w-full max-w-md">
            <div className="mb-10">
              <div className="mb-3 inline-flex rounded-full bg-green-50 px-3 py-1 text-xs font-semibold text-green-700">
                Koar Fast Tax Portal
              </div>

              <h1 className="text-3xl font-bold text-gray-900">Welcome back</h1>

              <p className="mt-2 text-sm text-gray-500">
                Sign in to access your account.
              </p>
            </div>

            {error && (
              <p className="mb-5 rounded-xl border border-red-100 bg-red-50 p-4 text-sm text-red-600">
                {error}
              </p>
            )}

            <form onSubmit={handleSubmit} className="space-y-5">
              <div>
                <label
                  htmlFor="email"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  Email
                </label>

                <div className="relative">
                  <Mail
                    size={18}
                    className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400"
                  />

                  <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    className="w-full rounded-xl border border-gray-300 bg-white py-3 pl-11 pr-4 outline-none transition focus:border-green-600 focus:ring-2 focus:ring-green-100"
                    placeholder="Enter your email address"
                    autoComplete="email"
                  />
                </div>
              </div>

              <div>
                <label
                  htmlFor="password"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  Password
                </label>

                <div className="relative">
                  <LockKeyhole
                    size={18}
                    className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400"
                  />

                  <input
                    id="password"
                    type={showPassword ? "text" : "password"}
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    className="w-full rounded-xl border border-gray-300 bg-white py-3 pl-11 pr-12 outline-none transition focus:border-green-600 focus:ring-2 focus:ring-green-100"
                    placeholder="Enter your password"
                    autoComplete="current-password"
                  />

                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                    aria-label={
                      showPassword ? "Hide password" : "Show password"
                    }
                  >
                    {showPassword ? <EyeOff size={19} /> : <Eye size={19} />}
                  </button>
                </div>
              </div>

              <div className="flex justify-end">
                <button
                  type="button"
                  className="text-sm font-medium text-green-700 hover:text-green-800"
                >
                  Forgot password?
                </button>
              </div>

              <button
                type="submit"
                className="w-full rounded-xl bg-green-700 px-4 py-3 font-semibold text-white transition hover:bg-green-800 focus:outline-none focus:ring-2 focus:ring-green-200"
              >
                Sign In
              </button>
            </form>

            <p className="mt-8 text-center text-xs text-gray-400">
              © Koar Fast Tax. Secure client access.
            </p>
          </div>
        </section>
      </div>
    </main>
  );
}

export default LoginPage;
