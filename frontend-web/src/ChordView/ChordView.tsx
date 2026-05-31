import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { apiFetch } from '../api';

interface Line {
  lyrics: string;
  chords: Record<string, string>;
}

interface Section {
  type: string;
  lines: Line[];
}

interface TrackVersion {
  trackId: string;
  contributorName?: string;
  isApproved: boolean;
  likeCount: number;
  updatedAt: string;
  content: Section[];
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

export default function ChordView() {
  const { id } = useParams();
  const [version, setVersion] = useState<TrackVersion | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    apiFetch(`/chords/${id}`)
      .then(res => {
        if (!res.ok) { setNotFound(true); setLoading(false); return null; }
        return res.json();
      })
      .then(data => {
        if (data) { setVersion(data); setLoading(false); }
      });
  }, [id]);

  if (loading) return <div className="chord-view-page"><p>Loading…</p></div>;
  if (notFound || !version) return (
    <div className="chord-view-page">
      <p>Chord sheet not found.</p>
      <Link to="/">← Back to search</Link>
    </div>
  );

  return (
    <div className="chord-view-page">
      <div className="chord-view-meta">
        {version.contributorName && <span>by {version.contributorName}</span>}
        <span className="like-count">♥ {version.likeCount}</span>
        {version.isApproved && <span className="approved-badge">✓ approved</span>}
      </div>
      <div className="chord-sheet">
        {version.content.map((section, sIdx) => (
          <div key={sIdx} className="chord-section">
            <h3 className="section-label">{section.type}</h3>
            {section.lines.map((line, lIdx) => {
              const chordLine = buildChordLine(line.lyrics, line.chords);
              return (
                <div key={lIdx} className="chord-line-pair">
                  {chordLine && <pre className="chord-names">{chordLine}</pre>}
                  <pre className="lyric-text">{line.lyrics || ' '}</pre>
                </div>
              );
            })}
          </div>
        ))}
      </div>
      <Link to="/" className="back-link">← Back to search</Link>
    </div>
  );
}
