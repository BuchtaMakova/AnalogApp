import { type PropsWithChildren, useEffect } from 'react'
import { X } from 'lucide-react'
import { cn } from '@/lib/cn'

interface DialogProps extends PropsWithChildren {
  open: boolean
  onClose: () => void
  title?: string
  widthClassName?: string
}

export function Dialog({ open, onClose, title, widthClassName = 'max-w-lg', children }: DialogProps) {
  useEffect(() => {
    if (!open) return
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [open, onClose])

  if (!open) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4" onClick={onClose}>
      <div
        className={cn('w-full rounded-xl border border-neutral-800 bg-neutral-900 shadow-2xl', widthClassName)}
        onClick={(e) => e.stopPropagation()}
      >
        {title && (
          <div className="flex items-center justify-between border-b border-neutral-800 px-5 py-4">
            <h2 className="text-sm font-semibold text-white">{title}</h2>
            <button
              type="button"
              onClick={onClose}
              className="rounded-md p-1 text-neutral-400 hover:bg-neutral-800 hover:text-white"
            >
              <X size={16} />
            </button>
          </div>
        )}
        <div className="p-5">{children}</div>
      </div>
    </div>
  )
}
