export interface CommentListItem {
  id: string
  ticketId: string
  authorId: string
  content: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface ListTicketCommentsResponse {
  items: CommentListItem[]
}

export interface CreateCommentRequest {
  content: string
}

export type CreateCommentResponse =
  CommentListItem
