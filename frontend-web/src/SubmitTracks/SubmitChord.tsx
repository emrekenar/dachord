import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiFetch } from '../api';
import ChordLine, { type LineData } from './ChordLine';

const SECTION_TYPES = ['Intro', 'Verse', 'Pre-Chorus', 'Chorus', 'Bridge', 'Outro', 'Interlude', 'Solo'];

interface TrackResult {
  trackId: string;
  title: string;
  artistName: string;
  albumName: string;
}

interface SectionState {
  id: string;
  type: string | null; // null = unlabeled visual break
  lines: LineData[];
}

// API response shape from GET /tracks/{id}/lyrics
interface ApiSection {
  type: string;
  lines: { lyrics: string; chords: Record<string, string>; timeMs?: number }[];
}

type LyricsStatus = 'idle' | 'loading' | 'done' | 'not_found' | 'error';

function newLine(): LineData {
  return { id: crypto.randomUUID(), lyrics: '', chords: [] };
}

function getContributorFromToken(): { id: string; email: string } | null {
  const token = localStorage.getItem('token');
  if (!token) return null;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const id =
      payload.nameid ??
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const email =
      payload.email ??
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    return id ? { id, email: email ?? '' } : null;
  } catch {
    return null;
  }
}

export default function SubmitChord() {
  const { trackId } = useParams<{ trackId?: string }>();
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [trackResults, setTrackResults] = useState<TrackResult[]>([]);
  const [searching, setSearching] = useState(false);
  const [selectedTrack, setSelectedTrack] = useState<TrackResult | null>(null);
  const [sections, setSections] = useState<SectionState[]>([]);
  const [message, setMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [lyricsStatus, setLyricsStatus] = useState<LyricsStatus>('idle');

  useEffect(() => {
    if (!trackId) return;
    apiFetch(`/track/${trackId}`)
      .then(res => (res.ok ? res.json() : null))
      .then(data => {
        if (data)
          selectTrack({ trackId: data.id, title: data.title, artistName: data.artistName, albumName: data.albumName });
      });
  }, [trackId]);

  async function searchTracks(e: { preventDefault(): void }) {
    e.preventDefault();
    setSearching(true);
    try {
      const res = await apiFetch('/searchTracks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Query: query }),
      });
      if (res.ok) {
        const data = await res.json();
        setTrackResults(data.results ?? []);
      }
    } finally {
      setSearching(false);
    }
  }

  function selectTrack(track: TrackResult) {
    setSelectedTrack(track);
    setTrackResults([]);
    setSections([{ id: crypto.randomUUID(), type: null, lines: [newLine()] }]);
    setLyricsStatus('idle');
    setMessage('');
  }

  async function importLyrics() {
    if (!selectedTrack) return;
    const token = localStorage.getItem('token');
    if (!token) { setMessage('Please log in to import lyrics.'); return; }

    setLyricsStatus('loading');
    setMessage('');
    try {
      const res = await apiFetch(`/tracks/${selectedTrack.trackId}/lyrics`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        const data = await res.json();
        const imported: SectionState[] = (data.sections as ApiSection[]).map(s => ({
          id: crypto.randomUUID(),
          type: s.type || null,
          lines: s.lines.map(l => ({ id: crypto.randomUUID(), lyrics: l.lyrics, chords: [], timeMs: l.timeMs })),
        }));
        setSections(imported.length > 0 ? imported : [{ id: crypto.randomUUID(), type: null, lines: [newLine()] }]);
        setLyricsStatus('done');
      } else if (res.status === 404) {
        setLyricsStatus('not_found');
      } else {
        setLyricsStatus('error');
      }
    } catch {
      setLyricsStatus('error');
    }
  }

  function addSection() {
    setSections(prev => [...prev, { id: crypto.randomUUID(), type: null, lines: [newLine()] }]);
  }

  function updateSectionType(sIdx: number, type: string | null) {
    setSections(prev => prev.map((s, i) => i === sIdx ? { ...s, type } : s));
  }

  function deleteSection(sIdx: number) {
    setSections(prev => prev.filter((_, i) => i !== sIdx));
  }

  function addLine(sIdx: number) {
    setSections(prev => prev.map((s, i) =>
      i === sIdx ? { ...s, lines: [...s.lines, newLine()] } : s
    ));
  }

  function deleteLine(sIdx: number, lIdx: number) {
    setSections(prev => prev.map((s, i) =>
      i === sIdx ? { ...s, lines: s.lines.filter((_, li) => li !== lIdx) } : s
    ));
  }

  function updateLine(sIdx: number, lIdx: number, updated: LineData) {
    setSections(prev => prev.map((s, si) =>
      si === sIdx
        ? { ...s, lines: s.lines.map((l, li) => li === lIdx ? updated : l) }
        : s
    ));
  }

  async function handleSubmit() {
    const contributor = getContributorFromToken();
    if (!contributor) { setMessage('Please log in to submit.'); return; }
    if (!selectedTrack) { setMessage('Please select a track.'); return; }
    if (sections.length === 0) { setMessage('Add at least one section.'); return; }

    setSubmitting(true);
    setMessage('');
    try {
      const res = await apiFetch('/tracks', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
        body: JSON.stringify({
          TrackId: selectedTrack.trackId,
          ContributorId: contributor.id,
          ContributorEmail: contributor.email,
          Content: sections.map(s => ({
            Type: s.type ?? '',
            Lines: s.lines.map(l => ({
              Lyrics: l.lyrics,
              Chords: Object.fromEntries(l.chords.map(c => [String(c.position), c.chord])),
            })),
          })),
        }),
      });
      if (res.ok) { navigate(-1); return; }
      else if (res.status === 401) setMessage('Please log in to submit.');
      else if (res.status === 400) setMessage('Invalid request — check all fields.');
      else setMessage('Failed to submit.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="submit-chord-page">
      <h2>Submit Chord Sheet</h2>

      {!selectedTrack ? (
        <div className="track-search-section">
          <p className="section-hint">Find the track you want to add chords for.</p>
          <form className="search-form" onSubmit={searchTracks}>
            <input
              type="text"
              placeholder="Search by song or artist…"
              value={query}
              onChange={e => setQuery(e.target.value)}
            />
            <button type="submit" disabled={searching || query.length < 2}>
              {searching ? 'Searching…' : 'Search'}
            </button>
          </form>
          {trackResults.length > 0 && (
            <div className="track-pick-list">
              {trackResults.map(t => (
                <button key={t.trackId} className="track-pick-item" onClick={() => selectTrack(t)}>
                  <strong>{t.title}</strong>
                  <span>{t.artistName}</span>
                  <span className="album-name">{t.albumName}</span>
                </button>
              ))}
            </div>
          )}
        </div>
      ) : (
        <div className="chord-editor-section">
          {/* Track bar */}
          <div className="selected-track-bar">
            <div>
              <strong>{selectedTrack.title}</strong>
              <span>{selectedTrack.artistName}</span>
            </div>
            <button className="btn-ghost" onClick={() => { setSelectedTrack(null); setSections([]); setMessage(''); }}>
              Change track
            </button>
          </div>

          {/* Lyrics import */}
          <div className="lyrics-import-bar">
            <button
              className="btn-import-lyrics"
              onClick={importLyrics}
              disabled={lyricsStatus === 'loading'}
            >
              {lyricsStatus === 'loading' ? 'Fetching lyrics…' : lyricsStatus === 'done' ? 'Re-import lyrics' : 'Import lyrics'}
            </button>
            {lyricsStatus === 'loading' && (
              <span className="import-status import-status--info">Hang on, this can take a few seconds…</span>
            )}
            {lyricsStatus === 'done' && (
              <span className="import-status import-status--ok">Lyrics imported — add chords by clicking above any line.</span>
            )}
            {lyricsStatus === 'not_found' && (
              <span className="import-status import-status--warn">Lyrics not found — add them manually.</span>
            )}
            {lyricsStatus === 'error' && (
              <span className="import-status import-status--err">Couldn't fetch lyrics. Try again or add manually.</span>
            )}
          </div>

          {/* Editor */}
          <div className="sections-container">
            {sections.map((section, sIdx) => (
              <div key={section.id} className="section-group">
                <div className="section-label-row">
                  {section.type !== null ? (
                    <select
                      className="section-type-select"
                      value={section.type}
                      onChange={e => updateSectionType(sIdx, e.target.value || null)}
                    >
                      <option value="">— remove label —</option>
                      {SECTION_TYPES.map(t => <option key={t}>{t}</option>)}
                    </select>
                  ) : (
                    <button className="btn-add-label" onClick={() => updateSectionType(sIdx, 'Verse')}>
                      + label
                    </button>
                  )}
                  <button className="btn-danger-sm" onClick={() => deleteSection(sIdx)}>Delete part</button>
                </div>

                {section.lines.map((line, lIdx) => (
                  <ChordLine
                    key={line.id}
                    line={line}
                    showDelete={section.lines.length > 1}
                    onChange={updated => updateLine(sIdx, lIdx, updated)}
                    onDelete={() => deleteLine(sIdx, lIdx)}
                  />
                ))}

                <button className="btn-add-line" onClick={() => addLine(sIdx)}>+ Add line</button>

                {sIdx < sections.length - 1 && <hr className="section-break" />}
              </div>
            ))}
          </div>

          <div className="editor-actions">
            <button className="btn-add-part" onClick={addSection}>+ Add part</button>
            {sections.length > 0 && (
              <button className="btn-submit" onClick={handleSubmit} disabled={submitting}>
                {submitting ? 'Submitting…' : 'Submit chord sheet'}
              </button>
            )}
          </div>

          {message && <div className="submit-message">{message}</div>}
        </div>
      )}
    </div>
  );
}
