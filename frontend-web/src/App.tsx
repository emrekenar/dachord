import './App.css';
import { Link, Outlet } from 'react-router-dom';

function App() {
  return (
    <>
      <header className="app-header">
        <h1>dachord</h1>
        <nav>
          <Link to="/">Search</Link> |{' '}
          <Link to="/submit">Submit Chords</Link> |{' '}
          <Link to="/login">Login</Link> |{' '}
          <Link to="/register">Register</Link>
        </nav>
      </header>
      <Outlet />
    </>
  );
}

export default App;
