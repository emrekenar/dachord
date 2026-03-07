import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import App from './App'; // Corrected import statement
import SubmitChord from './SubmitTracks/SubmitChord';
import TracksList from './ListTracks/TracksList';
import TrackDetail from './GetTrack/TrackDetail';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<App />}>
          <Route index element={<TracksList />} />
          <Route path="submit" element={<SubmitChord />} />
          <Route path="track/:id" element={<TrackDetail />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>,
)
