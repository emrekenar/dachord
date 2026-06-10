export type Role = 'User' | 'Moderator' | 'Admin';

interface TokenPayload {
  role?: string;
  exp?: number;
  displayName?: string;
  nameid?: string;
  [key: string]: unknown;
}

function decodeToken(): TokenPayload | null {
  const token = localStorage.getItem('token');
  if (!token) return null;
  try {
    return JSON.parse(atob(token.split('.')[1]));
  } catch {
    return null;
  }
}

export function getRole(): Role | null {
  const role = decodeToken()?.role;
  return role === 'Moderator' || role === 'Admin' ? role : role === 'User' ? 'User' : null;
}

export function canModerate(): boolean {
  const role = getRole();
  return role === 'Moderator' || role === 'Admin';
}

export function isAdmin(): boolean {
  return getRole() === 'Admin';
}

export function getUserId(): string | null {
  const payload = decodeToken();
  if (!payload) return null;
  const id =
    payload.nameid ??
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
  return typeof id === 'string' ? id : null;
}
