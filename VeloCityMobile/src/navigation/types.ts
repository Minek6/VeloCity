export type SelectedConnection = {
  externalRunId: number;
  externalVariantId: number;
  lineNumber: string;
  fromStopId: number;
  fromStopName: string;
  toStopId: number;
  toStopName: string;
  departureTime: string;
  arrivalTime: string;
  durationMinutes: number;
  scheduledTripId: number;
  serviceDate: string;
};

export type RootStackParamList = {
  Login: undefined;
  Register: undefined;
  Main: {
    screen?: string;
    params?: {
      screen: string;
    };
  };
  PurchaseTicket: {
    connection?: SelectedConnection;
  } | undefined;
  TicketHistory: undefined;
  ValidateTicket: {ticketId: number};
};

export type MainTabParamList = {
  Dashboard: { userName: string };
  Routes: undefined;
  Wallet: undefined;
  Profile: undefined;
};
