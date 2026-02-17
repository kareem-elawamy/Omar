import { Category } from '../Enum/category';
import { SaleType } from '../Enum/sale-type';

export interface Product {
  id?: number;
  name: string;
  barcode?: string | null;
  category: Category;
  saleType: SaleType;
  buyingPrice: number;
  pricePerKg?: number;
  pricePerPiece?: number;
  stockQuantity: number;
  isActive?: boolean;
}
