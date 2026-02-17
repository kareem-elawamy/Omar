export interface JwtPayload {
  role: string[];
  name: string;
  nameid: string;
  email: string;
  exp: number;
  [key: string]: any;
}
