import type { ApiProblem } from '../types/api'

const ADMIN_KEY_STORAGE = 'djmrkos.adminApiKey'

export class ApiError extends Error {
  status: number
  problem: ApiProblem

  constructor(status: number, problem: ApiProblem) {
    super(problem.title)
    this.status = status
    this.problem = problem
  }
}

export function getAdminApiKey(): string | null {
  try {
    return sessionStorage.getItem(ADMIN_KEY_STORAGE)
  } catch {
    return null
  }
}

export function setAdminApiKey(key: string | null) {
  try {
    if (key) sessionStorage.setItem(ADMIN_KEY_STORAGE, key)
    else sessionStorage.removeItem(ADMIN_KEY_STORAGE)
  } catch {
    // Private browsing or blocked storage — the session just won't survive a refresh.
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  admin?: boolean
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = {}
  if (options.body !== undefined) headers['Content-Type'] = 'application/json'

  if (options.admin) {
    const key = getAdminApiKey()
    if (key) headers['X-Api-Key'] = key
  }

  const response = await fetch(`/api${path}`, {
    method: options.method ?? 'GET',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  })

  if (response.status === 204) return undefined as T

  const isJson = response.headers.get('content-type')?.includes('json')
  const payload = isJson ? await response.json() : undefined

  if (!response.ok) {
    throw new ApiError(response.status, payload ?? { title: response.statusText, status: response.status })
  }

  return payload as T
}

export const api = {
  get: <T>(path: string, admin = false) => request<T>(path, { admin }),
  post: <T>(path: string, body?: unknown, admin = false) => request<T>(path, { method: 'POST', body, admin }),
  put: <T>(path: string, body?: unknown, admin = false) => request<T>(path, { method: 'PUT', body, admin }),
  patch: <T>(path: string, body?: unknown, admin = false) => request<T>(path, { method: 'PATCH', body, admin }),
  del: <T>(path: string, admin = false) => request<T>(path, { method: 'DELETE', admin }),
}
