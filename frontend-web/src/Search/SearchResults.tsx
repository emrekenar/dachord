import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

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
  const { t } = useTranslation();
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
              <span className="versions-badge zero">{t('results.chordSheets', { count: 0 })}</span>
            ) : (
              <span className="versions-badge has-versions">{t('results.chordSheets', { count: versions.length })}</span>
            )}
            {versions.length === 0 ? (
              <div className="no-versions">
                <Link to={`/submit/${track.trackId}`}>{t('results.addOne')}</Link>
              </div>
            ) : (
              <ul className="versions-list">
                {versions.map((v, i) => (
                  <li key={i} className="version-item">
                    <Link to={`/chords/${track.trackId}`}>
                      {v.contributorName ? t('results.by', { name: v.contributorName }) : t('results.viewChords')}
                    </Link>
                    {v.isApproved && <span className="approved-badge">{t('results.approved')}</span>}
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
