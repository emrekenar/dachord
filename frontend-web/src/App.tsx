import './App.css';
import { useState, useEffect } from 'react';
import { Link, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import SearchBar from './Search/SearchBar';
import { canModerate, isAdmin } from './auth';

function isTokenValid(): boolean {
  const token = localStorage.getItem('token');
  if (!token) return false;
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    if (payload.exp && payload.exp * 1000 < Date.now()) {
      localStorage.removeItem('token');
      return false;
    }
    return true;
  } catch {
    localStorage.removeItem('token');
    return false;
  }
}

function App() {
  const navigate = useNavigate();
  const location = useLocation();
  const { t, i18n } = useTranslation();
  const [isLoggedIn, setIsLoggedIn] = useState(isTokenValid);
  const [toast, setToast] = useState('');

  useEffect(() => {
    setIsLoggedIn(isTokenValid());
  }, [location]);

  function toggleLanguage() {
    i18n.changeLanguage(i18n.language.startsWith('tr') ? 'en' : 'tr');
  }

  useEffect(() => {
    const msg = (location.state as { toast?: string } | null)?.toast
      ?? sessionStorage.getItem('toast') ?? '';
    sessionStorage.removeItem('toast');
    if (!msg) return;
    setToast(msg);
    const id = setTimeout(() => setToast(''), 2500);
    return () => clearTimeout(id);
  }, [location.key]);

  const isSearchPage = location.pathname === '/' || location.pathname === '/search';

  function handleLogout() {
    localStorage.removeItem('token');
    setIsLoggedIn(false);
    navigate('/');
    setToast(t('common.loggedOut'));
  }

  const showModeration = isLoggedIn && canModerate();
  const showAdmin = isLoggedIn && isAdmin();

  return (
    <>
      <header className="app-header">
        <div className="app-header-top">
          <Link to="/" className="app-logo">dachord</Link>
          <nav>
            {showModeration && (
              <>
                <Link to="/moderation" className="nav-profile-link">{t('nav.moderation')}</Link>
                <span className="nav-sep">|</span>
              </>
            )}
            {showAdmin && (
              <>
                <Link to="/admin" className="nav-profile-link">{t('nav.admin')}</Link>
                <span className="nav-sep">|</span>
              </>
            )}
            {isLoggedIn ? (
              <>
                <Link to="/feedback" className="nav-profile-link">{t('nav.feedback')}</Link>
                <span className="nav-sep">|</span>
                <Link to="/profile" className="nav-profile-link">{t('nav.profile')}</Link>
                <span className="nav-sep">|</span>
                <button className="nav-logout-btn" onClick={handleLogout}>{t('nav.logout')}</button>
              </>
            ) : (
              <>
                <Link to="/register">{t('nav.register')}</Link>
                <span className="nav-sep">|</span>
                <Link to="/login">{t('nav.login')}</Link>
              </>
            )}
            <span className="nav-sep">|</span>
            <button className="nav-lang-btn" onClick={toggleLanguage}>
              {i18n.language.startsWith('tr') ? 'EN' : 'TR'}
            </button>
          </nav>
        </div>
        {!isSearchPage && (
          <div className="header-searchbar">
            <SearchBar onNavigate={path => navigate(path)} />
          </div>
        )}
      </header>
      <Outlet />
      {toast && <div className="toast">{toast}</div>}
    </>
  );
}

export default App;
