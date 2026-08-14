import { authenticatedApiRequest } from '../../../shared/api/apiClient'
import type { DashboardSummary } from '../types/dashboardTypes'

export function getDashboardSummary(
  accessToken: string,
): Promise<DashboardSummary> {
  return authenticatedApiRequest<DashboardSummary>(
    '/dashboard/summary',
    accessToken,
  )
}
