import { useState, useEffect, useRef } from 'react';

export interface ChordEntry { position: number; chord: string; }
export interface LineData { id: string; lyrics: string; chords: ChordEntry[]; }

interface Props {
  line: LineData;
  onChange: (updated: LineData) => void;
  onDelete?: () => void;
  showDelete: boolean;
}

// Cached so we only hit the DOM once per page load
let _cachedCharWidth: number | null = null;

function measureCharWidth(): number {
  if (_cachedCharWidth !== null) return _cachedCharWidth;
  const span = document.createElement('span');
  span.style.cssText =
    'font-family:monospace;font-size:0.95rem;visibility:hidden;position:fixed;white-space:pre;pointer-events:none';
  span.textContent = 'M'.repeat(20);
  document.body.appendChild(span);
  _cachedCharWidth = span.getBoundingClientRect().width / 20;
  document.body.removeChild(span);
  return _cachedCharWidth;
}

export default function ChordLine({ line, onChange, onDelete, showDelete }: Props) {
  const [editState, setEditState] = useState<{ pos: number; value: string } | null>(null);
  const [charWidth, setCharWidth] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => { setCharWidth(measureCharWidth()); }, []);
  useEffect(() => { if (editState !== null) inputRef.current?.focus(); }, [editState]);

  function handleRowClick(e: React.MouseEvent<HTMLDivElement>) {
    if (charWidth === 0) return;
    e.stopPropagation();
    const rect = e.currentTarget.getBoundingClientRect();
    const pos = Math.max(0, Math.floor((e.clientX - rect.left) / charWidth));
    const existing = line.chords.find(
      c => pos >= c.position && pos < c.position + Math.max(c.chord.length, 1)
    );
    setEditState({ pos: existing?.position ?? pos, value: existing?.chord ?? '' });
  }

  function commit() {
    if (!editState) return;
    const others = line.chords.filter(c => c.position !== editState.pos);
    const trimmed = editState.value.trim();
    onChange({
      ...line,
      chords: trimmed ? [...others, { position: editState.pos, chord: trimmed }] : others,
    });
    setEditState(null);
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Enter' || e.key === 'Tab') { e.preventDefault(); commit(); }
    else if (e.key === 'Escape') setEditState(null);
    else if (e.key === 'Backspace' && editState?.value === '') {
      onChange({ ...line, chords: line.chords.filter(c => c.position !== editState.pos) });
      setEditState(null);
    }
  }

  const sorted = [...line.chords].sort((a, b) => a.position - b.position);

  return (
    <div className="chord-line">
      {/* Chord row — click anywhere to place or edit a chord */}
      <div className="chord-row" onClick={handleRowClick} title="Click to add a chord">
        {sorted.map(c =>
          editState?.pos === c.position ? null : (
            <span
              key={c.position}
              className="chord-token"
              style={charWidth > 0 ? { left: c.position * charWidth } : undefined}
            >
              {c.chord}
            </span>
          )
        )}
        {editState !== null && charWidth > 0 && (
          <input
            ref={inputRef}
            className="chord-inline-input"
            value={editState.value}
            style={{ left: editState.pos * charWidth }}
            placeholder="Am"
            onChange={e => setEditState({ ...editState, value: e.target.value })}
            onKeyDown={handleKeyDown}
            onBlur={commit}
          />
        )}
      </div>

      {/* Lyrics row */}
      <div className="line-row">
        <input
          className="lyrics-input"
          type="text"
          placeholder="Lyrics…"
          value={line.lyrics}
          onChange={e => onChange({ ...line, lyrics: e.target.value })}
        />
        {showDelete && (
          <button className="btn-remove-line" onClick={onDelete} title="Remove line">×</button>
        )}
      </div>
    </div>
  );
}
