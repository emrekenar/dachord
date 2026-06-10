import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';
import { AVATAR_ICONS } from '../avatars';

interface UserProfile {
  displayName: string;
  bio: string;
  avatarIcon: string;
  role: string;
  numberOfApprovedSongs: number;
  numberOfLikes: number;
}

export default function Profile() {
  const { t } = useTranslation();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [displayName, setDisplayName] = useState('');
  const [originalName, setOriginalName] = useState('');
  const [bio, setBio] = useState('');
  const [originalBio, setOriginalBio] = useState('');
  const [avatarIcon, setAvatarIcon] = useState('');
  const [originalIcon, setOriginalIcon] = useState('');
  const [message, setMessage] = useState('');
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) { setLoading(false); return; }
    apiFetch('/user/me', { headers: { Authorization: `Bearer ${token}` } })
      .then(res => res.ok ? res.json() : null)
      .then((data: UserProfile | null) => {
        if (data) {
          setProfile(data);
          setDisplayName(data.displayName ?? '');
          setOriginalName(data.displayName ?? '');
          setBio(data.bio ?? '');
          setOriginalBio(data.bio ?? '');
          setAvatarIcon(data.avatarIcon ?? '');
          setOriginalIcon(data.avatarIcon ?? '');
        }
      })
      .finally(() => setLoading(false));
  }, []);

  async function handleSaveName(e: React.FormEvent) {
    e.preventDefault();
    const token = localStorage.getItem('token');
    if (!token) return;
    setSaving(true);
    setMessage('');
    try {
      const res = await apiFetch('/user/display-name', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ DisplayName: displayName }),
      });
      if (res.ok) { setMessage(t('profile.nameSaved')); setOriginalName(displayName); }
      else setMessage(t('profile.saveFailed'));
    } catch {
      setMessage(t('profile.saveFailed'));
    } finally {
      setSaving(false);
    }
  }

  async function handleSaveProfile(e: React.FormEvent) {
    e.preventDefault();
    const token = localStorage.getItem('token');
    if (!token) return;
    setSaving(true);
    setMessage('');
    try {
      const res = await apiFetch('/user/profile', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ Bio: bio, AvatarIcon: avatarIcon }),
      });
      if (res.ok) {
        setMessage(t('profile.profileSaved'));
        setOriginalBio(bio);
        setOriginalIcon(avatarIcon);
      } else setMessage(t('profile.saveFailed'));
    } catch {
      setMessage(t('profile.saveFailed'));
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="profile-page"><p>{t('common.loading')}</p></div>;

  const profileChanged = bio !== originalBio || avatarIcon !== originalIcon;
  const role = profile?.role;

  return (
    <div className="profile-page">
      <div className="profile-header">
        <div className="profile-avatar">
          {avatarIcon ? (
            <span className="profile-avatar-icon">{avatarIcon}</span>
          ) : (
            <div className="profile-avatar-placeholder">{displayName?.[0]?.toUpperCase() ?? '?'}</div>
          )}
        </div>
        <div className="profile-header-info">
          <h2>{displayName || t('profile.title')}</h2>
          {(role === 'Moderator' || role === 'Admin') && (
            <span className={`profile-role-badge role-${role.toLowerCase()}`}>{t(`roles.${role}`)}</span>
          )}
        </div>
      </div>

      {profile && (
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
      )}

      <form className="profile-form" onSubmit={handleSaveName}>
        <label htmlFor="displayName">{t('profile.displayName')}</label>
        <input
          id="displayName"
          type="text"
          placeholder={t('profile.displayNamePlaceholder')}
          value={displayName}
          onChange={e => { setDisplayName(e.target.value); setMessage(''); }}
        />
        <button type="submit" disabled={saving || displayName === originalName}>
          {saving ? t('common.saving') : t('profile.saveName')}
        </button>
      </form>

      <form className="profile-form" onSubmit={handleSaveProfile} style={{ marginTop: '1.5rem' }}>
        <label htmlFor="bio">{t('profile.bio')}</label>
        <textarea
          id="bio"
          placeholder={t('profile.bioPlaceholder')}
          value={bio}
          rows={3}
          onChange={e => { setBio(e.target.value); setMessage(''); }}
          className="profile-bio-input"
        />
        <label>{t('profile.avatar')}</label>
        <p className="profile-avatar-hint">{t('profile.avatarHint')}</p>
        <div className="avatar-picker">
          {AVATAR_ICONS.map(icon => (
            <button
              type="button"
              key={icon}
              className={`avatar-option${avatarIcon === icon ? ' selected' : ''}`}
              onClick={() => { setAvatarIcon(avatarIcon === icon ? '' : icon); setMessage(''); }}
              aria-label={icon}
              aria-pressed={avatarIcon === icon}
            >
              {icon}
            </button>
          ))}
        </div>
        <button type="submit" disabled={saving || !profileChanged}>
          {saving ? t('common.saving') : t('profile.saveProfile')}
        </button>
      </form>

      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
