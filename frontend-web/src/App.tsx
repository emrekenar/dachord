import './App.css';
import { useState, useEffect } from 'react';
import { Link, Outlet, useNavigate, useLocation } from 'react-router-dom';
import SearchBar from './Search/SearchBar';

function App() {
  const navigate = useNavigate();
  const location = useLocation();
  const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));

  useEffect(() => {
    setIsLoggedIn(!!localStorage.getItem('token'));
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
