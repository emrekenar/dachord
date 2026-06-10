import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import SearchBar from './SearchBar';
import SearchResults, { type TrackVersionsPair } from './SearchResults';
import ArtistView from './ArtistView';
import { apiFetch } from '../api';

export default function Search() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [results, setResults] = useState<TrackVersionsPair[]>([]);
  const [loading, setLoading] = useState(false);
  const [searched, setSearched] = useState(false);

  const q = searchParams.get('q') ?? '';
  const artistId = searchParams.get('artistId') ?? '';
  const artistName = searchParams.get('artistName') ?? '';
  const albumId = searchParams.get('albumId') ?? '';
  const albumName = searchParams.get('albumName') ?? '';

  useEffect(() => {
    if (artistId) return;

    const body: Record<string, string> = {};
    if (albumId) body.AlbumId = albumId;
    else if (q) body.Query = q;
    else return;

    const controller = new AbortController();
    setLoading(true);
    setSearched(false);

    apiFetch('/search', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
      signal: controller.signal,
    })
      .then(res => {
        setSearched(true);
        return res.ok ? res.json() : null;
      })
      .then(data => setResults(data?.results ?? []))
      .catch(e => { if ((e as Error).name !== 'AbortError') setResults([]); })
      .finally(() => setLoading(false));

    return () => controller.abort();
  }, [q, albumId, artistId]);

  return (
    <div className="search-page">
      <h2>{t('search.title')}</h2>
      <SearchBar key={searchParams.toString()} initialValue={q} onNavigate={navigate} />

      {artistId ? (
        <>
          <h3 className="search-heading">{t('search.albumsBy', { artist: artistName })}</h3>
          <ArtistView artistId={artistId} />
        </>
      ) : (
        <>
          {albumId && <h3 className="search-heading">{t('search.tracksFrom', { album: albumName })}</h3>}
          {q && <h3 className="search-heading">{t('search.resultsFor', { query: q })}</h3>}
          {loading && <p className="loading-state">{t('search.searching')}</p>}
          {searched && !loading && results.length === 0 && (
            <p className="empty-state">{t('search.noResults')}</p>
          )}
          <SearchResults results={results} />
        </>
      )}
    </div>
  );
}
