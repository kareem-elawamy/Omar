import { Category } from "../Enum/category";

export interface InventoryStatus {
  InventoryValue: number;
  LowStockCount: number;
  LowStockItems: lowStockItems[];
}
export interface lowStockItems {
  Name: string;
  StockQuantity: number;
  Category: Category;
}
