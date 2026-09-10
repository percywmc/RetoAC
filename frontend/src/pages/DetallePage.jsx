import { useState, useCallback, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { obtenerCarga, obtenerContenido } from '../services/cargasService';
import { usePolling, TERMINAL_STATES } from '../hooks/usePolling';
import EstadoBadge from '../components/EstadoBadge';
import { LoadingSpinner, ErrorMessage } from '../components/Feedback';

const POLLING_MS = Number(import.meta.env.VITE_POLLING_INTERVAL_MS ?? 6000);

const formatFecha = (iso) =>
  iso ? new Date(iso).toLocaleString('es-CO', { dateStyle: 'medium', timeStyle: 'short' }) : '—';

export default function DetallePage() {
  const { id } = useParams();

  const [carga, setCarga]     = useState(null);
  const [contenido, setContenido] = useState(null);
  const [loading, setLoading] = useState(true);
  const [loadingContenido, setLoadingContenido] = useState(false);
  const [error, setError]     = useState(null);
  const [errorContenido, setErrorContenido] = useState(null);

  const isTerminal = carga ? TERMINAL_STATES.includes(carga.estado) : false;

  // ── Fetch carga ──────────────────────────────────────────────────────
  const fetchCarga = useCallback(async () => {
    try {
      const data = await obtenerCarga(id);
      setCarga(data);
      setError(null);
    } catch (err) {
      setError(err.response?.data?.detail ?? 'No se pudo obtener el detalle de la carga.');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    setLoading(true);
    fetchCarga();
  }, [fetchCarga]);

  // Polling hasta que llegue a estado terminal
  usePolling(fetchCarga, POLLING_MS, !isTerminal && !!carga);

  // ── Fetch contenido (solo cuando ya terminó) ─────────────────────────
  const fetchContenido = useCallback(async () => {
    if (!isTerminal || contenido) return;
    setLoadingContenido(true);
    setErrorContenido(null);
    try {
      const data = await obtenerContenido(id);
      setContenido(data);
    } catch (err) {
      setErrorContenido(err.response?.data?.detail ?? 'No se pudo cargar el contenido del archivo.');
    } finally {
      setLoadingContenido(false);
    }
  }, [id, isTerminal, contenido]);

  useEffect(() => {
    fetchContenido();
  }, [fetchContenido]);

  // ── Render ───────────────────────────────────────────────────────────
  if (loading) return <LoadingSpinner size="lg" message="Cargando detalle..." />;

  return (
    <div>
      {/* Header */}
      <div className="page-header flex items-center gap-2" style={{ gap: '1rem' }}>
        <Link to="/cargas" className="btn btn-ghost btn-sm">← Volver</Link>
        <div>
          <h1>Detalle de Carga</h1>
          <p className="text-sm text-muted" style={{ marginTop: 0 }}>ID: {id}</p>
        </div>
      </div>

      <ErrorMessage message={error} onDismiss={() => setError(null)} />

      {carga && (
        <>
          {/* ── Ficha de la carga ── */}
          <div className="card" style={{ marginBottom: '1.5rem' }}>
            <div className="flex justify-between items-center" style={{ marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.1rem', fontWeight: 700 }}>
                📄 {carga.nombreArchivo}
              </h2>
              <EstadoBadge estado={carga.estado} />
            </div>

            <div className="detail-grid">
              <div className="detail-field">
                <label>Usuario</label>
                <span>{carga.usuario}</span>
              </div>
              <div className="detail-field">
                <label>Fecha de registro</label>
                <span>{formatFecha(carga.fechaRegistro)}</span>
              </div>
              <div className="detail-field">
                <label>Estado actual</label>
                <span><EstadoBadge estado={carga.estado} /></span>
              </div>
              <div className="detail-field">
                <label>Fecha finalización</label>
                <span>{formatFecha(carga.fechaFin)}</span>
              </div>
            </div>

            {/* Polling indicator si no ha terminado */}
            {!isTerminal && (
              <div className="polling-indicator" style={{ marginTop: '1.25rem' }}>
                <span className="polling-dot" />
                Actualizando estado cada {POLLING_MS / 1000}s...
              </div>
            )}
          </div>

          {/* ── Contenido del Excel ── */}
          <div className="card">
            <h2 style={{ fontSize: '1.1rem', fontWeight: 700, marginBottom: '1rem' }}>
              📊 Contenido procesado
            </h2>

            {!isTerminal && (
              <div className="alert alert-info">
                <span>ℹ️</span>
                <span>El contenido estará disponible una vez que la carga finalice.</span>
              </div>
            )}

            {isTerminal && loadingContenido && (
              <LoadingSpinner message="Cargando datos del archivo..." />
            )}

            {isTerminal && errorContenido && (
              <ErrorMessage message={errorContenido} onDismiss={() => setErrorContenido(null)} />
            )}

            {isTerminal && contenido && (
              <>
                <p className="text-sm text-muted" style={{ marginBottom: '1rem' }}>
                  {contenido.totalFilas ?? contenido.filas?.length ?? 0} registros procesados
                </p>
                <div className="table-wrap">
                  <table className="table" id="tabla-contenido-excel" aria-label="Contenido del archivo Excel">
                    <thead>
                      <tr>
                        {(contenido.columnas ?? []).map((col) => (
                          <th key={col}>{col}</th>
                        ))}
                      </tr>
                    </thead>
                    <tbody>
                      {(contenido.filas ?? []).slice(0, 200).map((fila, i) => (
                        <tr key={i}>
                          {(contenido.columnas ?? []).map((col) => (
                            <td key={col} className="text-sm">{fila[col] ?? '—'}</td>
                          ))}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                {(contenido.filas?.length ?? 0) > 200 && (
                  <p className="text-sm text-muted" style={{ marginTop: '0.75rem', textAlign: 'center' }}>
                    Mostrando primeros 200 registros de {contenido.filas.length}.
                  </p>
                )}
              </>
            )}

            {isTerminal && !loadingContenido && !contenido && !errorContenido && (
              <div className="empty-state">
                <span className="empty-icon">📂</span>
                <p>No se encontraron datos procesados para esta carga.</p>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}
