import { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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

function getContributorFromToken(): { id: string; email: string; displayName: string } | null {
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
    const displayName = payload.displayName ?? '';
    return id ? { id, email: email ?? '', displayName } : null;
  } catch {
    return null;
  }
}

export default function SubmitChord() {
  const { t } = useTranslation();
  const { trackId } = useParams<{ trackId?: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const editSections = (location.state as { editSections?: ApiSection[] } | null)?.editSections;
  const isEditing = !!editSections && editSections.length > 0;
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
        if (!data) return;
        const track = { trackId: data.id, title: data.title, artistName: data.artistName, albumName: data.albumName };
        if (isEditing) {
          setSelectedTrack(track);
          setTrackResults([]);
          setSections(editSections!.map(s => ({
            id: crypto.randomUUID(),
            type: s.type || null,
            lines: s.lines.map(l => ({
              id: crypto.randomUUID(),
              lyrics: l.lyrics,
              chords: Object.entries(l.chords).map(([pos, chord]) => ({ position: Number(pos), chord })),
              timeMs: l.timeMs,
            })),
          })));
          setLyricsStatus('done');
          setMessage('');
        } else {
          selectTrack(track);
        }
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
    if (!token) { setMessage(t('submit.loginToImport')); return; }

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

  function insertLine(sIdx: number, atIdx: number) {
    setSections(prev => prev.map((s, i) => {
      if (i !== sIdx) return s;
      const lines = [...s.lines];
      lines.splice(atIdx, 0, newLine());
      return { ...s, lines };
    }));
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
    if (!contributor) { setMessage(t('submit.loginToSubmit')); return; }
    if (!selectedTrack) { setMessage(t('submit.selectTrack')); return; }
    if (sections.length === 0) { setMessage(t('submit.addSection')); return; }

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
          ContributorName: contributor.displayName || null,
          Content: sections.map(s => ({
            Type: s.type ?? '',
            Lines: s.lines.map(l => ({
              Lyrics: l.lyrics,
              Chords: Object.fromEntries(l.chords.map(c => [String(c.position), c.chord])),
              TimeMs: l.timeMs,
            })),
          })),
        }),
      });
      if (res.ok) { sessionStorage.setItem('toast', t('submit.submitted')); navigate(-1); return; }
      else if (res.status === 401) setMessage(t('submit.loginToSubmit'));
      else if (res.status === 400) setMessage(t('submit.invalidRequest'));
      else setMessage(t('submit.failed'));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="submit-chord-page">
      <h2>{isEditing ? t('submit.editTitle') : t('submit.title')}</h2>

      {!selectedTrack ? (
        <div className="track-search-section">
          <p className="section-hint">{t('submit.findTrack')}</p>
          <form className="search-form" onSubmit={searchTracks}>
            <input
              type="text"
              placeholder={t('submit.placeholder')}
              value={query}
              onChange={e => setQuery(e.target.value)}
            />
            <button type="submit" disabled={searching || query.length < 2}>
              {searching ? t('submit.searching') : t('submit.search')}
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
              {t('submit.changeTrack')}
            </button>
          </div>

          {/* Lyrics import */}
          <div className="lyrics-import-bar">
            <button
              className="btn-import-lyrics"
              onClick={importLyrics}
              disabled={lyricsStatus === 'loading'}
            >
              {lyricsStatus === 'loading' ? t('submit.fetchingLyrics') : lyricsStatus === 'done' ? t('submit.reimportLyrics') : t('submit.importLyrics')}
            </button>
            {lyricsStatus === 'loading' && (
              <span className="import-status import-status--info">{t('submit.importInfo')}</span>
            )}
            {lyricsStatus === 'done' && (
              <span className="import-status import-status--ok">{t('submit.importOk')}</span>
            )}
            {lyricsStatus === 'not_found' && (
              <span className="import-status import-status--warn">{t('submit.importNotFound')}</span>
            )}
            {lyricsStatus === 'error' && (
              <span className="import-status import-status--err">{t('submit.importError')}</span>
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
                      <option value="">{t('submit.removeLabel')}</option>
                      {SECTION_TYPES.map(type => <option key={type}>{type}</option>)}
                    </select>
                  ) : (
                    <button className="btn-add-label" onClick={() => updateSectionType(sIdx, 'Verse')}>
                      {t('submit.addLabel')}
                    </button>
                  )}
                  <button className="btn-danger-sm" onClick={() => deleteSection(sIdx)}>{t('submit.deletePart')}</button>
                </div>

                {section.lines.map((line, lIdx) => (
                  <ChordLine
                    key={line.id}
                    line={line}
                    showDelete={section.lines.length > 1}
                    onChange={updated => updateLine(sIdx, lIdx, updated)}
                    onDelete={() => deleteLine(sIdx, lIdx)}
                    onInsertBefore={() => insertLine(sIdx, lIdx)}
                  />
                ))}

                <button className="btn-add-line" onClick={() => addLine(sIdx)}>{t('submit.addLine')}</button>

                {sIdx < sections.length - 1 && <hr className="section-break" />}
              </div>
            ))}
          </div>

          <div className="editor-actions">
            <button className="btn-add-part" onClick={addSection}>{t('submit.addPart')}</button>
            {sections.length > 0 && (
              <button className="btn-submit" onClick={handleSubmit} disabled={submitting}>
                {submitting ? t('submit.submitting') : t('submit.submit')}
              </button>
            )}
          </div>

          {message && <div className="submit-message">{message}</div>}
        </div>
      )}
    </div>
  );
}
