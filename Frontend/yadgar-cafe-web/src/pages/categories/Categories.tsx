import { useState, useEffect } from 'react';
import { categoryService, type CategoryResponse } from '../../services/categoryService';
import DashboardLayout from '../../components/layout/DashboardLayout/DashboardLayout';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import '../products/Products.css';

export default function Categories() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [categories, setCategories] = useState<CategoryResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({ name: '', description: '' });
  const [editingId, setEditingId] = useState<string | null>(null);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const data = await categoryService.getAllCategories();
      setCategories(data);
    } catch (error) {
      toast.error('Failed to load categories');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await categoryService.updateCategory(editingId, {
          name: formData.name,
          description: formData.description,
        });
        toast.success('Category updated successfully');
      } else {
        await categoryService.createCategory({
          name: formData.name,
          description: formData.description,
        });
        toast.success('Category created successfully');
      }
      fetchData();
      setShowForm(false);
      setFormData({ name: '', description: '' });
      setEditingId(null);
    } catch (error) {
      toast.error('Failed to save category');
    }
  };

  const handleEdit = (category: CategoryResponse) => {
    setFormData({ name: category.name, description: category.description || '' });
    setEditingId(category.id);
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this category?')) {
      try {
        await categoryService.deleteCategory(id);
        toast.success('Category deleted successfully');
        fetchData();
      } catch (error) {
        toast.error('Failed to delete category');
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
          <h1>Categories</h1>
          <button
            className="btn-primary"
            onClick={() => {
              setShowForm(!showForm);
              setEditingId(null);
              setFormData({ name: '', description: '' });
            }}
          >
            {showForm ? 'Cancel' : '+ New Category'}
          </button>
        </header>

        {showForm && (
          <form onSubmit={handleSubmit} className="product-form">
            <div className="form-group">
              <label>Category Name</label>
              <input
                type="text"
                required
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
            </div>
            <button type="submit" className="btn-primary">
              {editingId ? 'Update' : 'Create'} Category
            </button>
          </form>
        )}

        {loading ? (
          <div className="loading">Loading categories...</div>
        ) : categories.length === 0 ? (
          <div className="empty-state">No categories found</div>
        ) : (
          <div className="products-table">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {categories.map((category) => (
                  <tr key={category.id}>
                    <td>{category.name}</td>
                    <td>{category.description || 'N/A'}</td>
                    <td>
                      <button
                        className="btn-small btn-edit"
                        onClick={() => handleEdit(category)}
                      >
                        Edit
                      </button>
                      <button
                        className="btn-small btn-delete"
                        onClick={() => handleDelete(category.id)}
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
