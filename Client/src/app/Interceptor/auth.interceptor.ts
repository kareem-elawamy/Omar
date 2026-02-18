import { HttpInterceptorFn } from '@angular/common/http';

// auth.interceptor.ts
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // غيرنا 'token' لـ 'auth_token' عشان تطابق الـ Service
  const token = localStorage.getItem('auth_token');

  if (token) {
    console.log('Interceptor found token:', token);
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }

    });
    console.log('Interceptor added token to request:', cloned);
    return next(cloned);
  }
  return next(req);
};
