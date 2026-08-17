import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ingestKnowledgeDocument } from '@/api/knowledgeBase'
import { Dialog } from '@/components/ui/Dialog'
import { FormField, Input, Select, Textarea } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import type { KnowledgeSourceType } from '@/types/knowledgeBase'

const SOURCE_TYPES: KnowledgeSourceType[] = ['Manual', 'Technique', 'Guide', 'Article', 'Faq']

export function IngestArticlePanel({ open, onClose }: { open: boolean; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [documentTitle, setDocumentTitle] = useState('')
  const [sourceType, setSourceType] = useState<KnowledgeSourceType>('Technique')
  const [sourceUrl, setSourceUrl] = useState('')
  const [content, setContent] = useState('')

  const ingest = useMutation({
    mutationFn: () =>
      ingestKnowledgeDocument({ documentTitle, sourceType, sourceUrl: sourceUrl || null, content }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['knowledge-base'] })
      setDocumentTitle('')
      setSourceUrl('')
      setContent('')
      onClose()
    },
  })

  return (
    <Dialog open={open} onClose={onClose} title="Add knowledge base article" widthClassName="max-w-lg">
      <form
        className="flex flex-col gap-3"
        onSubmit={(e) => {
          e.preventDefault()
          ingest.mutate()
        }}
      >
        <FormField label="Title">
          <Input value={documentTitle} onChange={(e) => setDocumentTitle(e.target.value)} required />
        </FormField>

        <FormField label="Source type">
          <Select value={sourceType} onChange={(e) => setSourceType(e.target.value as KnowledgeSourceType)}>
            {SOURCE_TYPES.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </Select>
        </FormField>

        <FormField label="Source URL (optional)">
          <Input value={sourceUrl} onChange={(e) => setSourceUrl(e.target.value)} placeholder="https://…" />
        </FormField>

        <FormField label="Content">
          <Textarea
            value={content}
            onChange={(e) => setContent(e.target.value)}
            required
            minLength={50}
            className="min-h-40"
            placeholder="Paste the article text — it'll be chunked and embedded automatically."
          />
        </FormField>

        {ingest.isError && <p className="text-sm text-red-400">Failed to ingest — check the content length.</p>}

        <div className="flex justify-end gap-2 pt-1">
          <Button type="button" variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" variant="primary" disabled={ingest.isPending}>
            {ingest.isPending && <Spinner size={14} />}
            Ingest
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
