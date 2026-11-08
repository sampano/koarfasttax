export type TaxCaseActivity = {
  id: number;
  taxCaseId: number;
  action: string;
  description: string | null;
  userName: string;
  createdAt: string;
};
