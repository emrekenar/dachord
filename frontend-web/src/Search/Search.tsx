import { useState } from 'react';
import { Link } from 'react-router-dom';

interface TrackResponse {
  trackId: string;
  title: string;
  artistName: string;
  albumName: string;
  url?: string;
}

interface VersionSummary {
  contributorName?: string;
  likeCount: number;
  isApproved: boolean;
}

interface TrackVersionsPair {
  track: TrackResponse;
  versions: VersionSummary[];
}

export default function Search() {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<TrackVersionsPair[]>([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  async function handleSearch(e: { preventDefault(): void }) {
    e.preventDefault();
    if (query.length <= 2) return;
    setLoading(true);
    try {
      const res = await fetch('https://localhost:7266/search', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Query: query }),
      });
      setSearched(true);
      if (res.ok) {
        const data = await res.json();
        setResults(data.results ?? []);
      } else {
        setResults([]);
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="search-page">
      <h2>Search Chord Sheets</h2>
      <form className="search-form" onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="Search by song or artist..."
          value={query}
          onChange={e => setQuery(e.target.value)}
        />
        <button type="submit" disabled={loading || query.length <= 2}>
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {searched && !loading && results.length === 0 && (
        <p className="empty-state">
          No chord sheets found.{' '}
          <Link to="/submit">Be the first to add one!</Link>
        </p>
      )}

      <div className="search-results">
        {results.map(({ track, versions }) => (
          <div key={track.trackId} className="search-result-card">
            <div className="track-info">
              <strong>{track.title}</strong>
              <span className="artist-name">{track.artistName}</span>
              <span className="album-name">{track.albumName}</span>
            </div>
            {versions.length === 0 ? (
              <div className="no-versions">
                No chord sheets yet —{' '}
                <Link to="/submit">add one?</Link>
              </div>
            ) : (
              <ul className="versions-list">
                {versions.map((v, i) => (
                  <li key={i} className="version-item">
                    <Link to={`/chords/${track.trackId}`}>
                      {v.contributorName ? `by ${v.contributorName}` : 'View chords'}
                    </Link>
                    <span className="like-count">♥ {v.likeCount}</span>
                    {v.isApproved && <span className="approved-badge">✓ approved</span>}
                  </li>
                ))}
              </ul>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
