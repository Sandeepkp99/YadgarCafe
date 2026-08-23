import apiClient from './api';

export interface DashboardStatistics {
  todaysSales: number;
  monthlySales: number;
  totalOrders: number;
  lowStockCount: number;
  timestamp: string;
}

export const dashboardService = {
  async getStatistics(): Promise<DashboardStatistics> {
    const response = await apiClient.get<DashboardStatistics>('/dashboard/statistics');
    return response.data;
  },
};
