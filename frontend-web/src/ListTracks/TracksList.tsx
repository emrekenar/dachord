import { useEffect, useState } from 'react';

export default function TracksList() {
  const [tracks, setTracks] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch('https://localhost:7266/tracks')
      .then(res => res.json())
      .then(data => {
        setTracks(data);
        setLoading(false);
      });
  }, []);

  if (loading) return <div>Loading tracks...</div>;

  return (
    <div className="tracks-list">
      <h2>All Tracks</h2>
      <ul>
        {tracks.map(track => (
          <li key={track.id}>
            <a href={`/track/${track.id}`}>{track.title}</a>
          </li>
        ))}
      </ul>
    </div>
  );
}
