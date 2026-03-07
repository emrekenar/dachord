import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';

export default function TrackDetail() {
  const { id } = useParams();
  const [track, setTrack] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(`https://localhost:7266/tracks/${id}`)
      .then(res => res.json())
      .then(data => {
        setTrack(data);
        setLoading(false);
      });
  }, [id]);

  if (loading) return <div>Loading track...</div>;
  if (!track) return <div>Track not found.</div>;

  return (
    <div className="track-detail">
      <h2>{track.title}</h2>
      {track.artist && <p><b>Artist:</b> {track.artist}</p>}
      {track.lyrics && (
        <div>
          <b>Lyrics:</b>
          <pre>{track.lyrics.content}</pre>
        </div>
      )}
    </div>
  );
}
