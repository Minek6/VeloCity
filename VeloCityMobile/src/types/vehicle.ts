export interface Vehicle {
  id: number;
  sideNumber: string;
  model: string;
  isActive: boolean;
}

export interface VehicleCommand {
  name: string;
  registrationNumber: string;
  capacity: number;
}