import { apiRequest } from '../../../shared/api/apiClient'
import type {
  LoginRequest,
  LoginResponse,
} from '../types/authTypes'

export function login(
  request: LoginRequest,
): Promise<LoginResponse> {
  return apiRequest<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
