import { API_BASE_URL } from './config';
import type {
  LoginCommand,
  LoginResponse,
  RegisterCommand,
  RegisterResponse,
  TopUpBalanceCommand,
  BalanceDto,
  PaymentDtoPaginatedList,
  PaymentDto,
  TicketDto,
  TicketTypeDto,
  PurchaseTicketCommand,
  ValidateTicketCommand,
  TicketDtoPaginatedList,
  Vehicle,
  VehicleCommand,
  Line,
  LineCommand,
  Stop,
  LineDetailsDto, 
  PaginatedList, 
} from '../types';

class ApiService {
  private baseUrl: string;
  private token: string | null = null;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }

  public setToken(token: string | null) {
    this.token = token;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {},
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;

    const HeadersCtor = (globalThis as any).Headers;
    let headers: any;
    if (HeadersCtor) {
      headers = new HeadersCtor(options.headers as any);
      if (!headers.has('Content-Type')) {
        headers.set('Content-Type', 'application/json');
        if (this.token && !headers.has('Authorization')) {
          headers.set('Authorization', `Bearer ${this.token}`);
        }
      }
    } else {
      headers = {
        'Content-Type': 'application/json',
        ...(this.token ? { Authorization: `Bearer ${this.token}` } : {}),
        ...(options.headers as any),
      };
    }

    try {
      console.log(`API Request: ${options.method || 'GET'} ${url}`);

      const response = await fetch(url, {
        ...options,
        headers,
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.log('API ERROR STATUS:', response.status);
        console.log('API ERROR URL:', url);
        console.log('API ERROR BODY:', errorText);
        let extractedMessage = errorText || response.statusText;

        try {
          const errorJson = JSON.parse(errorText);
          extractedMessage =
            errorJson.message ||
            errorJson.detail ||
            errorJson.title ||
            extractedMessage;
        } catch (e) {}

        throw new Error(extractedMessage);
      }

      if (response.status === 204) {
        return {} as T;
      }

      const data = await response.json();
      console.log(`API Response:`, data);
      return data;
    } catch (error) {
      console.error('API Error:', error);
      throw error;
    }
  }

  async login(data: LoginCommand): Promise<LoginResponse> {
    const response = await this.request<LoginResponse>('/users/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });

    if (response.token) {
      this.setToken(response.token);
    }

    return response;
  }

  async register(data: RegisterCommand): Promise<RegisterResponse> {
    const response = await this.request<RegisterResponse>('/users/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });

    return response;
  }

  async getBalance(): Promise<BalanceDto> {
    return await this.request<BalanceDto>('/payments/balance', {
      method: 'GET',
    });
  }

  async topUp(data: TopUpBalanceCommand): Promise<TopUpBalanceCommand> {
    return await this.request<TopUpBalanceCommand>('/payments/top-up', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async getPayments(
    pageNumber: number = 1,
    pageSize: number = 10,
  ): Promise<PaymentDtoPaginatedList> {
    return await this.request<PaymentDtoPaginatedList>(
      `/payments?PageNumber=${pageNumber}&PageSize=${pageSize}`,
      { method: 'GET' },
    );
  }

  async getPayment(id: number): Promise<PaymentDto> {
    return await this.request<PaymentDto>(`/payments/${id}`, {
      method: 'GET',
    });
  }

  async getActiveTicket(): Promise<TicketDto[]> {
    return await this.request<TicketDto[]>('/tickets/my/active', {
      method: 'GET',
    });
  }

  // --- BILETY ---
  async getTicketTypes(): Promise<TicketTypeDto[]> {
    return await this.request<TicketTypeDto[]>('/tickets/types', { method: 'GET' });
  }

  async purchaseTicket(command: PurchaseTicketCommand): Promise<number> {
    return await this.request<number>('/tickets/purchase', {
      method: 'POST',
      body: JSON.stringify(command),
    });
  }
  async validateTicket(ticketId: number, vehicleId: number): Promise<void> {
    await this.request<void>(`/tickets/${ticketId}/validate`, {
      method: 'PATCH',
      body: JSON.stringify({ticketId, vehicleId}),
    });
  }

  async createTicket(data: TicketTypeDto): Promise<TicketTypeDto> {
    return await this.request<TicketTypeDto>('/tickets/types', { 
      method: 'POST', 
      body: JSON.stringify(data) 
    });
  }

  async updateTicket(id: number, data: TicketTypeDto): Promise<TicketTypeDto> {
    return await this.request<TicketTypeDto>(`/tickets/types/${id}`, { 
      method: 'PUT', 
      body: JSON.stringify(data) 
    });
  }

  async deleteTicket(id: number): Promise<void> {
    return await this.request<void>(`/tickets/types/${id}`, { method: 'DELETE' });
  }

  // --- POJAZDY ---
  async getVehicles(): Promise<Vehicle[]> {
    return await this.request<Vehicle[]>('/vehicles', { method: 'GET' });
  }

  async createVehicle(data: VehicleCommand): Promise<Vehicle> {
    return await this.request<Vehicle>('/vehicles', { 
      method: 'POST', 
      body: JSON.stringify(data) 
    });
  }

  async updateVehicle(id: number, data: VehicleCommand): Promise<Vehicle> {
    return await this.request<Vehicle>(`/vehicles/${id}`, { 
      method: 'PUT', 
      body: JSON.stringify(data) 
    });
  }

  async deleteVehicle(id: number): Promise<void> {
    return await this.request<void>(`/vehicles/${id}`, { method: 'DELETE' });
  }

  // --- LINIE / PRZYSTANKI ---
  async getLines(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    isDescending: boolean = false
  ): Promise<PaginatedList<Line>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      isDescending: isDescending.toString(),
      ...(searchTerm && { searchTerm })
    });

    return await this.request<PaginatedList<Line>>(`/lines?${params.toString()}`, { method: 'GET' });
  }

  async createLine(data: LineCommand): Promise<Line> {
    return await this.request<Line>('/lines', { 
      method: 'POST', 
      body: JSON.stringify(data) 
    });
  }

  async updateLine(id: number, data: LineCommand): Promise<Line> {
    return await this.request<Line>(`/lines/${id}`, { 
      method: 'PUT', 
      body: JSON.stringify(data) 
    });
  }

  async deleteLine(id: number): Promise<void> {
    return await this.request<void>(`/lines/${id}`, { method: 'DELETE' });
  }

  async getLineDetail(id: number): Promise<LineDetailsDto> {
  return await this.request<LineDetailsDto>(`/lines/${id}`, { method: 'GET' });
}

async getLineStops(id: number): Promise<Stop[]> {
  return await this.request<Stop[]>(`/lines/${id}/stops`, { method: 'GET' });
}

  async getTicketHistory(pageNumber: number = 1, pageSize: number = 10): Promise<TicketDtoPaginatedList> {
    return await this.request<TicketDtoPaginatedList>(
      `/tickets/my/history?PageNumber=${pageNumber}&PageSize=${pageSize}`,
      { method: 'GET' }
    );
  }
}

export default new ApiService();
