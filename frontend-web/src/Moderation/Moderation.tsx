import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';
import { canModerate } from '../auth';

interface QueueItem {
  trackId: string;
  title: string;
  artistName: string;
  imageUrl?: string;
  contributorId: string;
  contributorName?: string;
  likeCount: number;
}

const MIN_LIKES = 2;

export default function Moderation() {
  const { t } = useTranslation();
  const [items, setItems] = useState<QueueItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string | null>(null);
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) { setLoading(false); return; }
    apiFetch('/moderation/queue', { headers: { Authorization: `Bearer ${token}` } })
      .then(res => (res.ok ? res.json() : []))
      .then((data: QueueItem[]) => setItems(data ?? []))
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  }, []);

  async function approve(item: QueueItem) {
    const token = localStorage.getItem('token');
    if (!token) return;
    const key = `${item.trackId}:${item.contributorId}`;
    setBusy(key);
    setMessage('');
    try {
      const res = await apiFetch(`/chords/${item.trackId}/approve/${item.contributorId}`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        setItems(prev => prev.filter(i => `${i.trackId}:${i.contributorId}` !== key));
        setMessage(t('moderation.approved'));
      } else {
        setMessage(t('moderation.failed'));
      }
    } catch {
      setMessage(t('moderation.failed'));
    } finally {
      setBusy(null);
    }
  }

  if (!canModerate()) return <div className="moderation-page"><p>{t('moderation.empty')}</p></div>;
  if (loading) return <div className="moderation-page"><p>{t('common.loading')}</p></div>;

  return (
    <div className="moderation-page">
      <h2>{t('moderation.title')}</h2>
      <p className="section-hint">{t('moderation.subtitle', { minLikes: MIN_LIKES })}</p>

      {items.length === 0 ? (
        <p className="empty-state">{t('moderation.empty')}</p>
      ) : (
        <div className="moderation-list">
          {items.map(item => {
            const key = `${item.trackId}:${item.contributorId}`;
            return (
              <div key={key} className="moderation-item">
                {item.imageUrl && (
                  <img src={item.imageUrl} alt={item.title} className="moderation-thumb" />
                )}
                <div className="moderation-info">
                  <strong>{item.title}</strong>
                  <span className="artist-name">{item.artistName}</span>
                  <span className="moderation-meta">
                    {t('moderation.by', { name: item.contributorName ?? t('chordView.anonymous') })}
                    {' · '}
                    <span className="like-count">♥ {t('moderation.likes', { count: item.likeCount })}</span>
                  </span>
                </div>
                <div className="moderation-actions">
                  <Link to={`/chords/${item.trackId}`} className="btn-ghost">{t('moderation.view')}</Link>
                  <button
                    className="btn-approve"
                    onClick={() => approve(item)}
                    disabled={busy === key}
                  >
                    {busy === key ? t('moderation.approving') : t('moderation.approve')}
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
