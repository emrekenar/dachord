const BASE = import.meta.env.VITE_API_BASE ?? 'https://localhost:7266';
const DEV_KEY = import.meta.env.VITE_DEV_KEY ?? '';

export function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers as HeadersInit | undefined);
  if (DEV_KEY) headers.set('X-Dev-Key', DEV_KEY);
  return fetch(`${BASE}${path}`, { ...init, headers });
}
