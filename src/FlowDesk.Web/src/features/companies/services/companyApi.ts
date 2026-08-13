import { authenticatedApiRequest } from '../../../shared/api/apiClient'
import type { CompanyListItem } from '../types/companyTypes'

export function listCompanies(
  accessToken: string,
  includeInactive: boolean,
): Promise<CompanyListItem[]> {
  const searchParams = new URLSearchParams({
    includeInactive: String(includeInactive),
  })

  return authenticatedApiRequest<CompanyListItem[]>(
    `/companies?${searchParams.toString()}`,
    accessToken,
  )
}
