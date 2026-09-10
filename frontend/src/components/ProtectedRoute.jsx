import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LoadingSpinner } from './Feedback';

/**
 * ProtectedRoute — redirige a /login si el usuario no está autenticado.
 * Muestra spinner mientras se determina el estado de sesión.
 */
export default function ProtectedRoute() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) return <LoadingSpinner size="lg" message="Verificando sesión..." />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;

  return <Outlet />;
}
