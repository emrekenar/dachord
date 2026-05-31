import { useState, useRef, useEffect } from 'react';
import { useSearchSuggestions, type TrackSuggestion } from './useSearchSuggestions';

interface Props {
  initialValue?: string;
  onNavigate: (path: string) => void;
}

export default function SearchBar({ initialValue = '', onNavigate }: Props) {
  const [query, setQuery] = useState(initialValue);
  const [open, setOpen] = useState(false);
  const { suggestions } = useSearchSuggestions(query);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setOpen(suggestions.length > 0);
  }, [suggestions]);

  useEffect(() => {
    function handleMouseDown(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleMouseDown);
    return () => document.removeEventListener('mousedown', handleMouseDown);
  }, []);

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Enter' && query.trim().length > 2) {
      setOpen(false);
      onNavigate(`/search?q=${encodeURIComponent(query.trim())}`);
    }
  }

  function navigate(path: string) {
    setOpen(false);
    onNavigate(path);
  }

  return (
    <div ref={containerRef} className="search-bar-wrapper">
      <div className="search-form">
        <input
          type="text"
          placeholder="Search by song or artist..."
          value={query}
          onChange={e => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
        />
      </div>
      {open && (
        <ul className="search-dropdown">
          {suggestions.map((s: TrackSuggestion) => (
            <li key={s.trackId} className="search-dropdown-item">
              <button
                className="suggestion-title"
                onClick={() => navigate(`/track/${s.trackId}`)}
              >
                {s.title}
              </button>
              <div className="suggestion-meta">
                <button
                  className="suggestion-artist"
                  onClick={() => navigate(`/search?artistId=${s.artistId}&artistName=${encodeURIComponent(s.artistName)}`)}
                >
                  {s.artistName}
                </button>
                <span className="suggestion-sep">·</span>
                <button
                  className="suggestion-album"
                  onClick={() => navigate(`/search?albumId=${s.albumId}&albumName=${encodeURIComponent(s.albumName)}`)}
                >
                  {s.albumName}
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
