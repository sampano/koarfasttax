import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowLeft, Mail } from "lucide-react";
import { forgotPassword } from "../api/auth";

function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const navigate = useNavigate();

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      setError("");
      setMessage("");
      setIsSubmitting(true);

      const data = await forgotPassword(email);

      setMessage(data.message);
    } catch (error) {
      setError(
        error instanceof Error
          ? error.message
          : "Unable to process password reset.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="min-h-screen bg-gray-50">
      <div className="grid min-h-screen lg:grid-cols-2">
        <section className="relative hidden overflow-hidden lg:block">
          <img
            src="/login-background.webp"
            alt="Koar Fast Tax background"
            className="absolute inset-0 h-full w-full object-cover"
          />

          <div className="absolute inset-0 bg-gradient-to-br from-green-950/60 via-green-900/25 to-transparent" />

          <div className="relative z-10 flex h-full flex-col justify-between p-12">
            <div className="inline-flex flex-col">
              <div className="text-5xl font-black uppercase leading-none tracking-tight text-white">
                KOAR FAST
              </div>

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
                Recover access to your Koar Fast Tax account.
              </h2>

              <p className="mt-4 text-base leading-7 text-white/80">
                Enter the email associated with your portal account and we will
                send you instructions to reset your password.
              </p>
            </div>
          </div>
        </section>

        <section className="flex items-center justify-center px-6 py-12 sm:px-10 lg:px-16">
          <div className="w-full max-w-md">
            <button
              type="button"
              onClick={() => navigate("/login")}
              className="mb-8 inline-flex items-center gap-2 text-sm font-medium text-gray-500 hover:text-gray-700"
            >
              <ArrowLeft size={17} />
              Back to login
            </button>

            <div className="mb-8">
              <div className="mb-3 inline-flex rounded-full bg-green-50 px-3 py-1 text-xs font-semibold text-green-700">
                Password Recovery
              </div>

              <h1 className="text-3xl font-bold text-gray-900">
                Forgot your password?
              </h1>

              <p className="mt-2 text-sm leading-6 text-gray-500">
                Enter the email address associated with your portal account.
              </p>
            </div>

            {error && (
              <p className="mb-5 rounded-xl border border-red-100 bg-red-50 p-4 text-sm text-red-600">
                {error}
              </p>
            )}

            {message && (
              <p className="mb-5 rounded-xl border border-green-100 bg-green-50 p-4 text-sm text-green-700">
                {message}
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
                    required
                    autoComplete="email"
                    placeholder="Enter your email address"
                    className="w-full rounded-xl border border-gray-300 bg-white py-3 pl-11 pr-4 outline-none transition focus:border-green-600 focus:ring-2 focus:ring-green-100"
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full rounded-xl bg-green-700 px-4 py-3 font-semibold text-white transition hover:bg-green-800 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isSubmitting ? "Sending..." : "Send Reset Link"}
              </button>
            </form>
          </div>
        </section>
      </div>
    </main>
  );
}

export default ForgotPasswordPage;
