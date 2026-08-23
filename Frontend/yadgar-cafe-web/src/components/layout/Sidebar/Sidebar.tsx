import { useState } from 'react';
import { Link } from 'react-router-dom';
import './Sidebar.css';

interface SidebarProps {
  onLogout: () => void;
}

export default function Sidebar({ onLogout }: SidebarProps) {
  const [isOpen, setIsOpen] = useState(true);

  const toggleSidebar = () => {
    setIsOpen(!isOpen);
  };

  return (
    <aside className={`sidebar ${isOpen ? 'open' : 'closed'}`}>
      <div className="sidebar-header">
        <h1 className="sidebar-logo">? Yadgar Cafe</h1>
        <button className="toggle-btn" onClick={toggleSidebar}>
          {isOpen ? '?' : '?'}
        </button>
      </div>

      <nav className="sidebar-nav">
        <ul>
          <li>
            <Link to="/dashboard" className="nav-link">
              <span className="icon">??</span>
              {isOpen && <span className="text">Dashboard</span>}
            </Link>
          </li>
          <li>
            <Link to="/products" className="nav-link">
              <span className="icon">???</span>
              {isOpen && <span className="text">Products</span>}
            </Link>
          </li>
          <li>
            <Link to="/categories" className="nav-link">
              <span className="icon">??</span>
              {isOpen && <span className="text">Categories</span>}
            </Link>
          </li>
          <li>
            <Link to="/orders" className="nav-link">
              <span className="icon">??</span>
              {isOpen && <span className="text">Orders</span>}
            </Link>
          </li>
          <li>
            <Link to="/customers" className="nav-link">
              <span className="icon">??</span>
              {isOpen && <span className="text">Customers</span>}
            </Link>
          </li>
          <li>
            <Link to="/inventory" className="nav-link">
              <span className="icon">??</span>
              {isOpen && <span className="text">Inventory</span>}
            </Link>
          </li>
        </ul>
      </nav>

      <div className="sidebar-footer">
        <button className="logout-btn" onClick={onLogout}>
          <span className="icon">??</span>
          {isOpen && <span className="text">Logout</span>}
        </button>
      </div>
    </aside>
  );
}
