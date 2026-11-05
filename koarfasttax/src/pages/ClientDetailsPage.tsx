import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import TaxCaseStatusBadge from "../components/TaxCaseStatusBadge";
import type { Client, PortalUser } from "../types/client";
import type { TaxCase } from "../types/taxCase";
import {
  getClientById,
  updateClient,
  deleteClient,
  assignPortalUser,
  createPortalAccount,
  unassignPortalUser,
  resetPortalPassword,
  syncClientToGoHighLevel,
  updateClientNotifications,
  getPortalUserByClientEmail,
} from "../api/clients";
import { getTaxCasesByClient } from "../api/taxCases";
import Toast from "../components/Toast";
import ConfirmModal from "../components/ConfirmModal";

function ClientDetailsPage() {
  // Get the client ID from the URL parameters and set up navigation
  const { id } = useParams();
  const navigate = useNavigate();

  // State variables for client details, tax cases, loading state, and error message
  const [client, setClient] = useState<Client | null>(null);
  const [taxCases, setTaxCases] = useState<TaxCase[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // State variables for editing client details
  const [editing, setEditing] = useState(false);
  const [editFirstName, setEditFirstName] = useState("");
  const [editLastName, setEditLastName] = useState("");
  const [editEmail, setEditEmail] = useState("");
  const [editPhone, setEditPhone] = useState("");
  const [editError, setEditError] = useState("");
  const [saving, setSaving] = useState(false);

  // State variables for syncing with GoHighLevel
  const [syncingGhl, setSyncingGhl] = useState(false);

  // State variables for managing portal user assignment
  //const [portalUserSearch, setPortalUserSearch] = useState("");
  //const [portalUsers, setPortalUsers] = useState<PortalUser[]>([]);
  //const [searchingPortalUsers, setSearchingPortalUsers] = useState(false);

  const [assigningPortalUser, setAssigningPortalUser] = useState(false);

  // State variables for managing notification settings
  const [toast, setToast] = useState<{
    message: string;
    type: "success" | "error";
  } | null>(null);

  // State variables for managing portal account password reset
  const [showGhlConfirm, setShowGhlConfirm] = useState(false);
  // State variables for managing portal account password reset
  const [showUnlinkConfirm, setShowUnlinkConfirm] = useState(false);

  const [resetPassword, setResetPassword] = useState("");
  const [resettingPassword, setResettingPassword] = useState(false);
  const [showResetPassword, setShowResetPassword] = useState(false);

  const [unlinkingPortalUser, setUnlinkingPortalUser] = useState(false);

  const [showPasswordResetSuccess, setShowPasswordResetSuccess] =
    useState(false);

  const [portalPassword, setPortalPassword] = useState("");
  const [creatingPortalAccount, setCreatingPortalAccount] = useState(false);

  // State variable for checking if a portal user already exists
  const [existingPortalUser, setExistingPortalUser] =
    useState<PortalUser | null>(null);
  const [checkingPortalUser, setCheckingPortalUser] = useState(false);

  // State variables for updating notifications
  const [updatingNotifications, setUpdatingNotifications] = useState(false);
  const [notificationError, setNotificationError] = useState("");

  useEffect(() => {
    async function loadClient() {
      try {
        const data = await getClientById(id!);

        setClient(data);

        if (!data.portalUserId) {
          setCheckingPortalUser(true);

          try {
            const portalUser = await getPortalUserByClientEmail(data.id);

            setExistingPortalUser(portalUser);
          } finally {
            setCheckingPortalUser(false);
          }
        } else {
          setExistingPortalUser(null);
        }

        const taxCaseData = await getTaxCasesByClient(id!);

        setTaxCases(taxCaseData);
      } catch (error) {
        console.error(error);
        setError("Unable to connect to the server.");
      } finally {
        setLoading(false);
      }
    }

    loadClient();
  }, [id]);

  function startEditingClient() {
    if (!client) {
      return;
    }

    setEditFirstName(client.firstName);
    setEditLastName(client.lastName);
    setEditEmail(client.email);
    setEditPhone(client.phone ?? "");
    setEditError("");
    setEditing(true);
  }

  // Function to handle updating the client details
  async function handleUpdateClient(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!client) {
      return;
    }

    setEditError("");
    setSaving(true);

    try {
      const updatedClient = await updateClient(client.id, {
        firstName: editFirstName,
        lastName: editLastName,
        email: editEmail,
        phone: editPhone || null,
      });

      setClient(updatedClient);
      setEditing(false);

      setClient(updatedClient);
      setEditing(false);
    } catch (error) {
      console.error(error);
      setEditError("Unable to update client.");
    } finally {
      setSaving(false);
    }
  }

  // Function to handle deleting the client
  async function handleDeleteClient() {
    if (!client) {
      return;
    }

    try {
      await deleteClient(client.id);

      navigate("/clients");
    } catch (error) {
      console.error(error);
      setEditError("Unable to delete client.");
    }
  }

  async function handleAssignPortalUser(portalUser: PortalUser) {
    if (!client) {
      return;
    }

    setAssigningPortalUser(true);

    try {
      await assignPortalUser(client.id, portalUser.id);

      await reloadClient();

      showToast(
        `Portal user ${portalUser.email} assigned successfully.`,
        "success",
      );

      //setPortalUsers([]);
      //setPortalUserSearch("");
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error
          ? error.message
          : "Unable to assign portal user.",
        "error",
      );
    } finally {
      setAssigningPortalUser(false);
    }
  }

  // Function to handle syncing the client to GoHighLevel
  async function handleSyncToGoHighLevel() {
    if (!client) {
      return;
    }

    setSyncingGhl(true);

    try {
      const updatedClient = await syncClientToGoHighLevel(client.id);

      setClient(updatedClient);

      setShowGhlConfirm(false);

      showToast("Client successfully synced to GoHighLevel.", "success");
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error
          ? error.message
          : "Unable to sync client to GoHighLevel.",
        "error",
      );
    } finally {
      setSyncingGhl(false);
    }
  }

  async function handleCreatePortalAccount(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!client) {
      return;
    }

    if (!portalPassword.trim()) {
      showToast("Please enter a temporary password.", "error");
      return;
    }

    setCreatingPortalAccount(true);

    try {
      await createPortalAccount(client.id, portalPassword.trim());

      await reloadClient();

      showToast(
        `Portal account created successfully for ${client.email}.`,
        "success",
      );

      setPortalPassword("");
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error
          ? error.message
          : "Unable to create portal account.",
        "error",
      );
    } finally {
      setCreatingPortalAccount(false);
    }
  }

  async function handleUnassignPortalUser() {
    if (!client) {
      return;
    }

    setShowUnlinkConfirm(false);

    showToast("Portal account unlinked successfully.", "success");

    setUnlinkingPortalUser(true);

    try {
      await unassignPortalUser(client.id);

      await reloadClient();

      showToast("Portal account unlinked successfully.", "success");
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error
          ? error.message
          : "Unable to unlink portal account.",
        "error",
      );
    } finally {
      setUnlinkingPortalUser(false);
    }
  }

  async function handleResetPortalPassword(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!client) {
      return;
    }

    if (!resetPassword.trim()) {
      showToast("Please enter a new temporary password.", "error");
      return;
    }

    setResettingPassword(true);

    try {
      await resetPortalPassword(client.id, resetPassword.trim());

      setShowPasswordResetSuccess(true);
      setResetPassword("");
      setShowResetPassword(false);
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error
          ? error.message
          : "Unable to reset portal password.",
        "error",
      );
    } finally {
      setResettingPassword(false);
    }
  }

  async function reloadClient() {
    if (!id) {
      return;
    }

    const data = await getClientById(id);
    setClient(data);
  }

  async function handleNotificationToggle() {
    if (!client) {
      return;
    }

    setUpdatingNotifications(true);
    //setNotificationMessage("");
    setNotificationError("");

    try {
      const updatedClient = await updateClientNotifications(
        client.id,
        !client.notificationsEnabled,
      );

      setClient(updatedClient);
    } catch (error) {
      console.error(error);

      setNotificationError(
        error instanceof Error
          ? error.message
          : "Unable to update notification settings.",
      );
    } finally {
      setUpdatingNotifications(false);
    }
  }

  /*async function handlePortalUserSearch(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    const email = portalUserSearch.trim();

    if (!email) {
      showToast("Please enter an email address.", "error");
      return;
    }

    setSearchingPortalUsers(true);

    try {
      const users = await searchPortalUsers(email);

      setPortalUsers(users);
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error
          ? error.message
          : "Unable to search portal users.",
        "error",
      );
    } finally {
      setSearchingPortalUsers(false);
    }
  }*/

  function showToast(message: string, type: "success" | "error") {
    setToast({
      message,
      type,
    });

    setTimeout(() => {
      setToast(null);
    }, 4000);
  }

  if (loading) {
    return <p className="p-8">Loading client...</p>;
  }

  if (error) {
    return <p className="p-8 text-red-600">{error}</p>;
  }

  if (!client) {
    return null;
  }

  return (
    <div className="min-h-screen bg-gray-100 p-8">
      <div className="mx-auto max-w-4xl">
        <button
          type="button"
          onClick={() => navigate("/clients")}
          className="mb-6 text-sm font-medium text-blue-600 hover:text-blue-700"
        >
          ← Back to Clients
        </button>

        <div className="rounded-xl bg-white p-6 shadow">
          <h1 className="text-3xl font-bold text-gray-900">
            {client.firstName} {client.lastName}
          </h1>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={startEditingClient}
              className="rounded-lg bg-blue-50 px-4 py-2 text-sm font-medium text-blue-600 hover:bg-blue-100"
            >
              Edit
            </button>

            <button
              type="button"
              onClick={handleDeleteClient}
              className="rounded-lg bg-red-50 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-100"
            >
              Delete
            </button>
            {editing && (
              <form
                onSubmit={handleUpdateClient}
                className="mt-6 space-y-4 rounded-lg bg-gray-50 p-5"
              >
                {editError && (
                  <p className="rounded-lg bg-red-50 p-3 text-sm text-red-600">
                    {editError}
                  </p>
                )}

                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <label
                      htmlFor="editFirstName"
                      className="mb-2 block text-sm font-medium text-gray-700"
                    >
                      First Name
                    </label>

                    <input
                      id="editFirstName"
                      type="text"
                      value={editFirstName}
                      onChange={(event) => setEditFirstName(event.target.value)}
                      required
                      maxLength={50}
                      className="w-full rounded-lg border border-gray-300 px-4 py-3"
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="editLastName"
                      className="mb-2 block text-sm font-medium text-gray-700"
                    >
                      Last Name
                    </label>

                    <input
                      id="editLastName"
                      type="text"
                      value={editLastName}
                      onChange={(event) => setEditLastName(event.target.value)}
                      required
                      maxLength={50}
                      className="w-full rounded-lg border border-gray-300 px-4 py-3"
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="editEmail"
                      className="mb-2 block text-sm font-medium text-gray-700"
                    >
                      Email
                    </label>

                    <input
                      id="editEmail"
                      type="email"
                      value={editEmail}
                      onChange={(event) => setEditEmail(event.target.value)}
                      required
                      className="w-full rounded-lg border border-gray-300 px-4 py-3"
                    />
                  </div>

                  <div>
                    <label
                      htmlFor="editPhone"
                      className="mb-2 block text-sm font-medium text-gray-700"
                    >
                      Phone
                    </label>

                    <input
                      id="editPhone"
                      type="text"
                      value={editPhone}
                      onChange={(event) => setEditPhone(event.target.value)}
                      maxLength={30}
                      className="w-full rounded-lg border border-gray-300 px-4 py-3"
                    />
                  </div>
                </div>

                <div className="flex gap-3">
                  <button
                    type="submit"
                    disabled={saving}
                    className="rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                  >
                    {saving ? "Saving..." : "Save Changes"}
                  </button>

                  <button
                    type="button"
                    onClick={() => {
                      setEditing(false);
                      setEditError("");
                    }}
                    className="rounded-lg border border-gray-300 px-4 py-2 font-medium text-gray-700"
                  >
                    Cancel
                  </button>
                </div>
              </form>
            )}
          </div>
          <div className="mt-6 grid gap-6 md:grid-cols-2">
            <div>
              <p className="text-sm text-gray-500">Email</p>
              <p className="font-medium text-gray-900">{client.email}</p>
            </div>

            <div>
              <p className="text-sm text-gray-500">Phone</p>
              <p className="font-medium text-gray-900">{client.phone ?? "-"}</p>
            </div>

            <div>
              <p className="text-sm text-gray-500">Created</p>
              <p className="font-medium text-gray-900">
                {new Date(client.createdAt).toLocaleDateString()}
              </p>
            </div>

            <div>
              <p className="text-sm text-gray-500">Client ID</p>
              <p className="font-medium text-gray-900">{client.id}</p>
            </div>
          </div>
        </div>
        <div className="mt-8">
          <h2 className="mb-4 text-2xl font-bold text-gray-900">GoHighLevel</h2>

          <div className="rounded-xl bg-white p-6 shadow">
            {client.goHighLevelContactId ? (
              <div>
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      GoHighLevel Contact Linked
                    </h3>

                    <p className="mt-1 text-sm text-gray-500">
                      This client is connected to a GoHighLevel contact.
                    </p>
                  </div>

                  <span className="rounded-full bg-green-100 px-3 py-1 text-sm font-medium text-green-700">
                    Synced
                  </span>
                </div>

                <div className="mt-5">
                  <p className="text-sm text-gray-500">
                    GoHighLevel Contact ID
                  </p>

                  <p className="font-medium text-gray-900">
                    {client.goHighLevelContactId}
                  </p>
                </div>
                <div className="mt-6 border-t border-gray-200 pt-5">
                  <div className="flex items-center justify-between gap-4">
                    <div>
                      <h3 className="font-semibold text-gray-900">
                        Email & SMS Notifications
                      </h3>

                      <p className="mt-1 text-sm text-gray-500">
                        Allow ClientFlow to send notifications to this client
                        through GoHighLevel.
                      </p>
                    </div>

                    <button
                      type="button"
                      onClick={handleNotificationToggle}
                      disabled={updatingNotifications}
                      aria-pressed={client.notificationsEnabled}
                      className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors disabled:opacity-50 ${
                        client.notificationsEnabled
                          ? "bg-green-600"
                          : "bg-gray-300"
                      }`}
                    >
                      <span
                        className={`inline-block h-4 w-4 transform rounded-full bg-white transition-transform ${
                          client.notificationsEnabled
                            ? "translate-x-6"
                            : "translate-x-1"
                        }`}
                      />
                    </button>
                  </div>

                  <div className="mt-3">
                    <span
                      className={`rounded-full px-3 py-1 text-sm font-medium ${
                        client.notificationsEnabled
                          ? "bg-green-100 text-green-700"
                          : "text-red-600 bg-red-100"
                      }`}
                    >
                      {client.notificationsEnabled
                        ? "Notifications Enabled"
                        : "Notifications Disabled"}
                    </span>
                  </div>

                  {notificationError && (
                    <p className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-600">
                      {notificationError}
                    </p>
                  )}
                </div>
              </div>
            ) : (
              <div>
                <h3 className="text-lg font-semibold text-gray-900">
                  Not Connected to GoHighLevel
                </h3>

                <p className="mt-1 text-sm text-gray-500">
                  Link this client to GoHighLevel to enable CRM, email, and SMS
                  features.
                </p>

                <button
                  type="button"
                  onClick={() => setShowGhlConfirm(true)}
                  disabled={syncingGhl}
                  className="mt-5 rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white hover:opacity-90 disabled:opacity-50"
                >
                  {syncingGhl ? "Syncing..." : "Sync to GoHighLevel"}
                </button>
              </div>
            )}
          </div>
        </div>
        <div className="mt-8">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-2xl font-bold text-gray-900">Tax Cases</h2>

            <button
              type="button"
              onClick={() => navigate(`/clients/${client.id}/tax-cases/new`)}
              className="rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700"
            >
              Add Tax Case
            </button>
          </div>

          {taxCases.length === 0 ? (
            <div className="rounded-xl bg-white p-6 shadow">
              <p className="text-gray-500">
                No tax cases found for this client.
              </p>
            </div>
          ) : (
            <div className="overflow-hidden rounded-xl bg-white shadow">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Tax Year
                    </th>

                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Status
                    </th>

                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Missing Documents
                    </th>

                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Progress
                    </th>
                  </tr>
                </thead>

                <tbody>
                  {taxCases.map((taxCase) => (
                    <tr
                      key={taxCase.id}
                      onClick={() => navigate(`/tax-cases/${taxCase.id}`)}
                      className="border-t border-gray-200"
                    >
                      <td className="px-6 py-4">{taxCase.taxYear}</td>

                      <td className="px-6 py-4">
                        <TaxCaseStatusBadge status={taxCase.status} />
                      </td>

                      <td className="px-6 py-4">
                        {taxCase.missingDocumentsCount}
                      </td>

                      <td className="px-6 py-4">
                        {taxCase.documentProgressPercentage}%
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
        <div className="mt-8">
          <h2 className="mb-4 text-2xl font-bold text-gray-900">
            Portal Account
          </h2>

          {client.portalUserId ? (
            <div className="rounded-xl bg-white p-6 shadow">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">
                    Portal Account Active
                  </h3>

                  <p className="mt-1 text-sm text-gray-500">
                    This client already has a portal login account.
                  </p>
                </div>

                <span className="rounded-full bg-green-100 px-3 py-1 text-sm font-medium text-green-700">
                  Active
                </span>
              </div>

              <div className="mt-5">
                <p className="text-sm text-gray-500">Login Email</p>
                <p className="font-medium text-gray-900">{client.email}</p>
              </div>

              <button
                type="button"
                onClick={() => setShowResetPassword((current) => !current)}
                className="mt-5 rounded-lg bg-blue-50 px-4 py-2 text-sm font-medium text-blue-600 hover:bg-blue-100"
              >
                Reset Password
              </button>

              <button
                type="button"
                onClick={() => setShowUnlinkConfirm(true)}
                disabled={unlinkingPortalUser}
                className="ml-2 mt-5 rounded-lg bg-red-50 px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-100 disabled:opacity-50"
              >
                {unlinkingPortalUser ? "Unlinking..." : "Unlink Portal Account"}
              </button>

              {showResetPassword && (
                <form
                  onSubmit={handleResetPortalPassword}
                  className="mt-4 rounded-lg bg-gray-50 p-4"
                >
                  <label
                    htmlFor="resetPortalPassword"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    New Temporary Password
                  </label>

                  <input
                    id="resetPortalPassword"
                    type="password"
                    value={resetPassword}
                    onChange={(event) => setResetPassword(event.target.value)}
                    className="w-full rounded-lg border border-gray-300 px-4 py-3"
                  />

                  <div className="mt-3 flex gap-2">
                    <button
                      type="submit"
                      disabled={resettingPassword}
                      className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                    >
                      {resettingPassword ? "Resetting..." : "Reset Password"}
                    </button>

                    <button
                      type="button"
                      onClick={() => {
                        setShowResetPassword(false);
                        setResetPassword("");
                      }}
                      className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700"
                    >
                      Cancel
                    </button>
                  </div>
                </form>
              )}
            </div>
          ) : checkingPortalUser ? (
            <div className="rounded-xl bg-white p-6 shadow">
              <p className="text-sm text-gray-500">
                Checking for an existing portal account...
              </p>
            </div>
          ) : existingPortalUser ? (
            <div className="rounded-xl bg-white p-6 shadow">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">
                    Existing Portal Account Found
                  </h3>

                  <p className="mt-1 text-sm text-gray-500">
                    A portal account already exists with this client's email.
                  </p>
                </div>

                <span className="rounded-full bg-yellow-100 px-3 py-1 text-sm font-medium text-yellow-700">
                  Existing
                </span>
              </div>

              <div className="mt-5 rounded-lg bg-gray-50 p-4">
                <p className="text-sm text-gray-500">Email</p>

                <p className="font-medium text-gray-900">
                  {existingPortalUser.email}
                </p>

                <p className="mt-3 text-sm text-gray-500">Portal User ID</p>

                <p className="break-all text-sm text-gray-700">
                  {existingPortalUser.id}
                </p>
              </div>

              {existingPortalUser.isAssigned ? (
                <p className="mt-4 rounded-lg bg-red-50 p-3 text-sm font-medium text-red-600">
                  This portal account is already assigned to another client.
                </p>
              ) : (
                <button
                  type="button"
                  onClick={() => handleAssignPortalUser(existingPortalUser)}
                  disabled={assigningPortalUser}
                  className="mt-5 rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700 disabled:opacity-50"
                >
                  {assigningPortalUser
                    ? "Assigning..."
                    : "Assign Existing Portal Account"}
                </button>
              )}
            </div>
          ) : (
            <form
              onSubmit={handleCreatePortalAccount}
              className="rounded-xl bg-white p-6 shadow"
            >
              <h3 className="text-lg font-semibold text-gray-900">
                Create Portal Account
              </h3>

              <p className="mt-1 text-sm text-gray-500">
                No existing portal account was found for this client.
              </p>

              <div className="mt-5 grid gap-4 md:grid-cols-2">
                <div>
                  <label
                    htmlFor="portalEmail"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    Login Email
                  </label>

                  <input
                    id="portalEmail"
                    type="email"
                    value={client.email}
                    disabled
                    className="w-full rounded-lg border border-gray-300 bg-gray-100 px-4 py-3 text-gray-600"
                  />
                </div>

                <div>
                  <label
                    htmlFor="portalPassword"
                    className="mb-2 block text-sm font-medium text-gray-700"
                  >
                    Temporary Password
                  </label>

                  <input
                    id="portalPassword"
                    type="password"
                    value={portalPassword}
                    onChange={(event) => setPortalPassword(event.target.value)}
                    placeholder="Enter temporary password"
                    className="w-full rounded-lg border border-gray-300 px-4 py-3"
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={creatingPortalAccount}
                className="mt-4 rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700 disabled:opacity-50"
              >
                {creatingPortalAccount
                  ? "Creating..."
                  : "Create Portal Account"}
              </button>
            </form>
          )}
        </div>
      </div>
      {showPasswordResetSuccess && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <h3 className="text-xl font-bold text-gray-900">
              Password Updated
            </h3>

            <p className="mt-3 text-sm text-gray-600">
              The portal password has been changed successfully.
            </p>

            <p className="mt-2 text-sm text-gray-500">
              The client can now log in using the new password.
            </p>

            <div className="mt-6 flex justify-end">
              <button
                type="button"
                onClick={() => setShowPasswordResetSuccess(false)}
                className="rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700"
              >
                OK
              </button>
            </div>
          </div>
        </div>
      )}

      {showGhlConfirm && client && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h3 className="text-xl font-bold text-gray-900">
                  Sync to GoHighLevel
                </h3>

                <p className="mt-2 text-sm text-gray-500">
                  This will add the client to your GoHighLevel account.
                </p>
              </div>

              <button
                type="button"
                onClick={() => setShowGhlConfirm(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                ✕
              </button>
            </div>

            <div className="mt-5 rounded-xl bg-gray-50 p-4">
              <p className="text-sm text-gray-500">Client</p>

              <p className="mt-1 font-semibold text-gray-900">
                {client.firstName} {client.lastName}
              </p>

              <p className="mt-1 text-sm text-gray-600">{client.email}</p>
            </div>

            <p className="mt-4 text-sm text-gray-600">
              After syncing, CRM, email, and SMS features can be enabled for
              this client.
            </p>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setShowGhlConfirm(false)}
                disabled={syncingGhl}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                type="button"
                onClick={handleSyncToGoHighLevel}
                disabled={syncingGhl}
                className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:opacity-50"
              >
                {syncingGhl ? "Syncing..." : "Sync to GoHighLevel"}
              </button>
            </div>
          </div>
        </div>
      )}
      {showUnlinkConfirm && client && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h3 className="text-xl font-bold text-gray-900">
                  Unlink Portal Account
                </h3>

                <p className="mt-2 text-sm text-gray-500">
                  This will remove the portal account connection from this
                  client.
                </p>
              </div>

              <button
                type="button"
                onClick={() => setShowUnlinkConfirm(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                ✕
              </button>
            </div>

            <div className="mt-5 rounded-xl bg-red-50 p-4">
              <p className="text-sm font-medium text-red-700">
                {client.firstName} {client.lastName}
              </p>

              <p className="mt-1 text-sm text-red-600">{client.email}</p>
            </div>

            <p className="mt-4 text-sm text-gray-600">
              The client will no longer be able to access this account through
              the current client record until it is linked again.
            </p>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setShowUnlinkConfirm(false)}
                disabled={unlinkingPortalUser}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
              >
                Cancel
              </button>

              <button
                type="button"
                onClick={handleUnassignPortalUser}
                disabled={unlinkingPortalUser}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {unlinkingPortalUser ? "Unlinking..." : "Unlink Portal Account"}
              </button>
            </div>
          </div>
        </div>
      )}

      <ConfirmModal
        open={showUnlinkConfirm}
        title="Unlink Portal Account"
        message="Are you sure you want to unlink this portal account?"
        confirmText="Unlink Portal Account"
        danger
        loading={unlinkingPortalUser}
        onCancel={() => setShowUnlinkConfirm(false)}
        onConfirm={handleUnassignPortalUser}
      />

      {toast && (
        <Toast
          message={toast.message}
          type={toast.type}
          onClose={() => setToast(null)}
        />
      )}
    </div>
  );
}

export default ClientDetailsPage;
