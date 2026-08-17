import { apiClient } from '@/api/client'
import type {
  AskKnowledgeBaseResult,
  IngestKnowledgeDocumentPayload,
  IngestKnowledgeDocumentResult,
  KnowledgeChunk,
} from '@/types/knowledgeBase'

export async function ingestKnowledgeDocument(
  payload: IngestKnowledgeDocumentPayload,
): Promise<IngestKnowledgeDocumentResult> {
  const { data } = await apiClient.post<IngestKnowledgeDocumentResult>('/knowledge-base/documents', payload)
  return data
}

export async function searchKnowledgeBase(query: string, topK = 5): Promise<KnowledgeChunk[]> {
  const { data } = await apiClient.get<KnowledgeChunk[]>('/knowledge-base/search', { params: { query, topK } })
  return data
}

export async function askKnowledgeBase(question: string, topK = 5): Promise<AskKnowledgeBaseResult> {
  const { data } = await apiClient.post<AskKnowledgeBaseResult>('/knowledge-base/ask', { question, topK })
  return data
}
