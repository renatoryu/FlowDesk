import { authenticatedApiRequest } from '../../../shared/api/apiClient'
import type {
  CreateCommentRequest,
  CreateCommentResponse,
  ListTicketCommentsResponse,
} from '../types/commentTypes'

export function listTicketComments(
  accessToken: string,
  ticketId: string,
): Promise<ListTicketCommentsResponse> {
  return authenticatedApiRequest<ListTicketCommentsResponse>(
    `/tickets/${ticketId}/comments`,
    accessToken,
  )
}

export function createTicketComment(
  accessToken: string,
  ticketId: string,
  request: CreateCommentRequest,
): Promise<CreateCommentResponse> {
  return authenticatedApiRequest<CreateCommentResponse>(
    `/tickets/${ticketId}/comments`,
    accessToken,
    {
      method: 'POST',
      body: JSON.stringify(request),
    },
  )
}
