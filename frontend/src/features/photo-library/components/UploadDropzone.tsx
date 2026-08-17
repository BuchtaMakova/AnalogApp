import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useRef, useState, type DragEvent } from 'react'
import { CheckCircle2, ImageUp, UploadCloud, XCircle } from 'lucide-react'
import { uploadAndRegisterPhoto } from '@/api/photos'
import { listFilmRolls } from '@/api/gear'
import { Dialog } from '@/components/ui/Dialog'
import { Select } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import { cn } from '@/lib/cn'

interface UploadJob {
  id: string
  fileName: string
  progress: number
  status: 'uploading' | 'done' | 'error'
  error?: string
}

interface UploadDropzoneProps {
  open: boolean
  onClose: () => void
}

const ACCEPTED_TYPES = ['image/jpeg', 'image/png', 'image/tiff', 'image/webp']

export function UploadDropzone({ open, onClose }: UploadDropzoneProps) {
  const queryClient = useQueryClient()
  const filmRolls = useQuery({ queryKey: ['film-rolls', 'all'], queryFn: () => listFilmRolls() })

  const [filmRollId, setFilmRollId] = useState('')
  const [jobs, setJobs] = useState<UploadJob[]>([])
  const [isDragging, setIsDragging] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const uploadFiles = (files: FileList | File[]) => {
    if (!filmRollId) return

    const accepted = Array.from(files).filter((f) => ACCEPTED_TYPES.includes(f.type))

    for (const file of accepted) {
      const jobId = crypto.randomUUID()
      setJobs((prev) => [...prev, { id: jobId, fileName: file.name, progress: 0, status: 'uploading' }])

      uploadAndRegisterPhoto(file, filmRollId, {}, (percent) => {
        setJobs((prev) => prev.map((j) => (j.id === jobId ? { ...j, progress: percent } : j)))
      })
        .then(() => {
          setJobs((prev) => prev.map((j) => (j.id === jobId ? { ...j, status: 'done', progress: 100 } : j)))
          queryClient.invalidateQueries({ queryKey: ['photos'] })
        })
        .catch((err: Error) => {
          setJobs((prev) =>
            prev.map((j) => (j.id === jobId ? { ...j, status: 'error', error: err.message } : j)),
          )
        })
    }
  }

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    setIsDragging(false)
    if (e.dataTransfer.files.length > 0) uploadFiles(e.dataTransfer.files)
  }

  const handleClose = () => {
    setJobs([])
    onClose()
  }

  return (
    <Dialog open={open} onClose={handleClose} title="Upload scans" widthClassName="max-w-xl">
      <div className="flex flex-col gap-4">
        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium text-neutral-400">Film roll</span>
          <Select value={filmRollId} onChange={(e) => setFilmRollId(e.target.value)}>
            <option value="">Select a roll…</option>
            {filmRolls.data?.map((roll) => (
              <option key={roll.id} value={roll.id}>
                {roll.name} ({roll.brand})
              </option>
            ))}
          </Select>
        </label>

        <div
          onDragOver={(e) => {
            e.preventDefault()
            setIsDragging(true)
          }}
          onDragLeave={() => setIsDragging(false)}
          onDrop={handleDrop}
          onClick={() => filmRollId && fileInputRef.current?.click()}
          className={cn(
            'flex flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed p-10 text-center transition-colors',
            filmRollId ? 'cursor-pointer' : 'cursor-not-allowed opacity-50',
            isDragging ? 'border-white bg-neutral-800/50' : 'border-neutral-700',
          )}
        >
          <UploadCloud size={28} className="text-neutral-500" />
          <p className="text-sm text-neutral-300">Drag & drop scanned frames here, or click to browse</p>
          <p className="text-xs text-neutral-500">JPEG, PNG, TIFF or WebP</p>
          <input
            ref={fileInputRef}
            type="file"
            multiple
            accept={ACCEPTED_TYPES.join(',')}
            className="hidden"
            onChange={(e) => e.target.files && uploadFiles(e.target.files)}
          />
        </div>

        {jobs.length > 0 && (
          <ul className="flex max-h-48 flex-col gap-2 overflow-y-auto">
            {jobs.map((job) => (
              <li key={job.id} className="flex items-center gap-2 text-xs">
                <ImageUp size={14} className="shrink-0 text-neutral-500" />
                <span className="flex-1 truncate text-neutral-300">{job.fileName}</span>
                {job.status === 'uploading' && (
                  <div className="h-1.5 w-20 overflow-hidden rounded-full bg-neutral-800">
                    <div className="h-full bg-white transition-all" style={{ width: `${job.progress}%` }} />
                  </div>
                )}
                {job.status === 'done' && <CheckCircle2 size={14} className="text-emerald-400" />}
                {job.status === 'error' && <XCircle size={14} className="text-red-400" />}
              </li>
            ))}
          </ul>
        )}

        <div className="flex justify-end">
          <Button variant="secondary" onClick={handleClose}>
            Close
          </Button>
        </div>
      </div>
    </Dialog>
  )
}
