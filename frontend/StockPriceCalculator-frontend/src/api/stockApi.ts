import axios from 'axios';

const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || '',
});

export interface CalculateSettlementRequest {
    symbol: string;
    quantity: number;
    date: string;
    market: string;
}

export interface CalculateSettlementResponse {
    symbol: string;
    tradeDate: string;
    shares: number;
    closePrice: number;
    totalAmount: number;
    hasPriceData: boolean;
}

export function calculateSettlement(payload: CalculateSettlementRequest) {
    return apiClient.post<CalculateSettlementResponse>('/api/stock/CalculateSettlement', payload);
}

export interface StockSearchResponse {
    stocks: Stock[];
}

export interface Stock {
    symbol: string;
    name: string;
    market: string;
}


export function searchStocks(keyword: string) {
    return apiClient.get<StockSearchResponse>(`/api/stock/search?keyword=${keyword}`);
}

