import { createContext, useContext, useState } from 'react';
import axios from 'axios';

interface AuthContextType {
  user: any | null;
  login: (email: string, password: string) => Promise<void>;
  register: (userData: any) => Promise<void>;
  requestPasswordReset: (email: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<any | null>(null);

  const login = async (email: string, password: string) => {
    try {
      const response = await axios.post('/api/auth/login', { email, password });
      const { token, user } = response.data;
      localStorage.setItem('token', token);
      setUser(user);
    } catch (error) {
      throw new Error('Login failed');
    }
  };

  const register = async (userData: any) => {
    try {
      await axios.post('/api/auth/register', userData);
    } catch (error) {
      throw new Error('Registration failed');
    }
  };

  const requestPasswordReset = async (email: string) => {
    try {
      await axios.post('/api/auth/reset-password', { email });
    } catch (error) {
      throw new Error('Password reset request failed');
    }
  };

  const logout = () => {
    localStorage.removeItem('token');
    setUser(null);
  };

  const value = {
    user,
    login,
    register,
    requestPasswordReset,
    logout
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};