import apiClient from './api';

export interface InventoryResponse {
  id: string;
  ingredientId: string;
  ingredientName?: string;
  quantity: number;
  minStock: number;
  maxStock: number;
  unit: string;
  lastRestocked: string;
}

export interface CreateInventoryRequest {
  ingredientId: string;
  quantity: number;
  minStock: number;
  maxStock: number;
  unit: string;
}

export interface UpdateInventoryRequest {
  quantity?: number;
  minStock?: number;
  maxStock?: number;
}

export interface InventoryHistoryResponse {
  id: string;
  inventoryId: string;
  previousQuantity: number;
  newQuantity: number;
  changeReason: string;
  changedAt: string;
}

export const inventoryService = {
  async getAllInventory(): Promise<InventoryResponse[]> {
    const response = await apiClient.get<InventoryResponse[]>('/inventories');
    return response.data;
  },

  async getInventoryById(id: string): Promise<InventoryResponse> {
    const response = await apiClient.get<InventoryResponse>(`/inventories/${id}`);
    return response.data;
  },

  async getLowStockItems(): Promise<InventoryResponse[]> {
    const response = await apiClient.get<InventoryResponse[]>('/inventories/low-stock');
    return response.data;
  },

  async getInventoryHistory(inventoryId: string): Promise<InventoryHistoryResponse[]> {
    const response = await apiClient.get<InventoryHistoryResponse[]>(`/inventories/${inventoryId}/history`);
    return response.data;
  },

  async createInventory(request: CreateInventoryRequest): Promise<InventoryResponse> {
    const response = await apiClient.post<InventoryResponse>('/inventories', request);
    return response.data;
  },

  async updateInventory(id: string, request: UpdateInventoryRequest): Promise<InventoryResponse> {
    const response = await apiClient.put<InventoryResponse>(`/inventories/${id}`, request);
    return response.data;
  },

  async deleteInventory(id: string): Promise<void> {
    await apiClient.delete(`/inventories/${id}`);
  },
};
