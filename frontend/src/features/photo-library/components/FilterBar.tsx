import { Star, X } from 'lucide-react'
import { useLibraryFilterOptions } from '@/features/photo-library/hooks/usePhotos'
import { Select } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import type { GetPhotosFilters } from '@/types/photo'

interface FilterBarProps {
  filters: GetPhotosFilters
  onChange: (filters: GetPhotosFilters) => void
}

export function FilterBar({ filters, onChange }: FilterBarProps) {
  const { filmRolls, cameraBodies, lenses, tags } = useLibraryFilterOptions()

  const hasActiveFilters = Object.values(filters).some((v) => v !== undefined && v !== '')

  return (
    <div className="flex flex-wrap items-center gap-2 border-b border-neutral-800 px-4 py-3">
      <Select
        value={filters.filmRollId ?? ''}
        onChange={(e) => onChange({ ...filters, filmRollId: e.target.value || undefined })}
        className="w-auto"
      >
        <option value="">All rolls</option>
        {filmRolls.data?.map((roll) => (
          <option key={roll.id} value={roll.id}>
            {roll.name}
          </option>
        ))}
      </Select>

      <Select
        value={filters.cameraBodyId ?? ''}
        onChange={(e) => onChange({ ...filters, cameraBodyId: e.target.value || undefined })}
        className="w-auto"
      >
        <option value="">All cameras</option>
        {cameraBodies.data?.map((body) => (
          <option key={body.id} value={body.id}>
            {body.brand} {body.model}
          </option>
        ))}
      </Select>

      <Select
        value={filters.lensId ?? ''}
        onChange={(e) => onChange({ ...filters, lensId: e.target.value || undefined })}
        className="w-auto"
      >
        <option value="">All lenses</option>
        {lenses.data?.map((lens) => (
          <option key={lens.id} value={lens.id}>
            {lens.brand} {lens.model}
          </option>
        ))}
      </Select>

      <Select
        value={filters.tag ?? ''}
        onChange={(e) => onChange({ ...filters, tag: e.target.value || undefined })}
        className="w-auto"
      >
        <option value="">All tags</option>
        {tags.data?.map((tag) => (
          <option key={tag.id} value={tag.slug}>
            {tag.name}
          </option>
        ))}
      </Select>

      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            key={star}
            type="button"
            onClick={() => onChange({ ...filters, minRating: filters.minRating === star ? undefined : star })}
            className="rounded p-0.5 hover:bg-neutral-800"
            aria-label={`Minimum rating ${star}`}
          >
            <Star
              size={16}
              className={
                (filters.minRating ?? 0) >= star ? 'fill-amber-400 text-amber-400' : 'fill-transparent text-neutral-600'
              }
            />
          </button>
        ))}
      </div>

      {hasActiveFilters && (
        <Button variant="ghost" size="sm" onClick={() => onChange({})}>
          <X size={14} />
          Clear
        </Button>
      )}
    </div>
  )
}
