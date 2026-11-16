export interface VerifyQrResponse {
  isValid: boolean;
  status: string;
  ticketId?: number;
  validFrom?: string;
  validTo?: string;
}
