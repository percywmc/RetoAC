import apiClient from './apiClient';

const BASE = '/api/v1/cargas';

/**
 * GET /api/v1/cargas?page=1&pageSize=10
 */
export const listarCargas = async (page = 1, pageSize = 10) => {
  const { data } = await apiClient.get(BASE, { params: { pageNumber: page, pageSize } });
  return data; // { items: [], total, page, pageSize }
};

/**
 * GET /api/v1/cargas/:id
 */
export const obtenerCarga = async (id) => {
  const { data } = await apiClient.get(`${BASE}/${id}`);
  return data;
};

/**
 * GET /api/v1/cargas/:id/contenido
 */
export const obtenerContenido = async (id) => {
  const { data } = await apiClient.get(`${BASE}/${id}/contenido`);
  return data; // { columnas: [], filas: [[]] }
};

/**
 * POST /api/v1/cargas  — multipart/form-data
 * @param {File} archivo
 * @param {(progress: number) => void} onProgress
 */
export const subirCarga = async (archivo, onProgress) => {
  const form = new FormData();
  form.append('archivo', archivo);

  const { data } = await apiClient.post(BASE, form, {
    headers: { 'Content-Type': 'multipart/form-data' },
    onUploadProgress: (evt) => {
      if (onProgress && evt.total) {
        onProgress(Math.round((evt.loaded * 100) / evt.total));
      }
    },
  });
  return data; // { idCarga }
};
