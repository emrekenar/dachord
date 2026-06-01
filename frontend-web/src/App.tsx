import './App.css';
import { useState, useEffect } from 'react';
import { Link, Outlet, useNavigate, useLocation } from 'react-router-dom';
import SearchBar from './Search/SearchBar';

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
  const [isLoggedIn, setIsLoggedIn] = useState(isTokenValid);

  useEffect(() => {
    setIsLoggedIn(isTokenValid());
  }, [location]);

  const isSearchPage = location.pathname === '/' || location.pathname === '/search';

  function handleLogout() {
    localStorage.removeItem('token');
    setIsLoggedIn(false);
    navigate('/');
  }

  return (
    <>
      <header className="app-header">
        <div className="app-header-top">
          <Link to="/" className="app-logo">dachord</Link>
          <nav>
            {isLoggedIn ? (
              <button className="nav-logout-btn" onClick={handleLogout}>Log out</button>
            ) : (
              <>
                <Link to="/register">Register</Link>
                <span className="nav-sep">|</span>
                <Link to="/login">Log in</Link>
              </>
            )}
          </nav>
        </div>
        {!isSearchPage && (
          <div className="header-searchbar">
            <SearchBar onNavigate={path => navigate(path)} />
          </div>
        )}
      </header>
      <Outlet />
    </>
  );
}

export default App;
