import { useState, useEffect } from 'react';
import { apiFetch } from '../api';

export default function Profile() {
  const [displayName, setDisplayName] = useState('');
  const [originalName, setOriginalName] = useState('');
  const [message, setMessage] = useState('');
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) { setLoading(false); return; }
    apiFetch('/user/me', { headers: { Authorization: `Bearer ${token}` } })
      .then(res => res.ok ? res.json() : null)
      .then(data => {
        if (data) {
          setDisplayName(data.displayName ?? '');
          setOriginalName(data.displayName ?? '');
        }
      })
      .finally(() => setLoading(false));
  }, []);

  async function handleSave(e: React.FormEvent) {
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
      if (res.ok) { setMessage('Display name saved.'); setOriginalName(displayName); }
      else setMessage('Failed to save.');
    } catch {
      setMessage('Failed to save.');
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="profile-page"><p>Loading…</p></div>;

  return (
    <div className="profile-page">
      <h2>Profile</h2>
      <form className="profile-form" onSubmit={handleSave}>
        <label htmlFor="displayName">Display name</label>
        <input
          id="displayName"
          type="text"
          placeholder="Your display name"
          value={displayName}
          onChange={e => { setDisplayName(e.target.value); setMessage(''); }}
        />
        <button type="submit" disabled={saving || displayName === originalName}>{saving ? 'Saving…' : 'Save'}</button>
      </form>
      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
