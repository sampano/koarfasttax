import { useEffect, useState, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";

import {
  getMyTaxCase,
  getMyTaxCaseDocuments,
  downloadMyDocument,
  deleteMyDocument,
  uploadMyDocument,
} from "../api/clientPortal";
import type { TaxCaseSummary } from "../types/taxCase";
import TaxCaseStatusBadge from "../components/TaxCaseStatusBadge";

import type { TaxDocument } from "../types/document";
import Toast from "../components/Toast";
import ConfirmModal from "../components/ConfirmModal";

function getDocumentRequestStatus(
  status: "Pending" | "Uploaded" | "Completed",
) {
  switch (status) {
    case "Uploaded":
      return {
        label: "Uploaded - Waiting for Review",
        className: "bg-amber-50 text-amber-700",
      };

    case "Completed":
      return {
        label: "Completed",
        className: "bg-green-50 text-green-700",
      };

    default:
      return {
        label: "Pending",
        className: "bg-red-50 text-red-700",
      };
  }
}

function ClientTaxCaseDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [taxCase, setTaxCase] = useState<TaxCaseSummary | null>(null);
  const [loading, setLoading] = useState(true);

  const [documents, setDocuments] = useState<TaxDocument[]>([]);

  const [selectedDocumentTypeId, setSelectedDocumentTypeId] = useState("");
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

  const fileInputRef = useRef<HTMLInputElement>(null);

  const [documentToDelete, setDocumentToDelete] = useState<number | null>(null);
  const [deletingDocument, setDeletingDocument] = useState(false);

  // State variables for managing notification settings
  const [toast, setToast] = useState<{
    message: string;
    type: "success" | "error";
  } | null>(null);

  async function loadTaxCase() {
    try {
      const [taxCaseData, documentData] = await Promise.all([
        getMyTaxCase(id!),
        getMyTaxCaseDocuments(id!),
      ]);

      setTaxCase(taxCaseData);
      setDocuments(documentData);
    } catch (error) {
      console.error(error);
      showToast(
        error instanceof Error ? error.message : "Unable to load tax case.",
        "error",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadTaxCase();
  }, [id]);

  async function handleDownload(documentId: number, fileName: string) {
    try {
      const blob = await downloadMyDocument(documentId);

      const url = window.URL.createObjectURL(blob);

      const link = document.createElement("a");
      link.href = url;
      link.download = fileName;

      document.body.appendChild(link);
      link.click();
      link.remove();

      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error ? error.message : "Unable to download document.",
        "error",
      );
    }
  }

  async function handleDeleteDocument() {
    if (documentToDelete === null) {
      return;
    }

    setDeletingDocument(true);

    try {
      await deleteMyDocument(documentToDelete);

      await loadTaxCase();

      setDocumentToDelete(null);

      showToast("Document deleted successfully.", "success");
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error ? error.message : "Unable to delete document.",
        "error",
      );
    } finally {
      setDeletingDocument(false);
    }
  }

  async function handleUpload(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedFile || !selectedDocumentTypeId || !taxCase) {
      showToast("Please select a document type and file.", "error");
      return;
    }

    setUploading(true);

    try {
      await uploadMyDocument(taxCase.id, selectedFile, selectedDocumentTypeId);

      await loadTaxCase();

      setSelectedDocumentTypeId("");
      setSelectedFile(null);

      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    } catch (error) {
      console.error(error);

      showToast(
        error instanceof Error ? error.message : "Unable to upload document.",
        "error",
      );
    } finally {
      setUploading(false);
    }
  }
  if (loading) {
    return <div className="p-8">Loading...</div>;
  }

  if (!taxCase) {
    return null;
  }

  function showToast(message: string, type: "success" | "error") {
    setToast({
      message,
      type,
    });

    setTimeout(() => {
      setToast(null);
    }, 4000);
  }

  return (
    <div className="min-h-screen bg-gray-100 p-8">
      <div className="mx-auto max-w-5xl">
        <button
          type="button"
          onClick={() => navigate("/client/dashboard")}
          className="mb-6 text-sm font-medium text-blue-600 hover:text-blue-700"
        >
          ← Back to Dashboard
        </button>

        <div className="rounded-xl bg-white p-6 shadow">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Tax Case</p>

              <h1 className="text-3xl font-bold text-gray-900">
                Tax Year {taxCase.taxYear}
              </h1>
            </div>

            <TaxCaseStatusBadge status={taxCase.status} />
          </div>

          <div className="mt-8 grid gap-4 md:grid-cols-4">
            <div className="rounded-lg bg-gray-50 p-4">
              <p className="text-sm text-gray-500">Requested</p>
              <p className="mt-1 text-2xl font-bold">
                {taxCase.requestedDocumentsCount}
              </p>
            </div>

            <div className="rounded-lg bg-gray-50 p-4">
              <p className="text-sm text-gray-500">Completed</p>
              <p className="mt-1 text-2xl font-bold">
                {taxCase.completedDocumentsCount}
              </p>
            </div>

            <div className="rounded-lg bg-gray-50 p-4">
              <p className="text-sm text-gray-500">Missing</p>
              <p className="mt-1 text-2xl font-bold">
                {taxCase.missingDocumentsCount}
              </p>
            </div>

            <div className="rounded-lg bg-gray-50 p-4">
              <p className="text-sm text-gray-500">Progress</p>
              <p className="mt-1 text-2xl font-bold">
                {taxCase.documentProgressPercentage}%
              </p>
            </div>
          </div>
        </div>

        <div className="mt-8">
          <h2 className="mb-4 text-2xl font-bold text-gray-900">
            Required Documents
          </h2>

          {taxCase.documentRequests.length === 0 ? (
            <div className="rounded-xl bg-white p-6 shadow">
              <p className="text-gray-500">No documents have been requested.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {taxCase.documentRequests.map((document) => {
                const status = getDocumentRequestStatus(document.status);

                return (
                  <div
                    key={document.id}
                    className="rounded-xl bg-white p-5 shadow"
                  >
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <p className="font-semibold text-gray-900">
                          {document.documentType}
                        </p>

                        <p className="mt-1 text-sm text-gray-500">
                          {document.documentCategory}
                        </p>

                        {document.note && (
                          <p className="mt-3 text-sm text-gray-700">
                            {document.note}
                          </p>
                        )}
                      </div>

                      <span
                        className={`rounded-full px-3 py-1 text-xs font-semibold ${status.className}`}
                      >
                        {status.label}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
        <div className="mt-8">
          <h2 className="mb-4 text-2xl font-bold text-gray-900">
            Upload Document
          </h2>

          <form
            onSubmit={handleUpload}
            className="rounded-xl bg-white p-6 shadow"
          >
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <label
                  htmlFor="documentType"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  Document Type
                </label>

                <select
                  id="documentType"
                  value={selectedDocumentTypeId}
                  onChange={(event) =>
                    setSelectedDocumentTypeId(event.target.value)
                  }
                  className="w-full rounded-lg border border-gray-300 px-4 py-3"
                >
                  <option value="">Select document type</option>

                  {taxCase.missingDocuments.map((document) => (
                    <option
                      key={document.documentTypeId}
                      value={document.documentTypeId}
                    >
                      {document.documentType}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label
                  htmlFor="file"
                  className="mb-2 block text-sm font-medium text-gray-700"
                >
                  File
                </label>

                <input
                  ref={fileInputRef}
                  id="file"
                  type="file"
                  accept=".pdf,.jpg,.jpeg,.png"
                  onChange={(event) =>
                    setSelectedFile(event.target.files?.[0] ?? null)
                  }
                  className="w-full rounded-lg border border-gray-300 px-4 py-3"
                />
              </div>
            </div>

            <button
              type="submit"
              disabled={uploading}
              className="mt-5 rounded-lg bg-blue-600 px-4 py-2 font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {uploading ? "Uploading..." : "Upload Document"}
            </button>
          </form>
        </div>

        <div className="mt-8">
          <h2 className="mb-4 text-2xl font-bold text-gray-900">
            Uploaded Documents
          </h2>

          {documents.length === 0 ? (
            <div className="rounded-xl bg-white p-6 shadow">
              <p className="text-gray-500">No documents uploaded yet.</p>
            </div>
          ) : (
            <div className="overflow-hidden rounded-xl bg-white shadow">
              <table className="w-full">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      File
                    </th>

                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Type
                    </th>

                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Uploaded
                    </th>

                    <th className="px-6 py-4 text-left text-sm font-medium text-gray-600">
                      Actions
                    </th>
                  </tr>
                </thead>

                <tbody>
                  {documents.map((document) => (
                    <tr key={document.id} className="border-t border-gray-200">
                      <td className="px-6 py-4">{document.fileName}</td>

                      <td className="px-6 py-4">
                        {document.documentType ?? "-"}
                      </td>

                      <td className="px-6 py-4">
                        {new Date(document.uploadedAt).toLocaleDateString()}
                      </td>

                      <td className="px-6 py-4">
                        <div className="flex gap-2">
                          <button
                            type="button"
                            onClick={() =>
                              handleDownload(document.id, document.fileName)
                            }
                            className="rounded-lg bg-blue-50 px-3 py-2 text-sm font-medium text-blue-600 hover:bg-blue-100"
                          >
                            Download
                          </button>

                          <button
                            type="button"
                            onClick={() => setDocumentToDelete(document.id)}
                            className="rounded-lg bg-red-50 px-3 py-2 text-sm font-medium text-red-600 hover:bg-red-100"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      <ConfirmModal
        open={documentToDelete !== null}
        title="Delete Document"
        message="Are you sure you want to delete this document? This action cannot be undone."
        confirmText="Delete Document"
        danger
        loading={deletingDocument}
        onCancel={() => setDocumentToDelete(null)}
        onConfirm={handleDeleteDocument}
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

export default ClientTaxCaseDetailsPage;
