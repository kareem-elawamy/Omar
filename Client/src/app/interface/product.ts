import { Category } from "../Enum/category";

export interface Product {
  name: string;
  price: number;
  quantity: number;
  category: Category;
  saleType: string;
  pricePerKg?: number;
  pricePerPiece?: number;
  stockQuantity: number;
}
