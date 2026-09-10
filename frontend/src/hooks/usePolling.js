import { useEffect, useRef } from 'react';

const TERMINAL_STATES = ['Finalizado', 'Notificado'];

/**
 * Ejecuta `callback` cada `intervalMs` mientras `active` sea true.
 * Se detiene automáticamente cuando el estado llega a un estado terminal.
 *
 * @param {() => void} callback   función a ejecutar en cada tick
 * @param {number}     intervalMs intervalo en milisegundos
 * @param {boolean}    active     si false, el intervalo no corre
 */
export function usePolling(callback, intervalMs, active) {
  const savedCallback = useRef(callback);

  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  useEffect(() => {
    if (!active) return;

    const id = setInterval(() => savedCallback.current(), intervalMs);
    return () => clearInterval(id);
  }, [intervalMs, active]);
}

export { TERMINAL_STATES };
