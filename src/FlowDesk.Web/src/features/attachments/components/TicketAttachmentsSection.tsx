import {
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query'
import {
  CircleAlert,
  Download,
  FileText,
  LoaderCircle,
  Paperclip,
  Upload,
} from 'lucide-react'
import {
  useRef,
  useState,
  type ChangeEvent,
  type SubmitEvent,
} from 'react'
import { ApiError } from '../../../shared/api/apiClient'
import { useAuth } from '../../auth/context/useAuth'
import type { TicketStatus } from '../../tickets/types/ticketTypes'
import {
  downloadTicketAttachment,
  listTicketAttachments,
  uploadTicketAttachment,
} from '../services/attachmentApi'
import styles from './TicketAttachmentsSection.module.css'

const maximumFileSize = 10 * 1024 * 1024

const allowedContentTypes = [
  'application/pdf',
  'image/png',
  'image/jpeg',
]

const allowedExtensionPattern = /\.(pdf|png|jpe?g)$/i

const dateFormatter = new Intl.DateTimeFormat('pt-BR', {
  dateStyle: 'short',
  timeStyle: 'short',
  timeZone: 'America/Sao_Paulo',
})

interface TicketAttachmentsSectionProps {
  ticketId: string
  ticketStatus: TicketStatus
}

function formatFileSize(sizeInBytes: number) {
  if (sizeInBytes < 1024) {
    return `${sizeInBytes} B`
  }

  if (sizeInBytes < 1024 * 1024) {
    return `${(sizeInBytes / 1024).toFixed(1)} KB`
  }

  return `${(
    sizeInBytes /
    (1024 * 1024)
  ).toFixed(1)} MB`
}

function TicketAttachmentsSection({
  ticketId,
  ticketStatus,
}: TicketAttachmentsSectionProps) {
  const { session } = useAuth()
  const queryClient = useQueryClient()
  const inputRef = useRef<HTMLInputElement>(null)

  const [selectedFile, setSelectedFile] =
    useState<File | null>(null)
  const [selectionError, setSelectionError] =
    useState<string | null>(null)
  const [downloadingId, setDownloadingId] =
    useState<string | null>(null)
  const [downloadError, setDownloadError] =
    useState<string | null>(null)

  const accessToken = session?.accessToken ?? ''

  const attachmentsQuery = useQuery({
    queryKey: ['tickets', 'attachments', ticketId],
    queryFn: () =>
      listTicketAttachments(accessToken, ticketId),
    enabled: accessToken.length > 0,
  })

  const uploadMutation = useMutation({
    mutationFn: (file: File) =>
      uploadTicketAttachment(
        accessToken,
        ticketId,
        file,
      ),
    onSuccess: () => {
      setSelectedFile(null)
      setSelectionError(null)

      if (inputRef.current) {
        inputRef.current.value = ''
      }

      void queryClient.invalidateQueries({
        queryKey: [
          'tickets',
          'attachments',
          ticketId,
        ],
      })
    },
  })

  if (!session) {
    return null
  }

  const attachments =
    attachmentsQuery.data?.items ?? []

  const isClosed = ticketStatus === 4

  function handleFileSelection(
    event: ChangeEvent<HTMLInputElement>,
  ) {
    const file = event.target.files?.[0]

    setSelectionError(null)
    uploadMutation.reset()

    if (!file) {
      setSelectedFile(null)
      return
    }

    if (
      !allowedExtensionPattern.test(file.name) ||
      !allowedContentTypes.includes(file.type)
    ) {
      setSelectedFile(null)
      setSelectionError(
        'Selecione um arquivo PDF, PNG, JPG ou JPEG.',
      )
      event.target.value = ''
      return
    }

    if (file.size > maximumFileSize) {
      setSelectedFile(null)
      setSelectionError(
        'O arquivo deve possuir no máximo 10 MB.',
      )
      event.target.value = ''
      return
    }

    setSelectedFile(file)
  }

  function handleUpload(
    event: SubmitEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    if (!selectedFile) {
      setSelectionError(
        'Selecione um arquivo para enviar.',
      )
      return
    }

    uploadMutation.mutate(selectedFile)
  }

  async function handleDownload(
    attachmentId: string,
    originalFileName: string,
  ) {
    setDownloadError(null)
    setDownloadingId(attachmentId)

    try {
      const blob = await downloadTicketAttachment(
        accessToken,
        ticketId,
        attachmentId,
      )

      const downloadUrl =
        URL.createObjectURL(blob)
      const anchor = document.createElement('a')

      anchor.href = downloadUrl
      anchor.download = originalFileName
      document.body.appendChild(anchor)
      anchor.click()
      anchor.remove()

      URL.revokeObjectURL(downloadUrl)
    } catch (error) {
      setDownloadError(
        error instanceof ApiError
          ? error.message
          : 'Não foi possível baixar o arquivo.',
      )
    } finally {
      setDownloadingId(null)
    }
  }

  const uploadError =
    uploadMutation.error instanceof ApiError
      ? uploadMutation.error.status === 409
        ? 'Este chamado não aceita novos anexos.'
        : uploadMutation.error.message
      : uploadMutation.error
        ? 'Não foi possível enviar o arquivo.'
        : null

  return (
    <section className={styles.section}>
      <header>
        <Paperclip aria-hidden="true" />

        <div>
          <h2>Anexos</h2>
          <span>{attachments.length} arquivo(s)</span>
        </div>
      </header>

      {attachmentsQuery.isPending && (
        <div className={styles.state}>
          <LoaderCircle
            className={styles.spinning}
            aria-hidden="true"
          />
          Carregando anexos...
        </div>
      )}

      {attachmentsQuery.isError && (
        <div className={styles.state}>
          <CircleAlert aria-hidden="true" />
          <span>
            Não foi possível carregar os anexos.
          </span>
          <button
            type="button"
            onClick={() => attachmentsQuery.refetch()}
          >
            Tentar novamente
          </button>
        </div>
      )}

      {!attachmentsQuery.isPending &&
        !attachmentsQuery.isError &&
        attachments.length === 0 && (
          <div className={styles.empty}>
            Nenhum arquivo anexado.
          </div>
        )}

      {attachments.length > 0 && (
        <div className={styles.fileList}>
          {attachments.map((attachment) => (
            <article key={attachment.id}>
              <span className={styles.fileIcon}>
                <FileText aria-hidden="true" />
              </span>

              <div className={styles.fileInfo}>
                <strong>
                  {attachment.originalFileName}
                </strong>
                <span>
                  {formatFileSize(
                    attachment.sizeInBytes,
                  )}
                  {' • '}
                  {dateFormatter.format(
                    new Date(
                      attachment.createdAtUtc,
                    ),
                  )}
                </span>
              </div>

              <button
                type="button"
                className={styles.downloadButton}
                disabled={
                  downloadingId === attachment.id
                }
                onClick={() =>
                  void handleDownload(
                    attachment.id,
                    attachment.originalFileName,
                  )
                }
                aria-label={`Baixar ${attachment.originalFileName}`}
              >
                {downloadingId === attachment.id ? (
                  <LoaderCircle
                    className={styles.spinning}
                    aria-hidden="true"
                  />
                ) : (
                  <Download aria-hidden="true" />
                )}
              </button>
            </article>
          ))}
        </div>
      )}

      {downloadError && (
        <div className={styles.apiError} role="alert">
          <CircleAlert aria-hidden="true" />
          {downloadError}
        </div>
      )}

      {isClosed ? (
        <div className={styles.closedNotice}>
          Este chamado está fechado e não aceita
          novos anexos.
        </div>
      ) : (
        <form
          className={styles.uploadForm}
          onSubmit={handleUpload}
        >
          <label htmlFor="ticket-attachment">
            <Upload aria-hidden="true" />
            Selecionar arquivo
          </label>

          <input
            ref={inputRef}
            id="ticket-attachment"
            type="file"
            accept=".pdf,.png,.jpg,.jpeg,application/pdf,image/png,image/jpeg"
            onChange={handleFileSelection}
          />

          <div className={styles.selection}>
            <span>
              {selectedFile
                ? `${selectedFile.name} · ${formatFileSize(
                  selectedFile.size,
                )}`
                : 'PDF, PNG, JPG ou JPEG · até 10 MB'}
            </span>

            <button
              type="submit"
              disabled={
                !selectedFile ||
                uploadMutation.isPending
              }
            >
              {uploadMutation.isPending ? (
                <LoaderCircle
                  className={styles.spinning}
                  aria-hidden="true"
                />
              ) : (
                <Upload aria-hidden="true" />
              )}

              {uploadMutation.isPending
                ? 'Enviando...'
                : 'Enviar anexo'}
            </button>
          </div>

          {(selectionError || uploadError) && (
            <div className={styles.apiError} role="alert">
              <CircleAlert aria-hidden="true" />
              {selectionError ?? uploadError}
            </div>
          )}
        </form>
      )}
    </section>
  )
}

export default TicketAttachmentsSection
