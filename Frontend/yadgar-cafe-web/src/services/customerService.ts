import apiClient from './api';

export interface CustomerResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  address?: string;
  city?: string;
  zipCode?: string;
}

export interface CreateCustomerRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  address?: string;
  city?: string;
  zipCode?: string;
}

export interface UpdateCustomerRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  zipCode?: string;
}

export const customerService = {
  async getAllCustomers(): Promise<CustomerResponse[]> {
    const response = await apiClient.get<CustomerResponse[]>('/customers');
    return response.data;
  },

  async getCustomerById(id: string): Promise<CustomerResponse> {
    const response = await apiClient.get<CustomerResponse>(`/customers/${id}`);
    return response.data;
  },

  async createCustomer(request: CreateCustomerRequest): Promise<CustomerResponse> {
    const response = await apiClient.post<CustomerResponse>('/customers', request);
    return response.data;
  },

  async updateCustomer(id: string, request: UpdateCustomerRequest): Promise<CustomerResponse> {
    const response = await apiClient.put<CustomerResponse>(`/customers/${id}`, request);
    return response.data;
  },

  async deleteCustomer(id: string): Promise<void> {
    await apiClient.delete(`/customers/${id}`);
  },
};
