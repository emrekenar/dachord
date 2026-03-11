import { useState } from 'react';

export default function SubmitChord() {
  const [title, setTitle] = useState('');
  const [lyrics, setLyrics] = useState('');
  const [message, setMessage] = useState('');

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const data = {
      Title: title,
      Lyrics: {
        Content: lyrics,
        Chords: {}
      }
    };
    fetch('https://localhost:7266/tracks', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json', 
        'Authorization': `Bearer ${localStorage.getItem('token')}` 
      },
      body: JSON.stringify(data)
    })
      .then(res => {
        if (res.ok) setMessage('Track submitted!');
        else if (res.status === 401) setMessage('Please login to submit a track.');
        else if (res.status === 400) setMessage('Invalid track data.');
        else setMessage('Failed to submit track.');
      })
      .catch(() => setMessage('Failed to submit track.'));
  }

  return (
    <div className="submit-chord-page">
      <h2>Submit Chord</h2>
      <form className="submit-chords-form" onSubmit={handleSubmit}>
        <label htmlFor="track-name">Track Name</label>
        <input id="track-name" name="track-name" type="text" placeholder="Enter track name" value={title} onChange={e => setTitle(e.target.value)} />
        <label htmlFor="track-lyrics">Lyrics</label>
        <textarea id="track-lyrics" name="track-lyrics" placeholder="Enter lyrics" value={lyrics} onChange={e => setLyrics(e.target.value)} />
        <button type="submit">Submit Chord</button>
      </form>
      {message && <div className="submit-message">{message}</div>}
    </div>
  );
}
