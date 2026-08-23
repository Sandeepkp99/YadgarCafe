import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { dashboardService } from '../../services/dashboardService';
import DashboardLayout from '../../components/layout/DashboardLayout/DashboardLayout';
import { toast } from 'react-toastify';
import './Dashboard.css';

interface DashboardStats {
  todaysSales: number;
  monthlySales: number;
  totalOrders: number;
  lowStockCount: number;
}

export default function Dashboard() {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchStats = async () => {
      try {
        const data = await dashboardService.getStatistics();
        setStats({
          todaysSales: data.todaysSales,
          monthlySales: data.monthlySales,
          totalOrders: data.totalOrders,
          lowStockCount: data.lowStockCount,
        });
      } catch (error) {
        toast.error('Failed to load dashboard statistics');
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, []);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <DashboardLayout onLogout={handleLogout}>
      <div className="dashboard-container">
        <header className="dashboard-header">
          <h1>Welcome, {user?.firstName}!</h1>
          <p>Dashboard</p>
        </header>

        {loading ? (
          <div className="loading">Loading dashboard...</div>
        ) : stats ? (
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-icon today">??</div>
              <div className="stat-content">
                <h3>Today's Sales</h3>
                <p className="stat-value">${stats.todaysSales.toFixed(2)}</p>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon monthly">??</div>
              <div className="stat-content">
                <h3>Monthly Sales</h3>
                <p className="stat-value">${stats.monthlySales.toFixed(2)}</p>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon orders">??</div>
              <div className="stat-content">
                <h3>Total Orders</h3>
                <p className="stat-value">{stats.totalOrders}</p>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-icon warning">??</div>
              <div className="stat-content">
                <h3>Low Stock Items</h3>
                <p className="stat-value">{stats.lowStockCount}</p>
              </div>
            </div>
          </div>
        ) : null}

        <div className="quick-links">
          <h2>Quick Links</h2>
          <div className="links-grid">
            <a href="/products" className="link-button">
              <span className="icon">???</span>
              <span>Products</span>
            </a>
            <a href="/orders" className="link-button">
              <span className="icon">??</span>
              <span>Orders</span>
            </a>
            <a href="/customers" className="link-button">
              <span className="icon">??</span>
              <span>Customers</span>
            </a>
            <a href="/inventory" className="link-button">
              <span className="icon">??</span>
              <span>Inventory</span>
            </a>
            <a href="/categories" className="link-button">
              <span className="icon">??</span>
              <span>Categories</span>
            </a>
          </div>
        </div>
      </div>
    </DashboardLayout>
  );
}
