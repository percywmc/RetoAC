import { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { subirCarga } from '../services/cargasService';
import { ErrorMessage, SuccessMessage, LoadingSpinner } from '../components/Feedback';

const MAX_MB = Number(import.meta.env.VITE_MAX_FILE_SIZE_MB ?? 10);
const MAX_BYTES = MAX_MB * 1024 * 1024;

const formatBytes = (bytes) => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
};

export default function SubidaPage() {
  const navigate = useNavigate();
  const inputRef = useRef(null);

  const [archivo, setArchivo] = useState(null);
  const [dragging, setDragging] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [progress, setProgress] = useState(0);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  // ── Validación del archivo ─────────────────────────────────────────────
  const validateFile = (file) => {
    if (!file) return 'Selecciona un archivo.';
    if (!file.name.toLowerCase().endsWith('.xlsx'))
      return 'Solo se aceptan archivos con extensión .xlsx';
    if (file.size > MAX_BYTES)
      return `El archivo supera el tamaño máximo permitido de ${MAX_MB} MB.`;
    return null;
  };

  const handleFileChange = (file) => {
    setError(null);
    setSuccess(null);
    setProgress(0);
    const validationError = validateFile(file);
    if (validationError) {
      setError(validationError);
      setArchivo(null);
    } else {
      setArchivo(file);
    }
  };

  // ── Drag & drop handlers ───────────────────────────────────────────────
  const onDragOver  = (e) => { e.preventDefault(); setDragging(true); };
  const onDragLeave = ()  => setDragging(false);
  const onDrop      = (e) => {
    e.preventDefault();
    setDragging(false);
    const file = e.dataTransfer.files?.[0];
    if (file) handleFileChange(file);
  };

  // ── Submit ─────────────────────────────────────────────────────────────
  const handleSubmit = async (e) => {
    e.preventDefault();
    const validationError = validateFile(archivo);
    if (validationError) { setError(validationError); return; }

    setUploading(true);
    setError(null);
    setSuccess(null);

    try {
      const result = await subirCarga(archivo, setProgress);
      setSuccess(`✅ Archivo subido correctamente. Id de carga: ${result.idCarga ?? result.id ?? 'asignado'}.`);
      setArchivo(null);
      if (inputRef.current) inputRef.current.value = '';

      // Redirigir al historial tras 2 segundos
      setTimeout(() => navigate('/cargas'), 2000);
    } catch (err) {
      setError(
        err.response?.data?.detail ??
        err.response?.data?.errors?.archivo?.[0] ??
        'Ocurrió un error al subir el archivo. Intenta nuevamente.'
      );
    } finally {
      setUploading(false);
    }
  };

  return (
    <div style={{ maxWidth: 640, margin: '0 auto' }}>
      <div className="page-header">
        <h1>Subir archivo Excel</h1>
        <p>Sube un archivo .xlsx con los datos a procesar de forma masiva.</p>
      </div>

      <div className="card">
        <ErrorMessage message={error} onDismiss={() => setError(null)} />
        <SuccessMessage message={success} />

        <form onSubmit={handleSubmit} id="form-subida-excel">
          {/* Drop zone */}
          <label
            htmlFor="input-archivo"
            className={`drop-zone${dragging ? ' active' : ''}`}
            onDragOver={onDragOver}
            onDragLeave={onDragLeave}
            onDrop={onDrop}
            aria-describedby="drop-zone-help"
          >
            <div className="drop-icon" aria-hidden="true">
              {archivo ? '📗' : '📁'}
            </div>
            {archivo ? (
              <h3>Archivo seleccionado</h3>
            ) : (
              <>
                <h3>Arrastra tu archivo aquí</h3>
                <p id="drop-zone-help">
                  o <strong>haz clic</strong> para buscarlo en tu equipo
                </p>
                <p style={{ marginTop: '0.5rem', fontSize: '0.8rem', color: 'var(--color-text-faint)' }}>
                  Solo archivos .xlsx · Máximo {MAX_MB} MB
                </p>
              </>
            )}
            <input
              ref={inputRef}
              id="input-archivo"
              type="file"
              accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
              onChange={(e) => handleFileChange(e.target.files?.[0])}
            />
          </label>

          {/* File info card */}
          {archivo && (
            <div className="file-selected">
              <span className="file-icon" aria-hidden="true">📗</span>
              <div className="file-info">
                <div className="file-name">{archivo.name}</div>
                <div className="file-size">{formatBytes(archivo.size)}</div>
              </div>
              <button
                type="button"
                className="btn btn-ghost btn-sm"
                onClick={() => { setArchivo(null); setError(null); if (inputRef.current) inputRef.current.value = ''; }}
                aria-label="Quitar archivo"
              >
                ✕
              </button>
            </div>
          )}

          {/* Progress bar */}
          {uploading && (
            <div style={{ marginTop: '1.25rem' }}>
              <div className="flex justify-between text-sm text-muted" style={{ marginBottom: '0.4rem' }}>
                <span>Subiendo archivo...</span>
                <span>{progress}%</span>
              </div>
              <div className="progress-bar-wrap">
                <div className="progress-bar" style={{ width: `${progress}%` }} role="progressbar" aria-valuenow={progress} />
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-2" style={{ marginTop: '1.5rem' }}>
            <button
              id="btn-subir-excel"
              type="submit"
              className="btn btn-primary btn-lg"
              style={{ flex: 1 }}
              disabled={!archivo || uploading}
            >
              {uploading ? (
                <><LoadingSpinner /> Procesando...</>
              ) : (
                '📤 Subir y procesar'
              )}
            </button>
            <button
              type="button"
              className="btn btn-ghost btn-lg"
              onClick={() => navigate('/cargas')}
              disabled={uploading}
            >
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
