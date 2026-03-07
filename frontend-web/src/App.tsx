import './App.css';
import { Link, Outlet } from 'react-router-dom';

function App() {
  return (
    <>
      <header className="app-header">
        <h1>dachord</h1>
        <nav>
          <Link to="/">All Tracks</Link> |{' '}
          <Link to="/submit">Submit Chord</Link>
        </nav>
      </header>
      <Outlet />
    </>
  );
}

export default App;
