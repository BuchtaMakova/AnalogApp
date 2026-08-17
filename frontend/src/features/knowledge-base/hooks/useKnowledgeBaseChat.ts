import { useState } from 'react'
import { askKnowledgeBase } from '@/api/knowledgeBase'
import type { ChatMessage } from '@/types/knowledgeBase'

export function useKnowledgeBaseChat() {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [isSending, setIsSending] = useState(false)

  const sendQuestion = async (question: string) => {
    const trimmed = question.trim()
    if (!trimmed || isSending) return

    const userMessage: ChatMessage = { id: crypto.randomUUID(), role: 'user', content: trimmed }
    const pendingId = crypto.randomUUID()
    const pendingMessage: ChatMessage = { id: pendingId, role: 'assistant', content: '', pending: true }

    setMessages((prev) => [...prev, userMessage, pendingMessage])
    setIsSending(true)

    try {
      const result = await askKnowledgeBase(trimmed)
      setMessages((prev) =>
        prev.map((m) =>
          m.id === pendingId ? { ...m, content: result.answer, citations: result.citations, pending: false } : m,
        ),
      )
    } catch {
      setMessages((prev) =>
        prev.map((m) =>
          m.id === pendingId
            ? { ...m, content: 'Something went wrong answering that — try again.', pending: false }
            : m,
        ),
      )
    } finally {
      setIsSending(false)
    }
  }

  return { messages, isSending, sendQuestion }
}
