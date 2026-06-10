import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';
import { isAdmin, getUserId, type Role } from '../auth';

interface PublicUser {
  displayName: string;
  bio: string;
  avatarIcon: string;
  role: Role;
  numberOfApprovedSongs: number;
  numberOfLikes: number;
}

export default function PublicProfile() {
  const { t } = useTranslation();
  const { id } = useParams();
  const [profile, setProfile] = useState<PublicUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState('');

  useEffect(() => {
    setLoading(true);
    apiFetch(`/user/${id}`)
      .then(res => (res.ok ? res.json() : null))
      .then((data: PublicUser | null) => setProfile(data))
      .catch(() => setProfile(null))
      .finally(() => setLoading(false));
  }, [id]);

  async function setRole(newRole: Role) {
    const token = localStorage.getItem('token');
    if (!token || !id) return;
    setBusy(true);
    setMessage('');
    try {
      const res = await apiFetch(`/admin/users/${id}/role`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ Role: newRole }),
      });
      if (res.ok) {
        setProfile(p => (p ? { ...p, role: newRole } : p));
        setMessage(t('admin.updated'));
      } else {
        setMessage(t('admin.failed'));
      }
    } catch {
      setMessage(t('admin.failed'));
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <div className="profile-page"><p>{t('common.loading')}</p></div>;
  if (!profile) return <div className="profile-page"><p className="empty-state">{t('publicProfile.notFound')}</p></div>;

  const role = profile.role;
  const canManageRole = isAdmin() && id !== getUserId();

  return (
    <div className="profile-page">
      <div className="profile-header">
        <div className="profile-avatar">
          {profile.avatarIcon ? (
            <span className="profile-avatar-icon">{profile.avatarIcon}</span>
          ) : (
            <div className="profile-avatar-placeholder">{profile.displayName?.[0]?.toUpperCase() ?? '?'}</div>
          )}
        </div>
        <div className="profile-header-info">
          <h2>{profile.displayName || t('publicProfile.anonymous')}</h2>
          {(role === 'Moderator' || role === 'Admin') && (
            <span className={`profile-role-badge role-${role.toLowerCase()}`}>{t(`roles.${role}`)}</span>
          )}
        </div>
      </div>

      {profile.bio && <p className="public-profile-bio">{profile.bio}</p>}

      <div className="profile-stats">
        <div className="profile-stat">
          <span className="profile-stat-value">{profile.numberOfApprovedSongs}</span>
          <span className="profile-stat-label">{t('profile.approvedSheets')}</span>
        </div>
        <div className="profile-stat">
          <span className="profile-stat-value">{profile.numberOfLikes}</span>
          <span className="profile-stat-label">{t('profile.likesReceived')}</span>
        </div>
      </div>

      {canManageRole && (
        <div className="profile-admin-controls">
          <p className="section-hint">{t('admin.subtitle')}</p>
          <div className="admin-actions">
            {role !== 'Moderator' && (
              <button className="btn-ghost" disabled={busy} onClick={() => setRole('Moderator')}>
                {t('admin.makeModerator')}
              </button>
            )}
            {role !== 'Admin' && (
              <button className="btn-ghost" disabled={busy} onClick={() => setRole('Admin')}>
                {t('admin.makeAdmin')}
              </button>
            )}
            {role !== 'User' && (
              <button className="btn-ghost" disabled={busy} onClick={() => setRole('User')}>
                {t('admin.makeUser')}
              </button>
            )}
          </div>
          {message && <div className="submit-message">{message}</div>}
        </div>
      )}
    </div>
  );
}
