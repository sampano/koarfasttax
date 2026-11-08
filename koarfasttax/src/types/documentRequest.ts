export type DocumentRequestStatus = "Pending" | "Uploaded" | "Completed";

export type DocumentRequest = {
  id: number;
  taxCaseId: number;
  documentTypeId: number;
  documentType: string;
  documentCategory: string;
  note: string | null;
  status: DocumentRequestStatus;
  createdAt: string;
};

export type DocumentRequestSummary = {
  id: number;
  documentTypeId: number;
  documentType: string;
  documentCategory: string;
  note: string | null;
  status: DocumentRequestStatus;
};
