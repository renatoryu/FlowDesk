import { authenticatedApiRequest } from '../../../shared/api/apiClient'
import type {
  ListTicketsParams,
  ListTicketsResponse,
  CreateTicketRequest,
  CreateTicketResponse,
  TicketDetails,
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

export function createTicket(
  accessToken: string,
  request: CreateTicketRequest,
): Promise<CreateTicketResponse> {
  return authenticatedApiRequest<CreateTicketResponse>(
    '/tickets',
    accessToken,
    {
      method: 'POST',
      body: JSON.stringify(request),
    },
  )
}

export function getTicketById(
  accessToken: string,
  ticketId: string,
): Promise<TicketDetails> {
  return authenticatedApiRequest<TicketDetails>(
    `/tickets/${ticketId}`,
    accessToken,
  )
}
