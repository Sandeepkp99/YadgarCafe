import apiClient from './api';

export interface IngredientResponse {
  id: string;
  name: string;
  description?: string;
  unit: string;
}

export interface CreateIngredientRequest {
  name: string;
  description?: string;
  unit: string;
}

export interface UpdateIngredientRequest {
  name?: string;
  description?: string;
  unit?: string;
}

export const ingredientService = {
  async getAllIngredients(): Promise<IngredientResponse[]> {
    const response = await apiClient.get<IngredientResponse[]>('/ingredients');
    return response.data;
  },

  async getIngredientById(id: string): Promise<IngredientResponse> {
    const response = await apiClient.get<IngredientResponse>(`/ingredients/${id}`);
    return response.data;
  },

  async getLowStockIngredients(): Promise<IngredientResponse[]> {
    const response = await apiClient.get<IngredientResponse[]>('/ingredients/low-stock');
    return response.data;
  },

  async createIngredient(request: CreateIngredientRequest): Promise<IngredientResponse> {
    const response = await apiClient.post<IngredientResponse>('/ingredients', request);
    return response.data;
  },

  async updateIngredient(id: string, request: UpdateIngredientRequest): Promise<IngredientResponse> {
    const response = await apiClient.put<IngredientResponse>(`/ingredients/${id}`, request);
    return response.data;
  },

  async deleteIngredient(id: string): Promise<void> {
    await apiClient.delete(`/ingredients/${id}`);
  },
};
