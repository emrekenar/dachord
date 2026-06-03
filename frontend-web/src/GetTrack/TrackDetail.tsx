import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { apiFetch } from '../api';

interface Track {
  id: string;
  title: string;
  artistName: string;
  albumName: string;
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

  if (loading) return <div className="track-detail"><p>Loading…</p></div>;

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
              <p className="album-name">{track.albumName}</p>
              {track.url && (
                <a href={track.url} target="_blank" rel="noopener noreferrer" className="spotify-link">
                  Open on Spotify
                </a>
              )}
            </div>
          </div>

          {versions.length > 0 ? (
            <div className="chord-versions-list">
              <h3>Chord Sheets</h3>
              {versions.map((v, i) => (
                <Link key={i} to={`/chords/${v.trackId}`} className="chord-version-item">
                  <span className="contributor-name">{v.contributorName ?? 'Anonymous'}</span>
                  {v.isApproved && <span className="approved-badge">✓</span>}
                  <span className="like-count">♥ {v.likeCount}</span>
                </Link>
              ))}
            </div>
          ) : (
            <p className="empty-state" style={{ marginTop: '1.5rem' }}>
              No chord sheets yet for this track.{' '}
              <Link to={`/submit/${id}`}>Be the first to add one!</Link>
            </p>
          )}
        </>
      ) : (
        <p className="empty-state">Track not found.</p>
      )}
    </div>
  );
}
