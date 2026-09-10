import { useState, useCallback, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { listarCargas } from '../services/cargasService';
import { usePolling, TERMINAL_STATES } from '../hooks/usePolling';
import EstadoBadge from '../components/EstadoBadge';
import { LoadingSpinner, ErrorMessage } from '../components/Feedback';

const PAGE_SIZE = 10;
const POLLING_MS = Number(import.meta.env.VITE_POLLING_INTERVAL_MS ?? 6000);

const formatFecha = (iso) =>
  iso ? new Date(iso).toLocaleString('es-CO', { dateStyle: 'short', timeStyle: 'short' }) : '—';

export default function HistorialPage() {
  const [cargas, setCargas] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  // ¿Hay cargas en estado no-terminal? → activar polling
  const hasActiveCargas = cargas.some(
    (c) => !TERMINAL_STATES.includes(c.estado)
  );

  const fetchCargas = useCallback(async () => {
    try {
      const data = await listarCargas(page, PAGE_SIZE);
      setCargas(data.items ?? []);
      setTotal(data.totalCount ?? 0);
      setError(null);
    } catch (err) {
      setError(
        err.response?.data?.detail ?? 'No se pudo obtener el historial de cargas.'
      );
    } finally {
      setLoading(false);
    }
  }, [page]);

  // Carga inicial
  useEffect(() => {
    setLoading(true);
    fetchCargas();
  }, [fetchCargas]);

  // Polling mientras haya cargas activas
  usePolling(fetchCargas, POLLING_MS, hasActiveCargas);

  if (loading) return <LoadingSpinner size="lg" message="Cargando historial..." />;

  return (
    <div>
      {/* Header */}
      <div className="page-header flex justify-between items-center">
        <div>
          <h1>Historial de Cargas</h1>
          <p>
            {total} registro{total !== 1 ? 's' : ''} en total
            {hasActiveCargas && (
              <span className="polling-indicator" style={{ marginLeft: '1rem' }}>
                <span className="polling-dot" /> Actualizando en tiempo real
              </span>
            )}
          </p>
        </div>
        <Link to="/subida" className="btn btn-primary" id="btn-nueva-carga">
          📤 Nueva carga
        </Link>
      </div>

      <ErrorMessage message={error} onDismiss={() => setError(null)} />

      {/* Table */}
      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        {cargas.length === 0 ? (
          <div className="empty-state">
            <span className="empty-icon">📂</span>
            <p>Aún no hay cargas registradas.</p>
            <Link to="/subida" className="btn btn-primary btn-sm" style={{ marginTop: '0.5rem' }}>
              Subir primer archivo
            </Link>
          </div>
        ) : (
          <div className="table-wrap">
            <table className="table" id="tabla-historial" aria-label="Historial de cargas">
              <thead>
                <tr>
                  <th>#</th>
                  <th>Archivo</th>
                  <th>Usuario</th>
                  <th>Fecha registro</th>
                  <th>Estado</th>
                  <th>Fecha fin</th>
                  <th>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {cargas.map((c, i) => (
                  <tr key={c.id}>
                    <td className="text-muted text-sm">{(page - 1) * PAGE_SIZE + i + 1}</td>
                    <td>
                      <span className="font-semibold" style={{ fontSize: '0.9rem' }}>
                        {c.nombreArchivo}
                      </span>
                    </td>
                    <td className="text-muted text-sm">{c.usuario}</td>
                    <td className="text-sm">{formatFecha(c.fechaRegistro)}</td>
                    <td>
                      <EstadoBadge estado={c.estado} />
                    </td>
                    <td className="text-sm text-muted">{formatFecha(c.fechaFin)}</td>
                    <td>
                      <Link
                        to={`/cargas/${c.id}`}
                        className="btn btn-ghost btn-sm"
                        id={`btn-detalle-${c.id}`}
                      >
                        Ver detalle →
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="pagination" aria-label="Paginación">
          <button
            className="page-btn"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            aria-label="Página anterior"
          >
            ‹
          </button>
          {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
            <button
              key={p}
              className={`page-btn${p === page ? ' active' : ''}`}
              onClick={() => setPage(p)}
              aria-label={`Página ${p}`}
              aria-current={p === page ? 'page' : undefined}
            >
              {p}
            </button>
          ))}
          <button
            className="page-btn"
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            aria-label="Página siguiente"
          >
            ›
          </button>
        </div>
      )}
    </div>
  );
}
