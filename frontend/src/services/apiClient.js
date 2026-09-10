import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

const apiClient = axios.create({
  baseURL: BASE_URL,
  timeout: 30_000,
});

// ── Request interceptor: adjunta JWT en cada request ──────────────────────
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('ac_access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// ── Response interceptor: maneja 401 con refresh automático ───────────────
let isRefreshing = false;
let pendingQueue = [];

const processQueue = (error, token = null) => {
  pendingQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve(token);
  });
  pendingQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // Si la respuesta es 401 y aún no intentamos el refresh
    if (error.response?.status === 401 && !original._retry) {
      const refreshToken = localStorage.getItem('ac_refresh_token');

      // Si no hay refresh token, forzamos logout directo
      if (!refreshToken) {
        clearSession();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        // Encolar peticiones que llegaron mientras se refresca
        return new Promise((resolve, reject) => {
          pendingQueue.push({ resolve, reject });
        }).then((token) => {
          original.headers.Authorization = `Bearer ${token}`;
          return apiClient(original);
        });
      }

      original._retry = true;
      isRefreshing = true;

      try {
        const { data } = await axios.post(`${BASE_URL}/api/v1/auth/refresh`, {
          refreshToken,
        });

        const newToken = data.accessToken;
        const newRefresh = data.refreshToken;

        localStorage.setItem('ac_access_token', newToken);
        localStorage.setItem('ac_refresh_token', newRefresh);

        apiClient.defaults.headers.Authorization = `Bearer ${newToken}`;
        original.headers.Authorization = `Bearer ${newToken}`;

        processQueue(null, newToken);
        return apiClient(original);
      } catch (refreshError) {
        processQueue(refreshError, null);
        clearSession();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export const clearSession = () => {
  localStorage.removeItem('ac_access_token');
  localStorage.removeItem('ac_refresh_token');
  localStorage.removeItem('ac_user');
};

export default apiClient;
