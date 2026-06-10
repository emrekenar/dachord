import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './i18n';

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import App from './App';
import Login from './Login/Login';
import Register from './Register/Register';
import Search from './Search/Search';
import SubmitChord from './SubmitTracks/SubmitChord';
import ChordView from './ChordView/ChordView';
import TrackDetail from './GetTrack/TrackDetail';
import Profile from './Profile/Profile';
import PublicProfile from './Profile/PublicProfile';
import Moderation from './Moderation/Moderation';
import Admin from './Admin/Admin';
import Feedback from './Feedback/Feedback';

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
          <Route path="profile" element={<Profile />} />
          <Route path="users/:id" element={<PublicProfile />} />
          <Route path="moderation" element={<Moderation />} />
          <Route path="admin" element={<Admin />} />
          <Route path="feedback" element={<Feedback />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>,
)
