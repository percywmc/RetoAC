import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const initials = (user?.email ?? '?')
    .split('@')[0]
    .slice(0, 2)
    .toUpperCase();

  return (
    <nav className="navbar" role="navigation" aria-label="Navegación principal">
      <NavLink to="/cargas" className="navbar-brand">
        <div className="navbar-logo-icon" aria-hidden="true">🎰</div>
        <span className="logo-text">Atlantic City</span>
      </NavLink>

      <div className="navbar-nav">
        <NavLink
          to="/cargas"
          id="nav-historial"
          className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
        >
          📋 Historial
        </NavLink>
        <NavLink
          to="/subida"
          id="nav-subida"
          className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
        >
          📤 Subir Excel
        </NavLink>
      </div>

      <div className="navbar-user">
        <div className="navbar-avatar" title={user?.email}>{initials}</div>
        <span className="text-sm" style={{ display: 'none' }}>{user?.email}</span>
        <button
          id="btn-logout"
          onClick={handleLogout}
          className="btn btn-ghost btn-sm"
          title="Cerrar sesión"
        >
          Salir
        </button>
      </div>
    </nav>
  );
}
