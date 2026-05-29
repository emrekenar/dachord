import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { apiFetch } from '../api';
const SECTION_TYPES = ['Intro', 'Verse', 'Pre-Chorus', 'Chorus', 'Bridge', 'Outro', 'Interlude', 'Solo'];

interface TrackResult {
  trackId: string;
  title: string;
  artistName: string;
  albumName: string;
}

interface ChordEntry {
  position: number;
  chord: string;
}

interface LineState {
  id: string;
  lyrics: string;
  chords: ChordEntry[];
}

interface SectionState {
  id: string;
  type: string;
  lines: LineState[];
}

function newLine(): LineState {
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

function chordPreviewLine(lyrics: string, chords: ChordEntry[]): string {
  const sorted = [...chords].sort((a, b) => a.position - b.position);
  if (sorted.length === 0) return '';
  const maxLen = Math.max(
    lyrics.length,
    ...sorted.map(c => c.position + c.chord.length)
  ) + 1;
  const chars = Array<string>(maxLen).fill(' ');
  for (const { position, chord } of sorted) {
    for (let i = 0; i < chord.length && position + i < chars.length; i++) {
      chars[position + i] = chord[i];
    }
  }
  return chars.join('').trimEnd();
}

export default function SubmitChord() {
  const { trackId } = useParams<{ trackId?: string }>();
  const [query, setQuery] = useState('');
  const [trackResults, setTrackResults] = useState<TrackResult[]>([]);
  const [searching, setSearching] = useState(false);
  const [selectedTrack, setSelectedTrack] = useState<TrackResult | null>(null);
  const [sections, setSections] = useState<SectionState[]>([]);
  const [message, setMessage] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!trackId) return;
    apiFetch(`/track/${trackId}`)
      .then(res => (res.ok ? res.json() : null))
      .then(data => {
        if (data) selectTrack({ trackId: data.id, title: data.title, artistName: data.artistName, albumName: data.albumName });
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
    setSections([{ id: crypto.randomUUID(), type: 'Verse', lines: [newLine()] }]);
  }

  function addSection() {
    setSections(prev => [...prev, { id: crypto.randomUUID(), type: 'Verse', lines: [newLine()] }]);
  }

  function updateSectionType(sIdx: number, type: string) {
    setSections(prev => prev.map((s, i) => i === sIdx ? { ...s, type } : s));
  }

  function deleteSection(sIdx: number) {
    setSections(prev => prev.filter((_, i) => i !== sIdx));
  }

  function addLine(sIdx: number) {
    setSections(prev => prev.map((s, i) => i === sIdx
      ? { ...s, lines: [...s.lines, newLine()] }
      : s));
  }

  function deleteLine(sIdx: number, lIdx: number) {
    setSections(prev => prev.map((s, i) => i === sIdx
      ? { ...s, lines: s.lines.filter((_, li) => li !== lIdx) }
      : s));
  }

  function updateLyrics(sIdx: number, lIdx: number, lyrics: string) {
    setSections(prev => prev.map((s, si) => si === sIdx
      ? { ...s, lines: s.lines.map((l, li) => li === lIdx ? { ...l, lyrics } : l) }
      : s));
  }

  function addChord(sIdx: number, lIdx: number) {
    setSections(prev => prev.map((s, si) => si === sIdx
      ? { ...s, lines: s.lines.map((l, li) => li === lIdx
          ? { ...l, chords: [...l.chords, { position: 0, chord: '' }] }
          : l) }
      : s));
  }

  function updateChord(sIdx: number, lIdx: number, cIdx: number, field: keyof ChordEntry, value: string | number) {
    setSections(prev => prev.map((s, si) => si === sIdx
      ? { ...s, lines: s.lines.map((l, li) => li === lIdx
          ? { ...l, chords: l.chords.map((c, ci) => ci === cIdx ? { ...c, [field]: value } : c) }
          : l) }
      : s));
  }

  function deleteChord(sIdx: number, lIdx: number, cIdx: number) {
    setSections(prev => prev.map((s, si) => si === sIdx
      ? { ...s, lines: s.lines.map((l, li) => li === lIdx
          ? { ...l, chords: l.chords.filter((_, ci) => ci !== cIdx) }
          : l) }
      : s));
  }

  async function handleSubmit() {
    const contributor = getContributorFromToken();
    if (!contributor) { setMessage('Please log in to submit.'); return; }
    if (!selectedTrack) { setMessage('Please select a track.'); return; }
    if (sections.length === 0) { setMessage('Add at least one section.'); return; }

    setSubmitting(true);
    setMessage('');
    try {
      const res = await apiFetch('/chords', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
        body: JSON.stringify({
          TrackId: selectedTrack.trackId,
          ContributorId: contributor.id,
          ContributorEmail: contributor.email,
          Content: sections.map(s => ({
            Type: s.type,
            Lines: s.lines.map(l => ({
              Lyrics: l.lyrics,
              Chords: Object.fromEntries(l.chords.map(c => [String(c.position), c.chord])),
            })),
          })),
        }),
      });
      if (res.ok) setMessage('Chord sheet submitted!');
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
          <p className="section-hint">First, find the track you want to add chords for.</p>
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
          <div className="selected-track-bar">
            <div>
              <strong>{selectedTrack.title}</strong>
              <span>{selectedTrack.artistName}</span>
            </div>
            <button className="btn-ghost" onClick={() => { setSelectedTrack(null); setSections([]); setMessage(''); }}>
              Change track
            </button>
          </div>

          <div className="sections-container">
            {sections.map((section, sIdx) => (
              <div key={section.id} className="section-card">
                <div className="section-header">
                  <select
                    value={section.type}
                    onChange={e => updateSectionType(sIdx, e.target.value)}
                  >
                    {SECTION_TYPES.map(t => <option key={t}>{t}</option>)}
                  </select>
                  <button className="btn-danger-sm" onClick={() => deleteSection(sIdx)}>Delete part</button>
                </div>

                {section.lines.map((line, lIdx) => {
                  const preview = chordPreviewLine(line.lyrics, line.chords);
                  return (
                    <div key={line.id} className="line-editor">
                      {preview && <pre className="chord-preview-bar">{preview}</pre>}
                      <div className="line-row">
                        <input
                          className="lyrics-input"
                          type="text"
                          placeholder="Lyrics…"
                          value={line.lyrics}
                          onChange={e => updateLyrics(sIdx, lIdx, e.target.value)}
                        />
                        {section.lines.length > 1 && (
                          <button className="btn-remove-line" onClick={() => deleteLine(sIdx, lIdx)} title="Remove line">×</button>
                        )}
                      </div>
                      <div className="chord-entries-row">
                        {line.chords.map((c, cIdx) => (
                          <span key={cIdx} className="chord-entry">
                            <input
                              className="chord-name-input"
                              type="text"
                              placeholder="Am"
                              value={c.chord}
                              onChange={e => updateChord(sIdx, lIdx, cIdx, 'chord', e.target.value)}
                            />
                            <span className="at-label">@</span>
                            <input
                              className="position-input"
                              type="number"
                              min={0}
                              value={c.position}
                              onChange={e => updateChord(sIdx, lIdx, cIdx, 'position', Number(e.target.value))}
                            />
                            <button className="btn-remove-chord" onClick={() => deleteChord(sIdx, lIdx, cIdx)}>×</button>
                          </span>
                        ))}
                        <button className="btn-add-chord" onClick={() => addChord(sIdx, lIdx)}>+ chord</button>
                      </div>
                    </div>
                  );
                })}

                <button className="btn-add-line" onClick={() => addLine(sIdx)}>+ Add line</button>
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
