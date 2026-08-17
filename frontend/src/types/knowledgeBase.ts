export type KnowledgeSourceType = 'Manual' | 'Technique' | 'Guide' | 'Article' | 'Faq'

export interface KnowledgeChunk {
  id: string
  documentTitle: string
  sourceType: KnowledgeSourceType
  sourceUrl: string | null
  chunkIndex: number
  content: string
  similarityScore: number
}

export interface IngestKnowledgeDocumentPayload {
  documentTitle: string
  sourceType: KnowledgeSourceType
  sourceUrl?: string | null
  content: string
}

export interface IngestKnowledgeDocumentResult {
  documentTitle: string
  chunksCreated: number
}

export interface KnowledgeCitation {
  chunkId: string
  citationNumber: number
  documentTitle: string
  sourceUrl: string | null
  excerpt: string
}

export interface AskKnowledgeBaseResult {
  answer: string
  citations: KnowledgeCitation[]
}

export interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  content: string
  citations?: KnowledgeCitation[]
  pending?: boolean
}
