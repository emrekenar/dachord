import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { apiFetch } from '../api';

interface Track {
  id: string;
  title: string;
  artistName: string;
  albumName: string;
  releaseYear?: string;
  imageUrl?: string;
  url?: string;
}

interface ChordVersion {
  trackId: string;
  contributorName?: string;
  isApproved: boolean;
  likeCount: number;
  updatedAt: string;
}

export default function TrackDetail() {
  const { t } = useTranslation();
  const { id } = useParams();
  const [track, setTrack] = useState<Track | null>(null);
  const [versions, setVersions] = useState<ChordVersion[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      apiFetch(`/track/${id}`).then(res => res.ok ? res.json() : null),
      apiFetch(`/tracks/${id}/chords`).then(res => res.ok ? res.json() : []),
    ]).then(([trackData, chordsData]) => {
      setTrack(trackData);
      setVersions(chordsData ?? []);
      setLoading(false);
    }).catch(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="track-detail"><p>{t('common.loading')}</p></div>;

  return (
    <div className="track-detail">
      {track ? (
        <>
          <div className="track-detail-header">
            {track.imageUrl && (
              <img src={track.imageUrl} alt={track.albumName} className="track-album-art" />
            )}
            <div className="track-detail-meta">
              <h2>{track.title}</h2>
              <p className="artist-name">{track.artistName}</p>
              <p className="album-name">{track.albumName}{track.releaseYear ? ` (${track.releaseYear})` : ''}</p>
              {track.url && (
                <a href={track.url} target="_blank" rel="noopener noreferrer" className="spotify-link">
                  {t('track.openOnSpotify')}
                </a>
              )}
            </div>
          </div>

          {versions.length > 0 ? (
            <div className="chord-versions-list">
              <h3>{t('track.chordSheets')}</h3>
              {versions.map((v, i) => (
                <Link key={i} to={`/chords/${v.trackId}`} className="chord-version-item">
                  <span className="contributor-name">{v.contributorName ?? t('track.anonymous')}</span>
                  {v.isApproved && <span className="approved-badge">✓</span>}
                  <span className="like-count">♥ {v.likeCount}</span>
                </Link>
              ))}
            </div>
          ) : (
            <p className="empty-state" style={{ marginTop: '1.5rem' }}>
              {t('track.noSheets')}{' '}
              <Link to={`/submit/${id}`}>{t('track.beFirst')}</Link>
            </p>
          )}
        </>
      ) : (
        <p className="empty-state">{t('track.notFound')}</p>
      )}
    </div>
  );
}
