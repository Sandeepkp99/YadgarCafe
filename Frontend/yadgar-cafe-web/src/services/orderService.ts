import apiClient from './api';

export interface OrderResponse {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName?: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  items?: OrderItemResponse[];
}

export interface OrderItemResponse {
  id: string;
  productId: string;
  productName?: string;
  quantity: number;
  price: number;
  subtotal: number;
}

export interface CreateOrderRequest {
  customerId: string;
  items: Array<{
    productId: string;
    quantity: number;
  }>;
}

export interface UpdateOrderStatusRequest {
  status: string;
}

export const orderService = {
  async getAllOrders(): Promise<OrderResponse[]> {
    const response = await apiClient.get<OrderResponse[]>('/orders');
    return response.data;
  },

  async getOrderById(id: string): Promise<OrderResponse> {
    const response = await apiClient.get<OrderResponse>(`/orders/${id}`);
    return response.data;
  },

  async getOrdersByCustomer(customerId: string): Promise<OrderResponse[]> {
    const response = await apiClient.get<OrderResponse[]>(`/orders/customer/${customerId}`);
    return response.data;
  },

  async getOrdersByStatus(status: string): Promise<OrderResponse[]> {
    const response = await apiClient.get<OrderResponse[]>(`/orders/status/${status}`);
    return response.data;
  },

  async createOrder(request: CreateOrderRequest): Promise<OrderResponse> {
    const response = await apiClient.post<OrderResponse>('/orders', request);
    return response.data;
  },

  async updateOrderStatus(id: string, request: UpdateOrderStatusRequest): Promise<OrderResponse> {
    const response = await apiClient.put<OrderResponse>(`/orders/${id}/status`, request);
    return response.data;
  },

  async deleteOrder(id: string): Promise<void> {
    await apiClient.delete(`/orders/${id}`);
  },
};
