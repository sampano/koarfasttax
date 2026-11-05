import { useEffect, useState } from "react";
//import { apiFetch } from "../api/apiClient";
import { useNavigate } from "react-router-dom";
import type { Client, GoHighLevelContact } from "../types/client";
import {
  getClients,
  getGoHighLevelContacts,
  importGoHighLevelContact,
} from "../api/clients";

function ClientsPage() {
  // State variables for clients, error, and loading state
  const [clients, setClients] = useState<Client[]>([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  // State variables for search, pagination, and total count
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);

  // State variables for GoHighLevel contacts and related UI state
  const [showGhlContacts, setShowGhlContacts] = useState(false);
  const [ghlContacts, setGhlContacts] = useState<GoHighLevelContact[]>([]);
  const [loadingGhlContacts, setLoadingGhlContacts] = useState(false);
  const [ghlError, setGhlError] = useState("");
  const [ghlSearch, setGhlSearch] = useState("");
  const [importingContactId, setImportingContactId] = useState<string | null>(
    null,
  );

  // State variables for contact import confirmation
  const [contactToImport, setContactToImport] =
    useState<GoHighLevelContact | null>(null);
  const [showImportConfirm, setShowImportConfirm] = useState(false);

  // Hook for navigation
  const navigate = useNavigate();

  async function loadClients() {
    setLoading(true);
    setError("");

    try {
      const data = await getClients(search, page, pageSize);

      setClients(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (error) {
      console.error(error);
      setError("Unable to load clients.");
    } finally {
      setLoading(false);
    }
  }

  function handleOpenGhlContacts() {
    setShowGhlContacts(true);
    setGhlContacts([]);
    setGhlError("");
    setGhlSearch("");
  }
  async function handleImportGhlContact() {
    if (!contactToImport) {
      return;
    }

    setImportingContactId(contactToImport.id);
    setGhlError("");

    try {
      await importGoHighLevelContact(contactToImport.id);

      setShowImportConfirm(false);
      setContactToImport(null);

      const data = await getGoHighLevelContacts(ghlSearch);
      setGhlContacts(data);

      await loadClients();
    } catch (error) {
      setGhlError(
        error instanceof Error
          ? error.message
          : "Unable to import GoHighLevel contact.",
      );
    } finally {
      setImportingContactId(null);
    }
  }
  async function handleGhlSearch(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const keyword = ghlSearch.trim();

    if (keyword.length < 2) {
      setGhlError("Enter at least 2 characters.");
      setGhlContacts([]);
      return;
    }

    setLoadingGhlContacts(true);
    setGhlError("");

    try {
      const data = await getGoHighLevelContacts(keyword);

      setGhlContacts(data);
    } catch (error) {
      setGhlError(
        error instanceof Error
          ? error.message
          : "Unable to search GoHighLevel contacts.",
      );
    } finally {
      setLoadingGhlContacts(false);
    }
  }
  useEffect(() => {
    loadClients();
  }, [page]);

  function handleSearch(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (page === 1) {
      loadClients();
    } else {
      setPage(1);
    }
  }
  if (loading) {
    return <p className="p-8">Loading clients...</p>;
  }

  if (error) {
    return <p className="p-8 text-red-600">{error}</p>;
  }

  return (
    <div className="min-h-screen bg-gray-100 p-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Clients</h1>

        <div className="flex gap-3">
          <button
            type="button"
            onClick={handleOpenGhlContacts}
            className="rounded-lg border border-[#008619b9] px-4 py-2 font-medium text-[#008619b9] hover:bg-green-50"
          >
            Import from GoHighLevel
          </button>

          <button
            type="button"
            onClick={() => navigate("/clients/new")}
            className="rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white hover:opacity-90"
          >
            Add Client
          </button>
        </div>
      </div>
      <form onSubmit={handleSearch} className="mb-6 flex max-w-xl gap-3">
        <input
          type="text"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search clients..."
          className="flex-1 rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500"
        />

        <button
          type="submit"
          className="rounded-lg bg-blue-600 px-5 py-3 font-medium text-white hover:bg-blue-700"
        >
          Search
        </button>
      </form>
      <div className="overflow-hidden rounded-xl bg-white shadow">
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                Name
              </th>
              <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                Email
              </th>
              <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                Phone
              </th>
            </tr>
          </thead>

          <tbody>
            {clients.map((client) => (
              <tr
                key={client.id}
                onClick={() => navigate(`/clients/${client.id}`)}
                className="cursor-pointer border-t border-gray-200 hover:bg-gray-50"
              >
                <td className="px-6 py-4">
                  {client.firstName} {client.lastName}
                </td>

                <td className="px-6 py-4">{client.email}</td>

                <td className="px-6 py-4">{client.phone ?? "-"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className="mt-4 flex items-center justify-between">
        <p className="text-sm text-gray-500">
          {totalCount} client{totalCount === 1 ? "" : "s"}
        </p>

        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={() => setPage((current) => current - 1)}
            disabled={page <= 1}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
          >
            Previous
          </button>

          <span className="text-sm text-gray-600">
            Page {page} of {totalPages || 1}
          </span>

          <button
            type="button"
            onClick={() => setPage((current) => current + 1)}
            disabled={page >= totalPages}
            className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
      {showGhlContacts && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-2xl rounded-xl bg-white shadow-xl">
            <div className="flex items-center justify-between border-b border-gray-200 p-5">
              <div>
                <h2 className="text-xl font-bold text-gray-900">
                  Import from GoHighLevel
                </h2>

                <p className="mt-1 text-sm text-gray-500">
                  Select a GoHighLevel contact to add to ClientFlow.
                </p>
              </div>

              <button
                type="button"
                onClick={() => setShowGhlContacts(false)}
                className="text-xl text-gray-500 hover:text-gray-900"
              >
                ×
              </button>
            </div>
            <div className="border-b border-gray-200 p-5">
              <form onSubmit={handleGhlSearch} className="flex gap-3">
                <input
                  type="text"
                  value={ghlSearch}
                  onChange={(event) => setGhlSearch(event.target.value)}
                  placeholder="Search name, email, or phone..."
                  className="flex-1 rounded-lg border border-gray-300 px-4 py-3"
                />

                <button
                  type="submit"
                  disabled={loadingGhlContacts}
                  className="rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white disabled:opacity-50"
                >
                  {loadingGhlContacts ? "Searching..." : "Search"}
                </button>
              </form>
            </div>
            <div className="max-h-[500px] overflow-y-auto p-5">
              {ghlError && (
                <p className="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-600">
                  {ghlError}
                </p>
              )}

              {loadingGhlContacts ? (
                <p className="text-gray-500">Loading GoHighLevel contacts...</p>
              ) : ghlContacts.length === 0 ? (
                <p className="text-gray-500">No GoHighLevel contacts found.</p>
              ) : (
                <div className="space-y-3">
                  {ghlContacts.map((contact) => (
                    <div
                      key={contact.id}
                      className="flex items-center justify-between rounded-lg border border-gray-200 p-4"
                    >
                      <div>
                        <p className="font-semibold text-gray-900">
                          {contact.firstName || "No first name"}{" "}
                          {contact.lastName}
                        </p>

                        <p className="text-sm text-gray-500">
                          {contact.email || "No email"}
                        </p>

                        <p className="text-sm text-gray-500">
                          {contact.phone || "No phone"}
                        </p>
                      </div>

                      {contact.isImported ? (
                        <span className="rounded-full bg-green-100 px-3 py-1 text-sm font-medium text-green-700">
                          Imported
                        </span>
                      ) : (
                        <button
                          type="button"
                          onClick={() => {
                            setContactToImport(contact);
                            setShowImportConfirm(true);
                          }}
                          className="rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white"
                        >
                          Import
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
      {showImportConfirm && contactToImport && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <h3 className="text-xl font-bold text-gray-900">Import Contact</h3>

            <p className="mt-3 text-gray-600">
              Are you sure you want to import
              <span className="font-semibold text-gray-900">
                {" "}
                {contactToImport.firstName} {contactToImport.lastName}
              </span>{" "}
              into ClientFlow?
            </p>

            <div className="mt-4 rounded-lg bg-gray-50 p-4">
              <p className="font-medium text-gray-900">
                {contactToImport.firstName} {contactToImport.lastName}
              </p>

              <p className="mt-1 text-sm text-gray-500">
                {contactToImport.email || "No email"}
              </p>

              <p className="text-sm text-gray-500">
                {contactToImport.phone || "No phone"}
              </p>
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => {
                  setShowImportConfirm(false);
                  setContactToImport(null);
                }}
                disabled={importingContactId !== null}
                className="rounded-lg border border-gray-300 px-4 py-2 font-medium text-gray-700 hover:bg-gray-50"
              >
                Cancel
              </button>

              <button
                type="button"
                onClick={handleImportGhlContact}
                disabled={importingContactId !== null}
                className="rounded-lg bg-[#008619b9] px-4 py-2 font-medium text-white hover:opacity-90 disabled:opacity-50"
              >
                {importingContactId ? "Importing..." : "Import Contact"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ClientsPage;
