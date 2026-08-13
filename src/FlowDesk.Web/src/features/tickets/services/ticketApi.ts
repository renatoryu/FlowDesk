import { authenticatedApiRequest } from '../../../shared/api/apiClient'
import type {
  ListTicketsParams,
  ListTicketsResponse,
} from '../types/ticketTypes'

export function listTickets(
  accessToken: string,
  params: ListTicketsParams = {},
): Promise<ListTicketsResponse> {
  const searchParams = new URLSearchParams({
    page: String(params.page ?? 1),
    pageSize: String(params.pageSize ?? 10),
  })

  if (params.status !== undefined) {
    searchParams.set(
      'status',
      String(params.status),
    )
  }

  if (params.priority !== undefined) {
    searchParams.set(
      'priority',
      String(params.priority),
    )
  }

  if (params.categoryId) {
    searchParams.set(
      'categoryId',
      params.categoryId,
    )
  }

  return authenticatedApiRequest<ListTicketsResponse>(
    `/tickets?${searchParams.toString()}`,
    accessToken,
  )
}
