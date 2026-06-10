import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';
import { isAdmin, type Role } from '../auth';

interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  avatarIcon: string;
  role: Role;
}

export default function Admin() {
  const { t } = useTranslation();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string | null>(null);
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) { setLoading(false); return; }
    apiFetch('/admin/users', { headers: { Authorization: `Bearer ${token}` } })
      .then(res => (res.ok ? res.json() : []))
      .then((data: AdminUser[]) => setUsers(data ?? []))
      .catch(() => setUsers([]))
      .finally(() => setLoading(false));
  }, []);

  async function setRole(user: AdminUser, role: Role) {
    const token = localStorage.getItem('token');
    if (!token) return;
    setBusy(user.id);
    setMessage('');
    try {
      const res = await apiFetch(`/admin/users/${user.id}/role`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ Role: role }),
      });
      if (res.ok) {
        setUsers(prev => prev.map(u => (u.id === user.id ? { ...u, role } : u)));
        setMessage(t('admin.updated'));
      } else {
        setMessage(t('admin.failed'));
      }
    } catch {
      setMessage(t('admin.failed'));
    } finally {
      setBusy(null);
    }
  }

  if (!isAdmin()) return <div className="admin-page"><p>{t('admin.empty')}</p></div>;
  if (loading) return <div className="admin-page"><p>{t('common.loading')}</p></div>;

  return (
    <div className="admin-page">
      <h2>{t('admin.title')}</h2>
      <p className="section-hint">{t('admin.subtitle')}</p>

      {users.length === 0 ? (
        <p className="empty-state">{t('admin.empty')}</p>
      ) : (
        <table className="admin-table">
          <thead>
            <tr>
              <th>{t('admin.colUser')}</th>
              <th>{t('admin.colEmail')}</th>
              <th>{t('admin.colRole')}</th>
              <th>{t('admin.colActions')}</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr key={user.id}>
                <td>
                  <span className="admin-user-cell">
                    <span className="admin-avatar">{user.avatarIcon || '👤'}</span>
                    {user.displayName || '—'}
                  </span>
                </td>
                <td>{user.email}</td>
                <td>
                  <span className={`profile-role-badge role-${user.role.toLowerCase()}`}>
                    {t(`roles.${user.role}`)}
                  </span>
                </td>
                <td className="admin-actions">
                  {user.role !== 'Moderator' && (
                    <button className="btn-ghost" disabled={busy === user.id} onClick={() => setRole(user, 'Moderator')}>
                      {t('admin.makeModerator')}
                    </button>
                  )}
                  {user.role !== 'Admin' && (
                    <button className="btn-ghost" disabled={busy === user.id} onClick={() => setRole(user, 'Admin')}>
                      {t('admin.makeAdmin')}
                    </button>
                  )}
                  {user.role !== 'User' && (
                    <button className="btn-ghost" disabled={busy === user.id} onClick={() => setRole(user, 'User')}>
                      {t('admin.makeUser')}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
