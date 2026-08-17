import { Pencil, Trash2 } from 'lucide-react'
import { Badge } from '@/components/ui/Badge'

interface GearRowProps {
  title: string
  subtitle?: string | null
  meta?: string[]
  isActive: boolean
  onEdit: () => void
  onDelete: () => void
}

export function GearRow({ title, subtitle, meta, isActive, onEdit, onDelete }: GearRowProps) {
  return (
    <div className="flex items-center justify-between gap-4 rounded-lg border border-neutral-800 px-4 py-3">
      <div>
        <div className="flex items-center gap-2">
          <p className="text-sm font-medium text-white">{title}</p>
          {!isActive && <Badge tone="warning">Inactive</Badge>}
        </div>
        {subtitle && <p className="mt-0.5 text-xs text-neutral-500">{subtitle}</p>}
        {meta && meta.length > 0 && (
          <p className="mt-1 text-xs text-neutral-600">{meta.join(' · ')}</p>
        )}
      </div>
      <div className="flex shrink-0 gap-1">
        <button
          type="button"
          onClick={onEdit}
          className="rounded-md p-1.5 text-neutral-400 hover:bg-neutral-800 hover:text-white"
          aria-label="Edit"
        >
          <Pencil size={14} />
        </button>
        <button
          type="button"
          onClick={onDelete}
          className="rounded-md p-1.5 text-neutral-400 hover:bg-red-950 hover:text-red-400"
          aria-label="Delete"
        >
          <Trash2 size={14} />
        </button>
      </div>
    </div>
  )
}
