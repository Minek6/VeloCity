export interface TicketDto { 
  id: number;
  ticketTypeName: string;
  price: number;
  purchasedAt: string;
  validFrom: string | null;
  validTo: string | null;
  vehicleId: number | null;
  isValidated: boolean;
}

export interface TicketTypeDto {
  id: number;
  name: string;
  price: number;
  durationInMinutes: number;
  zoneLimit: number;
}

export interface PurchaseTicketCommand {
  ticketTypeId: number;
}

export interface ValidateTicketCommand {
  ticketId: number;
}

export interface TicketDtoPaginatedList {
  items: TicketDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface TicketVerificationResponse {
  isValid: boolean;
  message: string;
  ticketTypeName: string | null;
  validTo: string | null;
}
