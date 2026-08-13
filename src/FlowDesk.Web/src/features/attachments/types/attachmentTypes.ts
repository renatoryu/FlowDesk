export interface AttachmentListItem {
  id: string
  ticketId: string
  uploadedById: string
  originalFileName: string
  contentType: string
  sizeInBytes: number
  createdAtUtc: string
}

export interface ListTicketAttachmentsResponse {
  items: AttachmentListItem[]
}

export type UploadAttachmentResponse =
  AttachmentListItem
