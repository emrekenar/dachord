import { Link } from 'react-router-dom';

interface TrackResponse {
  trackId: string;
  title: string;
  artistName: string;
  albumName: string;
}

interface VersionSummary {
  contributorName?: string;
  likeCount: number;
  isApproved: boolean;
}

export interface TrackVersionsPair {
  track: TrackResponse;
  versions: VersionSummary[];
}

interface Props {
  results: TrackVersionsPair[];
  hideAlbum?: boolean;
}

export default function SearchResults({ results, hideAlbum = false }: Props) {
  const sorted = [...results].sort((a, b) => b.versions.length - a.versions.length);

  return (
    <div className="search-results">
      {sorted.map(({ track, versions }) => (
        <div key={track.trackId} className={`search-result-card ${versions.length === 0 ? 'no-versions-card' : ''}`}>
          <div className="track-info">
            <strong>{track.title}</strong>
            <span className="artist-name">{track.artistName}</span>
            {!hideAlbum && <span className="album-name">{track.albumName}</span>}
          </div>
          <div className="track-versions-summary">
            {versions.length === 0 ? (
              <span className="versions-badge zero">0 chord sheets</span>
            ) : (
              <span className="versions-badge has-versions">{versions.length} chord sheet{versions.length !== 1 ? 's' : ''}</span>
            )}
            {versions.length === 0 ? (
              <div className="no-versions">
                <Link to={`/submit/${track.trackId}`}>Add one?</Link>
              </div>
            ) : (
              <ul className="versions-list">
                {versions.map((v, i) => (
                  <li key={i} className="version-item">
                    <Link to={`/chords/${track.trackId}`}>
                      {v.contributorName ? `by ${v.contributorName}` : 'View chords'}
                    </Link>
                    {v.isApproved && <span className="approved-badge">✓ approved</span>}
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
