export function LoadingSpinner({ size = 'md', message }) {
  const cls = size === 'lg' ? 'spinner spinner-lg' : 'spinner';
  if (message) {
    return (
      <div className="loading-center">
        <div className={cls} role="status" aria-label="Cargando" />
        <span>{message}</span>
      </div>
    );
  }
  return <div className={cls} role="status" aria-label="Cargando" />;
}

export function ErrorMessage({ message, onDismiss }) {
  if (!message) return null;
  return (
    <div className="alert alert-error" role="alert">
      <span>⚠️</span>
      <span style={{ flex: 1 }}>{message}</span>
      {onDismiss && (
        <button
          onClick={onDismiss}
          style={{ background: 'none', border: 'none', color: 'inherit', cursor: 'pointer', fontSize: '1rem' }}
          aria-label="Cerrar"
        >
          ✕
        </button>
      )}
    </div>
  );
}

export function SuccessMessage({ message }) {
  if (!message) return null;
  return (
    <div className="alert alert-success" role="status">
      <span>✅</span>
      <span>{message}</span>
    </div>
  );
}
