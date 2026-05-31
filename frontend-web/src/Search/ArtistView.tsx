import { useEffect, useState } from 'react';
import { apiFetch } from '../api';
import SearchResults, { type TrackVersionsPair } from './SearchResults';

interface AlbumInfo {
  albumId: string;
  albumName: string;
  imageUrl?: string;
  releaseYear?: string;
}

interface Props {
  artistId: string;
}

export default function ArtistView({ artistId }: Props) {
  const [albums, setAlbums] = useState<AlbumInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [expanded, setExpanded] = useState<Set<string>>(new Set());
  const [albumTracks, setAlbumTracks] = useState<Record<string, TrackVersionsPair[]>>({});
  const [loadingAlbums, setLoadingAlbums] = useState<Set<string>>(new Set());

  useEffect(() => {
    apiFetch(`/artist/${artistId}/albums`)
      .then(res => (res.ok ? res.json() : []))
      .then(data => { setAlbums(data); setLoading(false); })
      .catch(() => setLoading(false));
  }, [artistId]);

  async function toggleAlbum(albumId: string) {
    if (expanded.has(albumId)) {
      setExpanded(prev => { const s = new Set(prev); s.delete(albumId); return s; });
      return;
    }

    setExpanded(prev => new Set(prev).add(albumId));

    if (albumTracks[albumId] !== undefined) return;

    setLoadingAlbums(prev => new Set(prev).add(albumId));
    try {
      const res = await apiFetch('/search', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ AlbumId: albumId }),
      });
      const data = res.ok ? await res.json() : null;
      setAlbumTracks(prev => ({ ...prev, [albumId]: data?.results ?? [] }));
    } finally {
      setLoadingAlbums(prev => { const s = new Set(prev); s.delete(albumId); return s; });
    }
  }

  if (loading) return <p className="loading-state">Loading albums…</p>;
  if (albums.length === 0) return <p className="empty-state">No albums found.</p>;

  return (
    <div className="artist-discography">
      {albums.map(album => {
        const isExpanded = expanded.has(album.albumId);
        const isLoadingTracks = loadingAlbums.has(album.albumId);
        const tracks = albumTracks[album.albumId];

        return (
          <div key={album.albumId} className="album-row">
            <button className="album-header" onClick={() => toggleAlbum(album.albumId)}>
              {album.imageUrl && (
                <img src={album.imageUrl} alt={album.albumName} className="album-thumb" />
              )}
              <span className="album-header-info">
                <span className="album-title">{album.albumName}</span>
                {album.releaseYear && <span className="album-year">{album.releaseYear}</span>}
              </span>
              <span className="album-chevron">{isExpanded ? '▾' : '▸'}</span>
            </button>
            {isExpanded && (
              <div className="album-tracks">
                {isLoadingTracks ? (
                  <p className="loading-state">Loading tracks…</p>
                ) : (
                  <SearchResults results={tracks ?? []} hideAlbum />
                )}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
