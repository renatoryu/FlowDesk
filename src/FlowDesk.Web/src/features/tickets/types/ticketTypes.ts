export type TicketStatus = 1 | 2 | 3 | 4
export type TicketPriority = 1 | 2 | 3 | 4

export interface TicketListItem {
  id: string
  companyId: string
  categoryId: string
  requesterId: string
  title: string
  priority: TicketPriority
  status: TicketStatus
  createdAtUtc: string
  updatedAtUtc: string
  statusChangedAtUtc: string
}

export interface ListTicketsResponse {
  items: TicketListItem[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ListTicketsParams {
  page?: number
  pageSize?: number
  status?: TicketStatus
  priority?: TicketPriority
  categoryId?: string
}
