export interface ApiProblem {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly problem: ApiProblem

  constructor(
    message: string,
    status: number,
    problem: ApiProblem = {},
  ) {
    super(message)

    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

const apiUrl = (
  import.meta.env.VITE_API_URL ?? '/api'
).replace(/\/$/, '')

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers)

  if (options.body && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  headers.set('Accept', 'application/json')

  const response = await fetch(`${apiUrl}${path}`, {
    ...options,
    headers,
  })

  const contentType = response.headers.get('content-type')
  const hasJson =
    contentType?.includes('application/json') === true

  const payload = hasJson
    ? await response.json()
    : null

  if (!response.ok) {
    const problem = (payload ?? {}) as ApiProblem

    throw new ApiError(
      problem.detail ??
      problem.title ??
      'Não foi possível concluir a solicitação.',
      response.status,
      problem,
    )
  }

  return payload as T
}

export function authenticatedApiRequest<T>(
  path: string,
  accessToken: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers)

  headers.set(
    'Authorization',
    `Bearer ${accessToken}`,
  )

  return apiRequest<T>(path, {
    ...options,
    headers,
  })
}

export async function authenticatedFileRequest(
  path: string,
  accessToken: string,
): Promise<Blob> {
  const response = await fetch(`${apiUrl}${path}`, {
    headers: {
      Accept: '*/*',
      Authorization: `Bearer ${accessToken}`,
    },
  })

  if (!response.ok) {
    const contentType =
      response.headers.get('content-type')

    const hasJson =
      contentType?.includes(
        'application/json',
      ) === true

    const problem = hasJson
      ? await response.json() as ApiProblem
      : {}

    throw new ApiError(
      problem.detail ??
      problem.title ??
      'Não foi possível baixar o arquivo.',
      response.status,
      problem,
    )
  }

  return response.blob()
}
