import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiFetch } from '../api';

interface Line {
  lyrics: string;
  chords: Record<string, string>;
  timeMs?: number;
}

interface Section {
  type: string;
  lines: Line[];
}

interface TrackInfo {
  title: string;
  artistName: string;
  albumName: string;
  releaseYear?: string;
  imageUrl?: string;
}

interface TrackVersion {
  trackId: string;
  contributorId: string;
  contributorName?: string;
  isApproved: boolean;
  likeCount: number;
  updatedAt: string;
  content: Section[];
}

function formatTime(ms: number): string {
  const totalSeconds = Math.floor(ms / 1000);
  const m = Math.floor(totalSeconds / 60).toString().padStart(2, '0');
  const s = (totalSeconds % 60).toString().padStart(2, '0');
  return `${m}:${s}`;
}

function buildChordLine(lyrics: string, chords: Record<string, string>): string {
  const entries = Object.entries(chords)
    .map(([pos, chord]) => ({ pos: parseInt(pos), chord }))
    .sort((a, b) => a.pos - b.pos);
  if (entries.length === 0) return '';
  const maxLen = Math.max(
    lyrics.length,
    ...entries.map(e => e.pos + e.chord.length)
  ) + 1;
  const chars = Array<string>(maxLen).fill(' ');
  for (const { pos, chord } of entries) {
    for (let i = 0; i < chord.length && pos + i < chars.length; i++) {
      chars[pos + i] = chord[i];
    }
  }
  return chars.join('').trimEnd();
}

function getRoleFromToken(): string | null {
  const token = localStorage.getItem('token');
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.role ?? null;
  } catch { return null; }
}

export default function ChordView() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [version, setVersion] = useState<TrackVersion | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  const [trackInfo, setTrackInfo] = useState<TrackInfo | null>(null);
  const isModerator = getRoleFromToken() === 'Moderator';

  useEffect(() => {
    apiFetch(`/chords/${id}`)
      .then(res => {
        if (!res.ok) { setNotFound(true); setLoading(false); return null; }
        return res.json();
      })
      .then((data: TrackVersion | null) => {
        if (!data) return;
        setVersion(data);
        setLoading(false);
        apiFetch(`/track/${data.trackId}`)
          .then(r => r.ok ? r.json() : null)
          .then(t => { if (t) setTrackInfo(t); });
      });
  }, [id]);

  async function handleApprove() {
    const token = localStorage.getItem('token');
    const res = await apiFetch(`/chords/${id}/approve`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.ok) {
      setVersion(v => v ? { ...v, isApproved: !v.isApproved } : v);
    }
  }

  if (loading) return <div className="chord-view-page"><p>Loading…</p></div>;
  if (notFound || !version) return (
    <div className="chord-view-page">
      <p>Chord sheet not found.</p>
      <button className="back-link" onClick={() => navigate(-1)}>← Back to search</button>
    </div>
  );

  return (
    <div className="chord-view-page">
      {trackInfo && (
        <div className="chord-view-track-header">
          {trackInfo.imageUrl && (
            <img src={trackInfo.imageUrl} alt={trackInfo.albumName} className="chord-view-album-art" />
          )}
          <div className="chord-view-track-info">
            <h2>{trackInfo.title}</h2>
            <span className="artist-name">{trackInfo.artistName}</span>
            <span className="album-name">{trackInfo.albumName}{trackInfo.releaseYear ? ` (${trackInfo.releaseYear})` : ''}</span>
          </div>
        </div>
      )}
      <div className="chord-view-meta">
        <span>by {version.contributorName ?? 'Anonymous'}</span>
        {version.isApproved && <span className="approved-badge">✓ approved</span>}
        {isModerator && (
          <button
            className={`btn-approve${version.isApproved ? ' unapprove' : ''}`}
            onClick={handleApprove}
          >
            {version.isApproved ? 'Unapprove' : 'Approve'}
          </button>
        )}
      </div>
      <div className="chord-sheet">
        {version.content.map((section, sIdx) => (
          <div key={sIdx} className="chord-section">
            <h3 className="section-label">{section.type}</h3>
            {section.lines.map((line, lIdx) => {
              const chordLine = buildChordLine(line.lyrics, line.chords);
              return (
                <div key={lIdx} className="chord-line-pair">
                  {line.timeMs != null && <span className="line-timestamp">{formatTime(line.timeMs)}</span>}
                  {chordLine && <pre className="chord-names">{chordLine}</pre>}
                  <pre className="lyric-text">{line.lyrics || ' '}</pre>
                </div>
              );
            })}
          </div>
        ))}
      </div>
      <button className="back-link" onClick={() => navigate(-1)}>← Back to search</button>
    </div>
  );
}
