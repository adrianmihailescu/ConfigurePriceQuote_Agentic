export type AgentDefinition = {
  fileName: string
  name: string
  description: string
  delegatedAgents: string[]
}

export type ProductChoice = {
  id: string
  label: string
  priceDelta: number
}

export type ProductOptionGroup = {
  key: string
  label: string
  required: boolean
  choices: ProductChoice[]
}

export type Product = {
  id: string
  name: string
  category: string
  description: string
  basePrice: number
  imageUrl: string
  options: ProductOptionGroup[]
}

export type AgentStep = {
  agent: string
  status: string
  summary: string
}

export type ChatResponse = {
  reply: string
  isValid: boolean
  subtotal: number
  discount: number
  total: number
  issues: string[]
  agentSteps: AgentStep[]
}

export type QuoteSummary = {
  id: number
  quoteNumber: string
  customerName: string
  customerEmail: string
  productName: string
  total: number
  status: string
  createdAt: string
}

export type SalesOverview = {
  quotesThisMonth: number
  quotesLastMonth: number
  totalQuotes: number
  openQuotes: number
  pipelineValue: number
  winRate: number | null
  decidedQuotes: number
}
