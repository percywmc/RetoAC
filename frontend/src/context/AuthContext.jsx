import { createContext, useContext, useState, useCallback } from 'react';
import { login as apiLogin } from '../services/authService';
import { clearSession } from '../services/apiClient';

const AuthContext = createContext(null);

const readUser = () => {
  try {
    const raw = localStorage.getItem('ac_user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
};

export function AuthProvider({ children }) {
  const [user, setUser] = useState(readUser);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const isAuthenticated = !!user && !!localStorage.getItem('ac_access_token');

  const login = useCallback(async (email, password) => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiLogin(email, password);
      localStorage.setItem('ac_access_token', data.accessToken);
      localStorage.setItem('ac_refresh_token', data.refreshToken);
      localStorage.setItem('ac_user', JSON.stringify(data.user ?? { email }));
      setUser(data.user ?? { email });
      return true;
    } catch (err) {
      const msg =
        err.response?.data?.detail ??
        err.response?.data?.title ??
        err.response?.data?.message ??
        'Credenciales inválidas. Verifica tu email y contraseña.';
      setError(msg);
      return false;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setUser(null);
    setError(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, loading, error, setError, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
};
