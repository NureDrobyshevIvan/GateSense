// DTO інтерфейси, що відповідають форматам відповідей бекенду

export interface ApiResponse<T> {
  data: T;
}

export interface LoginRequest {
  login: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  password: string;
}

export interface LoginResponse {
  userName?: string;
  email?: string;
  role: string;
  accessToken?: string;
  refreshToken?: string;
}

export interface UserProfile {
  id?: number;
  userName?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
}

export interface Garage {
  id: number;
  name: string;
  address?: string | null;
  timeZone?: string | null;
  ownerId?: number | null;
}

export interface CreateGarageRequest {
  name: string;
  address?: string;
  timeZone?: string;
}

export interface UpdateGarageRequest {
  name: string;
  address?: string;
  timeZone?: string;
}

export interface GateStateResponse {
  garageId: number;
  state: string;
  lastAction?: string | null;
  lastActionTime?: string | null;
}

export interface GateCommandRequest {
  accessKeyToken?: string;
}

export interface SensorReading {
  id?: number;
  deviceId?: number;
  sensorType?: number;
  value: number;
  unit?: string | null;
  recordedOn?: string | null;
}

export interface Paginated<T> {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export interface GarageMember {
  id: number;
  garageId: number;
  userId?: number;
  accessLevel?: number;
  expiresOn?: string | null;
  user?: { id?: number; userName?: string; email?: string } | null;
}

export interface AssignFamilyAccessRequest {
  garageId: number;
  email: string;
}

export interface CreateGuestAccessRequest {
  garageId: number;
  recipientName: string;
  recipientEmail?: string;
  expiresOn?: string;
}

export interface CreateGuestAccessResponse {
  id: number;
  token: string;
  expiresOn?: string | null;
}

export interface GateEvent {
  id: number;
  garageId: number;
  initiatorUserId?: number | null;
  accessKeyId?: number | null;
  triggerSource?: number | null;
  action?: number | null;
  result?: number | null;
  failureReason?: string | null;
  createdOn?: string | null;
}

export interface AdminUser {
  id: number;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  emailConfirmed: boolean;
  roles: string[];
}

export interface AdminUpdateUserRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  emailConfirmed?: boolean;
}

export interface AssignRoleRequest {
  role: string;
}

export interface AdminGarage {
  id: number;
  name: string;
  address?: string;
  timeZone?: string;
  ownerId?: number;
  ownerUserName?: string;
  ownerEmail?: string;
}

export interface ImportBackupResponse {
  usersImported: number;
  garagesImported: number;
  devicesImported: number;
  accessKeysImported: number;
  garageAccessImported: number;
  gateEventsImported: number;
  sensorReadingsImported: number;
}

// Helper для конвертації числових enum-ів у читабельні рядки
export const SensorTypeLabels: Record<number, string> = {
  0: 'CO',
  1: 'Smoke',
  2: 'Temperature',
  3: 'Humidity'
};

export const TriggerSourceLabels: Record<number, string> = {
  0: 'Owner',
  1: 'Family',
  2: 'Guest',
  3: 'System'
};

export const GateActionLabels: Record<number, string> = {
  0: 'Open',
  1: 'Close'
};

export const GateResultLabels: Record<number, string> = {
  0: 'Success',
  1: 'Failure'
};

export const AccessLevelLabels: Record<number, string> = {
  0: 'Owner',
  1: 'Family',
  2: 'Guest'
};
