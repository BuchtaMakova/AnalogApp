import { useState } from 'react'
import { Plus, Trash2, X } from 'lucide-react'
import {
  useDeletePhoto,
  usePhotoTagMutations,
  useUpdatePhotoRating,
} from '@/features/photo-library/hooks/usePhotoDetail'
import { useAlbumMembership, useAlbums } from '@/features/photo-library/hooks/useAlbumMembership'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createAlbum } from '@/api/albums'
import { StarRating } from '@/components/ui/StarRating'
import { Badge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import type { PhotoDetail } from '@/types/photo'

export function OrganizeTab({ photo, onDeleted }: { photo: PhotoDetail; onDeleted: () => void }) {
  const updateRating = useUpdatePhotoRating(photo.id)
  const { addTag, removeTag } = usePhotoTagMutations(photo.id)
  const { addToAlbum, removeFromAlbum } = useAlbumMembership(photo.id)
  const albums = useAlbums()
  const queryClient = useQueryClient()
  const deletePhoto = useDeletePhoto(photo.id)

  const [newTag, setNewTag] = useState('')
  const [newAlbumName, setNewAlbumName] = useState('')

  const createAlbumMutation = useMutation({
    mutationFn: () => createAlbum({ name: newAlbumName }),
    onSuccess: () => {
      setNewAlbumName('')
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })

  const submitNewTag = () => {
    const trimmed = newTag.trim()
    if (!trimmed) return
    addTag.mutate(trimmed)
    setNewTag('')
  }

  const albumIds = new Set(photo.albumIds)

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-neutral-500">Rating</h3>
        <StarRating value={photo.rating} onChange={(v) => updateRating.mutate(v)} size={20} />
      </div>

      <div>
        <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-neutral-500">Tags</h3>
        <div className="mb-2 flex flex-wrap gap-1.5">
          {photo.tags.map((tag) => (
            <Badge key={tag.tagId} tone={tag.isAiSuggested ? 'ai' : 'neutral'} className="gap-1 pr-1">
              {tag.name}
              <button
                type="button"
                onClick={() => removeTag.mutate(tag.tagId)}
                className="rounded-full p-0.5 hover:bg-black/20"
              >
                <X size={10} />
              </button>
            </Badge>
          ))}
          {photo.tags.length === 0 && <p className="text-sm text-neutral-500">No tags yet.</p>}
        </div>
        <div className="flex gap-2">
          <Input
            value={newTag}
            onChange={(e) => setNewTag(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && submitNewTag()}
            placeholder="Add a tag…"
          />
          <Button variant="secondary" onClick={submitNewTag} disabled={addTag.isPending}>
            <Plus size={14} />
          </Button>
        </div>
      </div>

      <div>
        <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-neutral-500">Albums</h3>
        <div className="flex flex-col gap-1.5">
          {albums.data?.map((album) => {
            const isMember = albumIds.has(album.id)
            return (
              <label
                key={album.id}
                className="flex cursor-pointer items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-neutral-200 hover:bg-neutral-800"
              >
                <input
                  type="checkbox"
                  checked={isMember}
                  onChange={() =>
                    isMember ? removeFromAlbum.mutate(album.id) : addToAlbum.mutate(album.id)
                  }
                  className="accent-white"
                />
                {album.name}
                <span className="ml-auto text-xs text-neutral-500">{album.photoCount}</span>
              </label>
            )
          })}
          {albums.data?.length === 0 && <p className="text-sm text-neutral-500">No albums yet.</p>}
        </div>
        <div className="mt-2 flex gap-2">
          <Input
            value={newAlbumName}
            onChange={(e) => setNewAlbumName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && newAlbumName.trim() && createAlbumMutation.mutate()}
            placeholder="New album name…"
          />
          <Button
            variant="secondary"
            onClick={() => createAlbumMutation.mutate()}
            disabled={!newAlbumName.trim() || createAlbumMutation.isPending}
          >
            <Plus size={14} />
          </Button>
        </div>
      </div>

      <div className="border-t border-neutral-800 pt-4">
        <h3 className="mb-2 text-xs font-semibold uppercase tracking-wide text-neutral-500">Danger zone</h3>
        <Button
          variant="danger"
          onClick={() =>
            confirm('Delete this photo? This removes the original, preview and thumbnail permanently.') &&
            deletePhoto.mutate(undefined, { onSuccess: onDeleted })
          }
          disabled={deletePhoto.isPending}
        >
          <Trash2 size={14} />
          Delete photo
        </Button>
        {deletePhoto.isError && <p className="mt-2 text-sm text-red-400">Couldn't delete this photo. Try again.</p>}
      </div>
    </div>
  )
}
