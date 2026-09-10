import apiClient from './apiClient';

/**
 * POST /api/v1/auth/login
 * @param {string} email
 * @param {string} password
 * @returns {{ accessToken, refreshToken, user }}
 */
export const login = async (username, password) => {
  const { data } = await apiClient.post('/api/v1/auth/login', { username, password });
  return data;
};

/**
 * POST /api/v1/auth/refresh
 * @param {string} refreshToken
 * @returns {{ accessToken, refreshToken }}
 */
export const refreshToken = async (token) => {
  const { data } = await apiClient.post('/api/v1/auth/refresh', { refreshToken: token });
  return data;
};
