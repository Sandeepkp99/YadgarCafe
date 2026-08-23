# Yadgar Cafe - Production Frontend

A modern React + TypeScript web application for managing the Yadgar Cafe business operations including products, orders, customers, categories, and inventory management.

## Features

? **Complete API Integration**
- Authentication (Login/Register)
- Dashboard with real-time statistics
- Product Management (CRUD operations)
- Category Management
- Order Management with status tracking
- Customer Management
- Inventory Management with low stock alerts

?? **Security Features**
- JWT token-based authentication
- Automatic token refresh
- Protected routes
- Secure API communication

?? **Responsive Design**
- Mobile-first approach
- Collapsible sidebar navigation
- Optimized for all screen sizes
- Modern UI with Tailwind-inspired design

## Tech Stack

- **React 19** - UI Framework
- **TypeScript** - Type safety
- **React Router v7** - Navigation
- **React Hook Form** - Form management
- **Zod** - Schema validation
- **Axios** - HTTP client
- **React Toastify** - Notifications
- **Vite** - Build tool

## Prerequisites

- Node.js (v16 or higher)
- npm or yarn package manager
- Yadgar Cafe Backend API running on `http://localhost:5000`

## Setup & Installation

### 1. Install Dependencies

```bash
cd Frontend/yadgar-cafe-web
npm install
```

### 2. Configure Environment Variables

Create a `.env.local` file in the project root:

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

For production, update with your production API URL.

### 3. Start Development Server

```bash
npm run dev
```

The application will be available at `http://localhost:5173`

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Project Structure

```
src/
??? components/
?   ??? common/
?   ?   ??? Button.tsx
?   ?   ??? Input.tsx
?   ??? layout/
?       ??? DashboardLayout/
?       ??? AuthLayout/
?       ??? Sidebar/
??? context/
?   ??? AuthContext.tsx
??? pages/
?   ??? auth/
?   ?   ??? Login.tsx
?   ?   ??? Register.tsx
?   ??? dashboard/
?   ?   ??? Dashboard.tsx
?   ??? products/
?   ?   ??? Products.tsx
?   ??? categories/
?   ?   ??? Categories.tsx
?   ??? orders/
?   ?   ??? Orders.tsx
?   ??? customers/
?   ?   ??? Customers.tsx
?   ??? inventory/
?       ??? Inventory.tsx
??? routes/
?   ??? AppRoutes.tsx
?   ??? ProtectedRoute.tsx
??? services/
?   ??? api.ts
?   ??? authService.ts
?   ??? productService.ts
?   ??? categoryService.ts
?   ??? orderService.ts
?   ??? customerService.ts
?   ??? inventoryService.ts
?   ??? dashboardService.ts
??? styles/
?   ??? globals.css
??? App.tsx
??? main.tsx
```

## API Integration

All API services are abstracted in the `services` folder:

### Authentication Service
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
await orderService.updateOrderStatus(id, status)
```

### Customer Service
```typescript
await customerService.getAllCustomers()
await customerService.createCustomer(data)
await customerService.updateCustomer(id, data)
```

### Inventory Service
```typescript
await inventoryService.getAllInventory()
await inventoryService.getLowStockItems()
await inventoryService.getInventoryHistory(inventoryId)
```

## Authentication Flow

1. User registers or logs in
2. Backend returns JWT tokens (access & refresh)
3. Tokens stored in localStorage
4. Axios interceptor adds token to all API requests
5. On 401 error, automatically refresh token
6. Protected routes redirect unauthenticated users to login

## State Management

Uses React Context API for:
- Authentication state (`AuthContext`)
- User information
- Login/logout/register functions

## Form Validation

Forms use React Hook Form with Zod schema validation:
- Email validation
- Password requirements (min 6 characters)
- Matching password confirmation
- Required fields

## UI Components

### Sidebar Navigation
- Collapsible menu with all main sections
- Quick logout option
- Icon-based navigation for mobile

### Dashboard
- Real-time statistics cards
- Today's sales
- Monthly sales
- Total orders
- Low stock alerts
- Quick access links

### Data Tables
- Sortable columns
- Action buttons (Edit/Delete)
- Status badges
- Responsive design

## Error Handling

- Toast notifications for user feedback
- API error handling with user-friendly messages
- Automatic token refresh on expiration
- Graceful logout on auth failure

## Production Build

```bash
npm run build
```

This creates an optimized build in the `dist` folder.

### Deploy to Production

1. Build the application
2. Deploy `dist` folder to your hosting service (Vercel, Netlify, etc.)
3. Update `VITE_API_BASE_URL` in environment variables
4. Configure CORS on backend for production domain

## Troubleshooting

### CORS Issues
Ensure backend is configured to accept requests from your frontend domain.

### API Connection Failed
- Verify backend is running on correct port
- Check `VITE_API_BASE_URL` in `.env.local`
- Check browser console for detailed errors

### Login Not Working
- Verify backend auth endpoint is accessible
- Check if user credentials are correct
- Check localStorage for token storage

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Performance Optimizations

- Code splitting with React Router
- Lazy loading of routes
- Memoization of components
- Optimized re-renders
- Tree-shaking with Vite

## Security Notes

- Never commit `.env.local` with real credentials
- Tokens are stored in localStorage (consider using httpOnly cookies for production)
- Always validate input on backend
- Use HTTPS in production
- Enable CORS properly for production domain

## Contributing

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Submit a pull request

## License

© 2024 Yadgar Cafe. All rights reserved.

## Support

For issues or questions, please contact the development team.
