/**
 * EstadoBadge — muestra el estado de una carga con color y dot animado.
 */
const ESTADO_MAP = {
  pendiente:   { label: 'Pendiente',   cls: 'badge-pendiente' },
  'en proceso': { label: 'En proceso',  cls: 'badge-en-proceso' },
  cargado:     { label: 'Cargado',     cls: 'badge-cargado' },
  finalizado:  { label: 'Finalizado',  cls: 'badge-finalizado' },
  notificado:  { label: 'Notificado',  cls: 'badge-notificado' },
};

export default function EstadoBadge({ estado }) {
  const key = String(estado ?? '').toLowerCase();
  const cfg = ESTADO_MAP[key] ?? { label: estado ?? '—', cls: 'badge-pendiente' };

  return <span className={`badge ${cfg.cls}`}>{cfg.label}</span>;
}
