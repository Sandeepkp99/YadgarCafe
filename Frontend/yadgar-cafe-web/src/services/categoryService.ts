import apiClient from './api';

export interface CategoryResponse {
  id: string;
  name: string;
  description?: string;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}

export interface UpdateCategoryRequest {
  name?: string;
  description?: string;
}

export const categoryService = {
  async getAllCategories(): Promise<CategoryResponse[]> {
    const response = await apiClient.get<CategoryResponse[]>('/categories');
    return response.data;
  },

  async getCategoryById(id: string): Promise<CategoryResponse> {
    const response = await apiClient.get<CategoryResponse>(`/categories/${id}`);
    return response.data;
  },

  async createCategory(request: CreateCategoryRequest): Promise<CategoryResponse> {
    const response = await apiClient.post<CategoryResponse>('/categories', request);
    return response.data;
  },

  async updateCategory(id: string, request: UpdateCategoryRequest): Promise<CategoryResponse> {
    const response = await apiClient.put<CategoryResponse>(`/categories/${id}`, request);
    return response.data;
  },

  async deleteCategory(id: string): Promise<void> {
    await apiClient.delete(`/categories/${id}`);
  },
};
