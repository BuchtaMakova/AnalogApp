import { Files, FolderArchive } from 'lucide-react'
import { Dialog } from '@/components/ui/Dialog'
import { Button } from '@/components/ui/Button'

interface DownloadSelectionDialogProps {
  open: boolean
  count: number
  onClose: () => void
  onDownloadSeparately: () => void
  onDownloadZip: () => void
  isDownloading: boolean
}

export function DownloadSelectionDialog({
  open,
  count,
  onClose,
  onDownloadSeparately,
  onDownloadZip,
  isDownloading,
}: DownloadSelectionDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} title={`Download ${count} photo${count === 1 ? '' : 's'}`}>
      <div className="flex flex-col gap-2">
        <Button
          variant="secondary"
          className="w-full justify-start"
          onClick={onDownloadSeparately}
          disabled={isDownloading}
        >
          <Files size={16} />
          <div className="flex flex-col items-start">
            <span>Download separately</span>
            <span className="text-xs font-normal text-neutral-400">One file per photo</span>
          </div>
        </Button>
        <Button variant="secondary" className="w-full justify-start" onClick={onDownloadZip} disabled={isDownloading}>
          <FolderArchive size={16} />
          <div className="flex flex-col items-start">
            <span>Download as ZIP</span>
            <span className="text-xs font-normal text-neutral-400">Bundled into one archive</span>
          </div>
        </Button>
      </div>
    </Dialog>
  )
}
