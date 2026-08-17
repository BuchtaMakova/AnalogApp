import { BookText } from 'lucide-react'
import { Spinner } from '@/components/ui/Spinner'
import { cn } from '@/lib/cn'
import type { ChatMessage } from '@/types/knowledgeBase'

export function ChatMessageBubble({ message }: { message: ChatMessage }) {
  const isUser = message.role === 'user'

  return (
    <div className={cn('flex flex-col gap-2', isUser ? 'items-end' : 'items-start')}>
      <div
        className={cn(
          'max-w-[85%] rounded-2xl px-4 py-2.5 text-sm leading-relaxed whitespace-pre-wrap',
          isUser ? 'bg-white text-neutral-900' : 'bg-neutral-800 text-neutral-100',
        )}
      >
        {message.pending ? <Spinner size={16} /> : message.content}
      </div>

      {!isUser && message.citations && message.citations.length > 0 && (
        <div className="flex max-w-[85%] flex-col gap-1.5">
          {message.citations.map((citation) => (
            <div
              key={citation.chunkId}
              className="flex items-start gap-2 rounded-lg border border-neutral-800 bg-neutral-900/60 px-3 py-2 text-xs"
            >
              <BookText size={13} className="mt-0.5 shrink-0 text-neutral-500" />
              <div>
                <p className="font-medium text-neutral-300">
                  [{citation.citationNumber}] {citation.documentTitle}
                </p>
                <p className="mt-0.5 text-neutral-500">{citation.excerpt}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
