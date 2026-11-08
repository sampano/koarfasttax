export type CaseAttention = {
  taxCaseId: number;
  clientId: number;
  clientName: string;
  taxYear: number;
  missingDocumentsCount: number;
};

export type CaseReview = {
  taxCaseId: number;
  clientId: number;
  clientName: string;
  taxYear: number;
  uploadedDocumentsCount: number;
};

export type RecentActivity = {
  documentId: number;
  fileName: string;
  documentType: string | null;
  clientName: string;
  taxCaseId: number;
  taxYear: number;
  uploadedAt: string;
};

export type TaxCaseStatusCount = {
  status: string;
  count: number;
};

export type DashboardSummary = {
  totalClients: number;
  totalTaxCases: number;
  waitingForDocumentsCount: number;
  completedTaxCasesCount: number;

  totalRequestedDocuments: number;
  totalCompletedDocuments: number;
  totalMissingDocuments: number;
  totalUploadedDocuments: number;

  overallDocumentProgressPercentage: number;

  casesNeedingAttention: CaseAttention[];
  casesNeedingReview: CaseReview[];

  recentActivity: RecentActivity[];
  taxCaseStatusBreakdown: TaxCaseStatusCount[];
};
