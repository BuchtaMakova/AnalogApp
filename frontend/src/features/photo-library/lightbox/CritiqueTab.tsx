import type { ReactNode } from 'react'
import { Sparkles } from 'lucide-react'
import { useAnalyzePhoto } from '@/features/photo-library/hooks/usePhotoDetail'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge'
import { Spinner } from '@/components/ui/Spinner'
import { formatDateTime } from '@/lib/format'
import type { PhotoDetail } from '@/types/photo'

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div>
      <h3 className="mb-1 text-xs font-semibold uppercase tracking-wide text-neutral-500">{title}</h3>
      <p className="text-sm leading-relaxed text-neutral-200">{children}</p>
    </div>
  )
}

export function CritiqueTab({ photo }: { photo: PhotoDetail }) {
  const analyze = useAnalyzePhoto(photo.id)
  const critique = photo.critique

  return (
    <div className="flex flex-col gap-4">
      <Button
        variant="primary"
        onClick={() => analyze.mutate()}
        disabled={analyze.isPending}
        className="self-start"
      >
        {analyze.isPending ? <Spinner size={14} /> : <Sparkles size={14} />}
        {critique ? 'Re-run analysis' : 'Run AI analysis'}
      </Button>

      {analyze.isError && <p className="text-sm text-red-400">Analysis failed. Try again.</p>}

      {!critique && !analyze.isPending && (
        <p className="text-sm text-neutral-500">No critique yet — run the AI vision analysis to get feedback.</p>
      )}

      {critique && (
        <div className="flex flex-col gap-4">
          <div className="flex items-center gap-3">
            <div className="flex h-14 w-14 shrink-0 flex-col items-center justify-center rounded-full border-2 border-violet-400/60 text-violet-300">
              <span className="text-lg font-bold leading-none">{critique.compositionScore}</span>
              <span className="text-[9px] leading-none">/10</span>
            </div>
            <div className="text-xs text-neutral-500">
              <p>{critique.model}</p>
              <p>{formatDateTime(critique.createdAtUtc)}</p>
            </div>
          </div>

          <Section title="Composition">{critique.compositionNotes}</Section>
          <Section title="Light & flash">{critique.lightingNotes}</Section>
          {critique.posingNotes && <Section title="Posing">{critique.posingNotes}</Section>}

          {critique.recommendations.length > 0 && (
            <div>
              <h3 className="mb-1.5 text-xs font-semibold uppercase tracking-wide text-neutral-500">
                Tips for next time
              </h3>
              <ul className="flex flex-col gap-1.5">
                {critique.recommendations.map((tip, i) => (
                  <li key={i} className="flex gap-2 text-sm text-neutral-200">
                    <span className="text-violet-400">•</span>
                    {tip}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {critique.suggestedTags.length > 0 && (
            <div>
              <h3 className="mb-1.5 text-xs font-semibold uppercase tracking-wide text-neutral-500">
                Suggested tags
              </h3>
              <div className="flex flex-wrap gap-1.5">
                {critique.suggestedTags.map((tag) => (
                  <Badge key={tag} tone="ai">
                    {tag}
                  </Badge>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
