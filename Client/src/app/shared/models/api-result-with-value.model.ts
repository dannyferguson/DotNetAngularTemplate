export interface ApiResultWithValue<T> {
  isSuccess: boolean;
  successMessage: string;
  errorMessage: string;
  value: T | null;
}
