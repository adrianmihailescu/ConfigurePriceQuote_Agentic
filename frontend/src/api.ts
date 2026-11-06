import type { ChatResponse, Product, QuoteSummary } from './types'

const apiBaseUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:5050').replace(/\/$/, '')

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    const body = await response.json().catch(() => undefined) as
      | { error?: string; title?: string; errors?: Record<string, string[]> }
      | undefined
    const details = body?.errors ? Object.values(body.errors).flat().join(' ') : undefined
    throw new Error(details || body?.error || body?.title || `Request failed (${response.status}).`)
  }

  return response.json() as Promise<T>
}

export const api = {
  catalog: () => request<Product[]>('/api/catalog'),
  quotes: () => request<QuoteSummary[]>('/api/quotes'),
  chat: (productId: string, selectedOptions: Record<string, string>, message: string) =>
    request<ChatResponse>('/api/assistant/chat', {
      method: 'POST',
      body: JSON.stringify({ productId, selectedOptions, message }),
    }),
  createQuote: (payload: {
    customerName: string
    customerEmail: string
    productId: string
    selectedOptions: Record<string, string>
    notes: string
  }) =>
    request<QuoteSummary>('/api/quotes', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
}
