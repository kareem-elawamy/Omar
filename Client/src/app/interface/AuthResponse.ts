// interface/AuthResponse.ts
export interface AuthResponse {
  isSuccess: boolean;
  message: string;
  token: string; // تأكد إنها token مش tokens
}
