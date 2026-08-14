import {
  authenticatedApiRequest,
  authenticatedFileRequest,
} from '../../../shared/api/apiClient'
import type {
  ListTicketAttachmentsResponse,
  UploadAttachmentResponse,
} from '../types/attachmentTypes'

export function downloadTicketAttachment(
  accessToken: string,
  ticketId: string,
  attachmentId: string,
): Promise<Blob> {
  return authenticatedFileRequest(
    `/tickets/${ticketId}/attachments/${attachmentId}/download`,
    accessToken,
  )
}

export function listTicketAttachments(
  accessToken: string,
  ticketId: string,
): Promise<ListTicketAttachmentsResponse> {
  return authenticatedApiRequest<ListTicketAttachmentsResponse>(
    `/tickets/${ticketId}/attachments`,
    accessToken,
  )
}

export function uploadTicketAttachment(
  accessToken: string,
  ticketId: string,
  file: File,
): Promise<UploadAttachmentResponse> {
  const formData = new FormData()

  formData.append('file', file)

  return authenticatedApiRequest<UploadAttachmentResponse>(
    `/tickets/${ticketId}/attachments`,
    accessToken,
    {
      method: 'POST',
      body: formData,
    },
  )
}
