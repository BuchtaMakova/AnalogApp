import { useState } from 'react'
import { Star } from 'lucide-react'
import { cn } from '@/lib/cn'

interface StarRatingProps {
  value: number
  onChange?: (value: number) => void
  size?: number
  readOnly?: boolean
}

export function StarRating({ value, onChange, size = 16, readOnly = false }: StarRatingProps) {
  const [hovered, setHovered] = useState<number | null>(null)
  const display = hovered ?? value

  return (
    <div className="flex items-center gap-0.5" onMouseLeave={() => setHovered(null)}>
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          disabled={readOnly}
          onMouseEnter={() => !readOnly && setHovered(star)}
          onClick={() => onChange?.(star === value ? 0 : star)}
          className={cn('transition-colors', readOnly ? 'cursor-default' : 'cursor-pointer')}
          aria-label={`Rate ${star} stars`}
        >
          <Star
            size={size}
            className={display >= star ? 'fill-amber-400 text-amber-400' : 'fill-transparent text-neutral-600'}
          />
        </button>
      ))}
    </div>
  )
}
