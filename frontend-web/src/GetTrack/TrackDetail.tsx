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

export default function TrackDetail() {
  const { id } = useParams();
  const [track, setTrack] = useState<Track | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    apiFetch(`/track/${id}`)
      .then(res => (res.ok ? res.json() : null))
      .then(data => { setTrack(data); setLoading(false); })
      .catch(() => setLoading(false));
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
          <p className="empty-state" style={{ marginTop: '1.5rem' }}>
            No chord sheets yet for this track.{' '}
            <Link to={`/submit/${id}`}>Be the first to add one!</Link>
          </p>
        </>
      ) : (
        <p className="empty-state">Track not found.</p>
      )}
    </div>
  );
}
