import { authenticatedApiRequest } from '../../../shared/api/apiClient'
import type { CategoryListItem } from '../types/categoryTypes'

export function listCategories(
  accessToken: string,
): Promise<CategoryListItem[]> {
  return authenticatedApiRequest<CategoryListItem[]>(
    '/categories',
    accessToken,
  )
}
