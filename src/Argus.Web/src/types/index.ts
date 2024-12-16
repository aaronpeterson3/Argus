export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export interface Tenant {
  id: string;
  name: string;
  isActive: boolean;
  userCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface AuthResponse {
  user: User;
  token: string;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
}

export interface DashboardStats {
  totalUsers: number;
  activeTenants: number;
  pendingInvites: number;
}