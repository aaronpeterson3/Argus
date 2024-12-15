export interface User {
  id: string;
  email: string;
  tenantId?: string;
  tenantName?: string;
  roles: string[];
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

export interface SystemInitRequest {
  tenantId: string;
  name: string;
  adminEmail: string;
  adminPassword: string;
}
