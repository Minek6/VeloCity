export type RootStackParamList = {
  Login: undefined;
  Register: undefined;
  Main: {
    screen?: string;
    params?: {
      screen: string;
    };
  };
  PurchaseTicket: undefined;
  TicketHistory: undefined;
  ValidateTicket: {ticketId: number};
};

export type MainTabParamList = {
  Dashboard: { userName: string };
  Routes: undefined;
  Wallet: undefined;
  Profile: undefined;
};
