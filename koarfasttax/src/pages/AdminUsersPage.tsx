import { useEffect, useState } from "react";
import {
  createAdminUser,
  getAdminUsers,
  resetAdminPassword,
  updateAdminStatus,
} from "../api/adminUsers";
import type { AdminUser } from "../types/adminUser";
import Toast from "../components/Toast";
import ConfirmModal from "../components/ConfirmModal";

export default function AdminUsersPage() {
  // Admin users state
  const [admins, setAdmins] = useState<AdminUser[]>([]);
  const [showForm, setShowForm] = useState(false);

  // Password reset state
  const [resetAdmin, setResetAdmin] = useState<AdminUser | null>(null);
  const [newPassword, setNewPassword] = useState("");
  const [resettingPassword, setResettingPassword] = useState(false);

  // Admin status update state
  const [statusAdmin, setStatusAdmin] = useState<AdminUser | null>(null);
  const [updatingStatus, setUpdatingStatus] = useState(false);

  const currentUserId = localStorage.getItem("userId");

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [toast, setToast] = useState<{
    message: string;
    type: "success" | "error";
  } | null>(null);

  function showToast(message: string, type: "success" | "error") {
    setToast({ message, type });

    setTimeout(() => {
      setToast(null);
    }, 4000);
  }

  async function loadAdmins() {
    try {
      const data = await getAdminUsers();
      setAdmins(data);
    } catch (error) {
      showToast(
        error instanceof Error ? error.message : "Unable to load admin users.",
        "error",
      );
    }
  }

  async function handleResetPassword() {
    if (!resetAdmin) return;

    if (newPassword.length < 8) {
      showToast("Password must be at least 8 characters.", "error");
      return;
    }

    setResettingPassword(true);

    try {
      await resetAdminPassword(resetAdmin.id, newPassword);

      setResetAdmin(null);
      setNewPassword("");

      showToast("Admin password reset successfully.", "success");
    } catch (error) {
      showToast(
        error instanceof Error
          ? error.message
          : "Unable to reset admin password.",
        "error",
      );
    } finally {
      setResettingPassword(false);
    }
  }

  async function handleUpdateStatus() {
    if (!statusAdmin) return;

    setUpdatingStatus(true);

    try {
      await updateAdminStatus(statusAdmin.id, !statusAdmin.isActive);

      await loadAdmins();

      showToast(
        statusAdmin.isActive
          ? "Admin account deactivated successfully."
          : "Admin account activated successfully.",
        "success",
      );

      setStatusAdmin(null);
    } catch (error) {
      showToast(
        error instanceof Error
          ? error.message
          : "Unable to update admin status.",
        "error",
      );
    } finally {
      setUpdatingStatus(false);
    }
  }

  useEffect(() => {
    loadAdmins();
  }, []);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!firstName.trim()) {
      showToast("First name is required.", "error");
      return;
    }

    if (!lastName.trim()) {
      showToast("Last name is required.", "error");
      return;
    }

    if (!email.trim()) {
      showToast("Email is required.", "error");
      return;
    }

    if (password.length < 8) {
      showToast("Password must be at least 8 characters.", "error");
      return;
    }

    try {
      await createAdminUser({
        firstName,
        lastName,
        email,
        password,
      });

      setFirstName("");
      setLastName("");
      setEmail("");
      setPassword("");
      setShowForm(false);

      await loadAdmins();

      showToast("Admin account created successfully.", "success");
    } catch (error) {
      showToast(
        error instanceof Error
          ? error.message
          : "Unable to create admin account.",
        "error",
      );
    }
  }

  return (
    <div className="p-6">
      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          onClose={() => setToast(null)}
        />
      )}

      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Admin Users</h1>

          <p className="mt-1 text-sm text-gray-500">
            Manage administrator accounts for Koar Fast Tax.
          </p>
        </div>

        <button
          type="button"
          onClick={() => setShowForm(!showForm)}
          className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          {showForm ? "Cancel" : "Create Admin"}
        </button>
      </div>

      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="mb-6 rounded-xl border border-gray-200 bg-white p-6 shadow-sm"
        >
          <h2 className="mb-4 text-lg font-semibold">Create Admin Account</h2>

          <div className="grid gap-4 md:grid-cols-2">
            <input
              type="text"
              placeholder="First Name"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              className="rounded-lg border border-gray-300 px-3 py-2"
            />

            <input
              type="text"
              placeholder="Last Name"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              className="rounded-lg border border-gray-300 px-3 py-2"
            />

            <input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="rounded-lg border border-gray-300 px-3 py-2"
            />

            <input
              type="password"
              placeholder="Temporary Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="rounded-lg border border-gray-300 px-3 py-2"
            />
          </div>

          <button
            type="submit"
            className="mt-5 rounded-lg bg-blue-600 px-4 py-2 text-white hover:bg-blue-700"
          >
            Create Admin
          </button>
        </form>
      )}

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <table className="w-full text-left">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-5 py-3">Name</th>
              <th className="px-5 py-3">Email</th>
              <th className="px-5 py-3">Role</th>
              <th className="px-5 py-3">Status</th>
              <th className="px-5 py-3">Actions</th>
            </tr>
          </thead>

          <tbody>
            {admins.map((admin) => (
              <tr key={admin.id} className="border-t">
                <td className="px-5 py-4">
                  {admin.firstName} {admin.lastName}
                </td>

                <td className="px-5 py-4">{admin.email}</td>

                <td className="px-5 py-4">{admin.role}</td>

                <td className="px-5 py-4">
                  <button
                    type="button"
                    disabled={admin.id === currentUserId}
                    onClick={() => setStatusAdmin(admin)}
                    title={
                      admin.id === currentUserId
                        ? "You cannot deactivate your own account."
                        : admin.isActive
                          ? "Deactivate admin account"
                          : "Activate admin account"
                    }
                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition ${
                      admin.isActive ? "bg-green-600" : "bg-gray-300"
                    } ${
                      admin.id === currentUserId
                        ? "cursor-not-allowed opacity-50"
                        : "cursor-pointer"
                    }`}
                    aria-label={
                      admin.isActive
                        ? "Deactivate admin account"
                        : "Activate admin account"
                    }
                  >
                    <span
                      className={`inline-block h-4 w-4 transform rounded-full bg-white transition ${
                        admin.isActive ? "translate-x-6" : "translate-x-1"
                      }`}
                    />
                  </button>
                </td>

                <td className="px-5 py-4">
                  <button
                    type="button"
                    disabled={!admin.isActive}
                    onClick={() => {
                      setResetAdmin(admin);
                      setNewPassword("");
                    }}
                    className={`text-sm font-medium ${
                      admin.isActive
                        ? "text-blue-600 hover:text-blue-800"
                        : "cursor-not-allowed text-gray-400"
                    }`}
                  >
                    Reset Password
                  </button>
                </td>
              </tr>
            ))}

            {admins.length === 0 && (
              <tr>
                <td colSpan={4} className="px-5 py-8 text-center text-gray-500">
                  No admin users found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {resetAdmin && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <h2 className="text-lg font-semibold text-gray-900">
              Reset Admin Password
            </h2>

            <p className="mt-2 text-sm text-gray-500">
              Set a new temporary password for {resetAdmin.firstName}{" "}
              {resetAdmin.lastName}.
            </p>

            <div className="mt-5">
              <label className="mb-1 block text-sm font-medium text-gray-700">
                New Password
              </label>

              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Minimum 8 characters"
                className="w-full rounded-lg border border-gray-300 px-3 py-2"
              />
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => {
                  setResetAdmin(null);
                  setNewPassword("");
                }}
                disabled={resettingPassword}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm"
              >
                Cancel
              </button>

              <button
                type="button"
                onClick={handleResetPassword}
                disabled={resettingPassword}
                className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {resettingPassword ? "Resetting..." : "Reset Password"}
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmModal
        open={statusAdmin !== null}
        title={
          statusAdmin?.isActive
            ? "Deactivate Admin Account"
            : "Activate Admin Account"
        }
        message={
          statusAdmin?.isActive
            ? `Are you sure you want to deactivate ${statusAdmin.firstName} ${statusAdmin.lastName}?`
            : `Are you sure you want to activate ${statusAdmin?.firstName} ${statusAdmin?.lastName}?`
        }
        confirmText={statusAdmin?.isActive ? "Deactivate" : "Activate"}
        danger={statusAdmin?.isActive ?? false}
        loading={updatingStatus}
        onCancel={() => setStatusAdmin(null)}
        onConfirm={handleUpdateStatus}
      />
    </div>
  );
}
