import { useState, useEffect } from 'react';
import { apiFetch } from '../api';

export interface TrackSuggestion {
  trackId: string;
  title: string;
  artistId: string;
  artistName: string;
  albumId: string;
  albumName: string;
  url?: string;
}

export function useSearchSuggestions(query: string): { suggestions: TrackSuggestion[]; loading: boolean } {
  const [suggestions, setSuggestions] = useState<TrackSuggestion[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (query.length < 2) {
      setSuggestions([]);
      return;
    }

    const controller = new AbortController();

    const timer = setTimeout(async () => {
      setLoading(true);
      try {
        const res = await apiFetch('/searchTracks', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ Query: query }),
          signal: controller.signal,
        });
        if (res.ok) {
          const data = await res.json();
          setSuggestions(data.results ?? []);
        } else {
          setSuggestions([]);
        }
      } catch (e) {
        if ((e as Error).name !== 'AbortError') setSuggestions([]);
      } finally {
        setLoading(false);
      }
    }, 350);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [query]);

  return { suggestions, loading };
}
