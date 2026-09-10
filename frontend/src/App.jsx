import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Navbar from './components/Navbar';
import LoginPage from './pages/LoginPage';
import HistorialPage from './pages/HistorialPage';
import SubidaPage from './pages/SubidaPage';
import DetallePage from './pages/DetallePage';

function AppLayout({ children }) {
  return (
    <div className="app-layout">
      <Navbar />
      <main className="main-content">{children}</main>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Página de login — pública */}
          <Route path="/login" element={<LoginPage />} />

          {/* Rutas protegidas — requieren JWT válido */}
          <Route element={<ProtectedRoute />}>
            <Route
              path="/cargas"
              element={
                <AppLayout>
                  <HistorialPage />
                </AppLayout>
              }
            />
            <Route
              path="/cargas/:id"
              element={
                <AppLayout>
                  <DetallePage />
                </AppLayout>
              }
            />
            <Route
              path="/subida"
              element={
                <AppLayout>
                  <SubidaPage />
                </AppLayout>
              }
            />
          </Route>

          {/* Fallback — redirige a historial (o login si no autenticado) */}
          <Route path="*" element={<Navigate to="/cargas" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
