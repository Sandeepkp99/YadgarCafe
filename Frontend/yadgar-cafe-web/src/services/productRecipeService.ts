import apiClient from './api';

export interface ProductRecipeResponse {
  id: string;
  productId: string;
  ingredientId: string;
  quantity: number;
  unit: string;
  productName?: string;
  ingredientName?: string;
}

export interface CreateProductRecipeRequest {
  productId: string;
  ingredientId: string;
  quantity: number;
  unit: string;
}

export interface UpdateProductRecipeRequest {
  quantity?: number;
  unit?: string;
}

export interface ProductWithRecipeResponse {
  productId: string;
  productName: string;
  recipes: ProductRecipeResponse[];
}

export const productRecipeService = {
  async getAllRecipes(): Promise<ProductRecipeResponse[]> {
    const response = await apiClient.get<ProductRecipeResponse[]>('/productrecipes');
    return response.data;
  },

  async getRecipeById(id: string): Promise<ProductRecipeResponse> {
    const response = await apiClient.get<ProductRecipeResponse>(`/productrecipes/${id}`);
    return response.data;
  },

  async getProductRecipes(productId: string): Promise<ProductWithRecipeResponse> {
    const response = await apiClient.get<ProductWithRecipeResponse>(`/productrecipes/product/${productId}`);
    return response.data;
  },

  async createRecipe(request: CreateProductRecipeRequest): Promise<ProductRecipeResponse> {
    const response = await apiClient.post<ProductRecipeResponse>('/productrecipes', request);
    return response.data;
  },

  async updateRecipe(id: string, request: UpdateProductRecipeRequest): Promise<ProductRecipeResponse> {
    const response = await apiClient.put<ProductRecipeResponse>(`/productrecipes/${id}`, request);
    return response.data;
  },

  async deleteRecipe(id: string): Promise<void> {
    await apiClient.delete(`/productrecipes/${id}`);
  },
};
