import { useState, useEffect } from 'react';
import { inventoryService, type InventoryResponse } from '../../services/inventoryService';
import DashboardLayout from '../../components/layout/DashboardLayout/DashboardLayout';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import '../products/Products.css';

export default function Inventory() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [inventory, setInventory] = useState<InventoryResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    quantity: '',
    minStock: '',
    maxStock: '',
  });
  const [editingId, setEditingId] = useState<string | null>(null);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const data = await inventoryService.getAllInventory();
      setInventory(data);
    } catch (error) {
      toast.error('Failed to load inventory');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await inventoryService.updateInventory(editingId, {
          quantity: parseInt(formData.quantity),
          minStock: parseInt(formData.minStock),
          maxStock: parseInt(formData.maxStock),
        });
        toast.success('Inventory updated successfully');
      }
      fetchData();
      setShowForm(false);
      setFormData({ quantity: '', minStock: '', maxStock: '' });
      setEditingId(null);
    } catch (error) {
      toast.error('Failed to save inventory');
    }
  };

  const handleEdit = (inv: InventoryResponse) => {
    setFormData({
      quantity: inv.quantity.toString(),
      minStock: inv.minStock.toString(),
      maxStock: inv.maxStock.toString(),
    });
    setEditingId(inv.id);
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this inventory item?')) {
      try {
        await inventoryService.deleteInventory(id);
        toast.success('Inventory item deleted successfully');
        fetchData();
      } catch (error) {
        toast.error('Failed to delete inventory item');
      }
    }
  };

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <DashboardLayout onLogout={handleLogout}>
      <div className="products-container">
        <header className="page-header">
          <h1>Inventory Management</h1>
        </header>

        {showForm && (
          <form onSubmit={handleSubmit} className="product-form">
            <div className="form-group">
              <label>Quantity</label>
              <input
                type="number"
                required
                value={formData.quantity}
                onChange={(e) => setFormData({ ...formData, quantity: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Min Stock</label>
              <input
                type="number"
                required
                value={formData.minStock}
                onChange={(e) => setFormData({ ...formData, minStock: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Max Stock</label>
              <input
                type="number"
                required
                value={formData.maxStock}
                onChange={(e) => setFormData({ ...formData, maxStock: e.target.value })}
              />
            </div>
            <button type="submit" className="btn-primary">
              Update Inventory
            </button>
            <button
              type="button"
              className="btn-primary"
              onClick={() => {
                setShowForm(false);
                setEditingId(null);
                setFormData({ quantity: '', minStock: '', maxStock: '' });
              }}
            >
              Cancel
            </button>
          </form>
        )}

        {loading ? (
          <div className="loading">Loading inventory...</div>
        ) : inventory.length === 0 ? (
          <div className="empty-state">No inventory items found</div>
        ) : (
          <div className="products-table">
            <table>
              <thead>
                <tr>
                  <th>Ingredient</th>
                  <th>Quantity</th>
                  <th>Unit</th>
                  <th>Min Stock</th>
                  <th>Max Stock</th>
                  <th>Status</th>
                  <th>Last Restocked</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {inventory.map((item) => (
                  <tr key={item.id}>
                    <td>{item.ingredientName || 'N/A'}</td>
                    <td>{item.quantity}</td>
                    <td>{item.unit}</td>
                    <td>{item.minStock}</td>
                    <td>{item.maxStock}</td>
                    <td>
                      <span
                        className={`badge ${
                          item.quantity < item.minStock
                            ? 'unavailable'
                            : item.quantity > item.maxStock
                              ? 'unavailable'
                              : 'available'
                        }`}
                      >
                        {item.quantity < item.minStock
                          ? 'Low Stock'
                          : item.quantity > item.maxStock
                            ? 'Overstock'
                            : 'OK'}
                      </span>
                    </td>
                    <td>{new Date(item.lastRestocked).toLocaleDateString()}</td>
                    <td>
                      <button
                        className="btn-small btn-edit"
                        onClick={() => handleEdit(item)}
                      >
                        Edit
                      </button>
                      <button
                        className="btn-small btn-delete"
                        onClick={() => handleDelete(item.id)}
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </DashboardLayout>
  );
}
