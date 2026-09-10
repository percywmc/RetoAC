import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ErrorMessage, LoadingSpinner } from '../components/Feedback';

export default function LoginPage() {
  const { login, loading, error, setError } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    if (!email.trim() || !password) return;

    const ok = await login(email.trim(), password);
    if (ok) navigate('/cargas', { replace: true });
  };

  return (
    <div className="login-page">
      <div className="login-card">
        {/* Brand */}
        <div className="login-logo">
          <div className="brand-icon" aria-hidden="true">🎰</div>
          <h1>Atlantic City</h1>
          <p>Casino Sports — Panel de Cargas Masivas</p>
        </div>

        {/* Error */}
        <ErrorMessage message={error} onDismiss={() => setError(null)} />

        {/* Form */}
        <form className="login-form" onSubmit={handleSubmit} noValidate>
          <div className="form-group">
            <label className="form-label" htmlFor="login-email">Correo electrónico</label>
            <input
              id="login-email"
              type="email"
              className="form-input"
              placeholder="usuario@ejemplo.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
              autoFocus
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="login-password">Contraseña</label>
            <input
              id="login-password"
              type="password"
              className="form-input"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete="current-password"
            />
          </div>

          <button
            id="btn-login-submit"
            type="submit"
            className="btn btn-primary btn-lg btn-block"
            disabled={loading || !email || !password}
          >
            {loading ? (
              <>
                <LoadingSpinner size="sm" />
                Iniciando sesión...
              </>
            ) : (
              'Iniciar sesión'
            )}
          </button>
        </form>

        <p className="text-center text-sm text-muted" style={{ marginTop: '1.5rem' }}>
          © {new Date().getFullYear()} Atlantic City Casino Sports
        </p>
      </div>
    </div>
  );
}
