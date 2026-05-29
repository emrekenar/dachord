import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import App from './App';
import Login from './Login/Login';
import Register from './Register/Register';
import Search from './Search/Search';
import SubmitChord from './SubmitTracks/SubmitChord';
import ChordView from './ChordView/ChordView';
import TrackDetail from './GetTrack/TrackDetail';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<App />}>
          <Route index element={<Search />} />
          <Route path="search" element={<Search />} />
          <Route path="submit/:trackId" element={<SubmitChord />} />
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="chords/:id" element={<ChordView />} />
          <Route path="track/:id" element={<TrackDetail />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>,
)
