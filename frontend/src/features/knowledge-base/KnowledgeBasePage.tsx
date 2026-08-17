import { useEffect, useRef, useState } from 'react'
import { BookPlus, Library } from 'lucide-react'
import { useKnowledgeBaseChat } from '@/features/knowledge-base/hooks/useKnowledgeBaseChat'
import { ChatMessageBubble } from '@/features/knowledge-base/components/ChatMessageBubble'
import { ChatInput } from '@/features/knowledge-base/components/ChatInput'
import { IngestArticlePanel } from '@/features/knowledge-base/components/IngestArticlePanel'
import { Button } from '@/components/ui/Button'

export function KnowledgeBasePage() {
  const { messages, isSending, sendQuestion } = useKnowledgeBaseChat()
  const [ingestOpen, setIngestOpen] = useState(false)
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  return (
    <div className="flex h-full flex-col">
      <div className="flex items-center justify-between px-6 pt-6 pb-4">
        <div>
          <h2 className="text-xl font-semibold text-white">Knowledge Base</h2>
          <p className="mt-1 text-sm text-neutral-400">Ask about analog technique, gear and darkroom process.</p>
        </div>
        <Button variant="secondary" onClick={() => setIngestOpen(true)}>
          <BookPlus size={16} />
          Add article
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto px-6">
        {messages.length === 0 && (
          <div className="flex h-full flex-col items-center justify-center gap-3 text-neutral-600">
            <Library size={40} />
            <p className="text-sm">Ask your first question about analog photography.</p>
          </div>
        )}

        <div className="flex flex-col gap-4 pb-4">
          {messages.map((message) => (
            <ChatMessageBubble key={message.id} message={message} />
          ))}
          <div ref={bottomRef} />
        </div>
      </div>

      <ChatInput onSend={sendQuestion} disabled={isSending} />

      <IngestArticlePanel open={ingestOpen} onClose={() => setIngestOpen(false)} />
    </div>
  )
}
