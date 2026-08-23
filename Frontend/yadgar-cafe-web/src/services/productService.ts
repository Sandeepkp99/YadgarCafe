import apiClient from './api';

export interface ProductResponse {
  id: string;
  name: string;
  description: string;
  price: number;
  categoryId: string;
  categoryName?: string;
  imageUrl?: string;
  isAvailable: boolean;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  categoryId: string;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  price?: number;
  categoryId?: string;
}

export const productService = {
  async getAllProducts(): Promise<ProductResponse[]> {
    const response = await apiClient.get<ProductResponse[]>('/products');
    return response.data;
  },

  async getProductsByCategory(categoryId: string): Promise<ProductResponse[]> {
    const response = await apiClient.get<ProductResponse[]>(`/products/category/${categoryId}`);
    return response.data;
  },

  async getProductById(id: string): Promise<ProductResponse> {
    const response = await apiClient.get<ProductResponse>(`/products/${id}`);
    return response.data;
  },

  async createProduct(request: CreateProductRequest): Promise<ProductResponse> {
    const response = await apiClient.post<ProductResponse>('/products', request);
    return response.data;
  },

  async updateProduct(id: string, request: UpdateProductRequest): Promise<ProductResponse> {
    const response = await apiClient.put<ProductResponse>(`/products/${id}`, request);
    return response.data;
  },

  async deleteProduct(id: string): Promise<void> {
    await apiClient.delete(`/products/${id}`);
  },
};
