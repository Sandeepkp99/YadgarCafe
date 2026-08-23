# Yadgar Cafe - Quick Reference Guide

## ?? Quick Links

- **Frontend Setup**: `Frontend/yadgar-cafe-web/SETUP.md`
- **Production Deployment**: `PRODUCTION_DEPLOYMENT.md`
- **Implementation Details**: `IMPLEMENTATION_SUMMARY.md`

---

## ? Quick Start Commands

### Frontend Development

```bash
# Install dependencies (first time only)
cd Frontend/yadgar-cafe-web
npm install

# Start development server
npm run dev
# Visit: http://localhost:5173

# Build for production
npm run build

# Preview production build
npm run preview

# Run linter
npm run lint
```

### Backend Development

```bash
# Build backend
cd Backend
dotnet build

# Run backend
dotnet run --project YadgarCafe.API

# Backend runs on: http://localhost:5000

# Create database
dotnet ef database update

# Add migration
dotnet ef migrations add MigrationName
```

---

## ?? Default Environment Variables

### Frontend (.env.local)
```env
VITE_API_BASE_URL=http://localhost:5000/api
```

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YadgarCafe;Integrated Security=true;"
  },
  "Jwt": {
    "Key": "Your-Very-Long-Secret-Key-With-At-Least-32-Characters",
    "ExpirationMinutes": 60
  }
}
```

---

## ?? Application URLs

| Component | URL | Port |
|-----------|-----|------|
| Frontend | http://localhost:5173 | 5173 |
| Backend API | http://localhost:5000 | 5000 |
| Swagger UI | http://localhost:5000/swagger | 5000 |
| Database | (localdb) | - |

---

## ?? Main Pages & Routes

| Page | Route | Description |
|------|-------|-------------|
| Login | `/login` | User authentication |
| Register | `/register` | New user registration |
| Dashboard | `/dashboard` | Main dashboard with stats |
| Products | `/products` | Product management |
| Categories | `/categories` | Category management |
| Orders | `/orders` | Order management |
| Customers | `/customers` | Customer management |
| Inventory | `/inventory` | Stock management |

---

## ?? Authentication

### Test Credentials
After registering:
- Email: your@email.com
- Password: (your chosen password)

### Auth Flow
1. Visit `/login` or `/register`
2. Enter credentials
3. JWT token stored in localStorage
4. Redirect to `/dashboard`
5. Token auto-attached to all API calls
6. Auto-refresh on expiration

---

## ?? API Service Methods

### Auth Service
```typescript
await authService.login({ email, password })
await authService.register({ email, password, firstName, lastName })
await authService.logout()
```

### Product Service
```typescript
await productService.getAllProducts()
await productService.getProductsByCategory(categoryId)
await productService.createProduct(data)
await productService.updateProduct(id, data)
await productService.deleteProduct(id)
```

### Order Service
```typescript
await orderService.getAllOrders()
await orderService.getOrdersByCustomer(customerId)
await orderService.updateOrderStatus(id, { status })
```

### Customer Service
```typescript
await customerService.getAllCustomers()
await customerService.createCustomer(data)
await customerService.updateCustomer(id, data)
await customerService.deleteCustomer(id)
```

### Inventory Service
```typescript
await inventoryService.getAllInventory()
await inventoryService.getLowStockItems()
await inventoryService.updateInventory(id, data)
```

---

## ??? Common Tasks

### Add a New Page

1. Create page component:
```typescript
// src/pages/newpage/NewPage.tsx
export default function NewPage() {
  return <div>New Page</div>
}
```

2. Add route:
```typescript
// src/routes/AppRoutes.tsx
<Route path="/newpage" element={<ProtectedRoute><NewPage /></ProtectedRoute>} />
```

3. Add to sidebar:
```typescript
// src/components/layout/Sidebar/Sidebar.tsx
<Link to="/newpage">New Page</Link>
```

### Call API Endpoint

```typescript
import { someService } from '../../services/someService'

const [data, setData] = useState(null)

useEffect(() => {
  const fetchData = async () => {
    try {
      const result = await someService.getAllItems()
      setData(result)
    } catch (error) {
      toast.error('Failed to load data')
    }
  }
  fetchData()
}, [])
```

### Add Form Validation

```typescript
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'

const schema = z.object({
  email: z.string().email('Invalid email'),
  password: z.string().min(6, 'Min 6 characters'),
})

const { register, handleSubmit, formState: { errors } } = useForm({
  resolver: zodResolver(schema),
})
```

---

## ?? Debugging Tips

### Frontend
1. Open DevTools: F12
2. Check Console for errors
3. Check Network tab for API calls
4. Check Application ? LocalStorage for tokens

### Backend
1. Check console output
2. View logs in `logs/` folder
3. Use Swagger UI at `/swagger`
4. Check database in SQL Server

### API Issues
```bash
# Test API endpoint
curl http://localhost:5000/api/products

# With auth
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5000/api/products
```

---

## ?? Project Structure

```
Frontend/yadgar-cafe-web/
??? src/
?   ??? components/      # Reusable components
?   ??? context/         # React Context
?   ??? pages/           # Page components
?   ??? routes/          # Routing
?   ??? services/        # API services
?   ??? styles/          # Global styles
?   ??? App.tsx          # Main app
?   ??? main.tsx         # Entry point
??? public/              # Static files
??? dist/                # Production build
??? package.json         # Dependencies
??? tsconfig.json        # TypeScript config
??? vite.config.ts       # Vite config
??? .env.local          # Environment variables
```

---

## ?? File Naming Conventions

- **Components**: PascalCase (ProductList.tsx)
- **Services**: camelCase with Service suffix (productService.ts)
- **Styles**: Same as component name (ProductList.css)
- **Routes**: Same as component name (productRoutes.ts)
- **Types/Interfaces**: PascalCase (ProductResponse)

---

## ?? UI Component Usage

### Button
```typescript
<Button 
  title="Click Me"
  onClick={() => { }}
  disabled={isLoading}
/>
```

### Input
```typescript
<Input
  label="Email"
  type="email"
  placeholder="Enter email"
  {...register('email')}
/>
```

### Status Badge
```typescript
<span className={`badge ${item.available ? 'available' : 'unavailable'}`}>
  {item.available ? 'Available' : 'Unavailable'}
</span>
```

---

## ?? Security Checklist

- ? Never commit `.env.local` with real credentials
- ? Always use HTTPS in production
- ? Validate input on both frontend and backend
- ? Keep dependencies updated: `npm update`
- ? Use strong JWT secret (min 32 chars)
- ? Implement rate limiting on backend
- ? Enable CORS only for your domain
- ? Regular security audits

---

## ?? Performance Checklist

- ? Code splitting with React Router
- ? Lazy loading routes
- ? Optimize images
- ? Memoize components if needed
- ? Use async operations
- ? Enable gzip compression
- ? Database indexes
- ? Connection pooling

---

## ?? Deployment Checklist

**Before Deploying:**
- [ ] Run `npm run build`
- [ ] Test production build locally: `npm run preview`
- [ ] Update `.env` for production
- [ ] Run backend migrations
- [ ] Test all features
- [ ] Check console for errors
- [ ] Verify API connections
- [ ] Test on different browsers
- [ ] Security audit

**After Deploying:**
- [ ] Verify frontend loads
- [ ] Test login/logout
- [ ] Test all CRUD operations
- [ ] Check network requests
- [ ] Monitor errors
- [ ] Backup database

---

## ?? Support & Help

### Resources
- React Docs: https://react.dev
- TypeScript: https://www.typescriptlang.org
- Vite: https://vitejs.dev
- React Router: https://reactrouter.com
- Axios: https://axios-http.com

### Common Issues

**Module not found**
```bash
npm install
```

**Build fails**
```bash
npm run lint
npm run build
```

**API not responding**
- Check backend is running
- Check firewall/network
- Verify CORS settings

**Token not working**
- Check token in localStorage
- Verify token format
- Check expiration time

---

## ?? Notes

- Keep frontend and backend in sync
- Test API changes with frontend
- Update documentation when adding features
- Commit frequently with meaningful messages
- Code review before merging

---

## ?? Next Steps

1. **Immediate**
   - [ ] Install dependencies
   - [ ] Configure environment
   - [ ] Start dev servers
   - [ ] Test login flow

2. **Short Term**
   - [ ] Test all CRUD operations
   - [ ] Test error handling
   - [ ] Test validation
   - [ ] Test responsive design

3. **Medium Term**
   - [ ] Add more features
   - [ ] Performance optimization
   - [ ] Code review
   - [ ] Security audit

4. **Long Term**
   - [ ] Production deployment
   - [ ] Monitoring setup
   - [ ] Backup automation
   - [ ] Team training

---

**Last Updated**: 2024
**Status**: ? Production Ready
