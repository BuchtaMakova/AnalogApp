import { useState } from 'react'
import { Plus, Trash2, Pencil } from 'lucide-react'
import { useFilmRolls } from '@/features/roll-manager/hooks/useFilmRolls'
import { Dialog } from '@/components/ui/Dialog'
import { FormField, Input, Select, Textarea } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge'
import { Spinner } from '@/components/ui/Spinner'
import { formatDate, formatFilmFormat, formatFilmRollStatus } from '@/lib/format'
import { FILM_FORMATS, FILM_ROLL_STATUSES, type FilmFormat, type FilmRoll, type FilmRollStatus } from '@/types/gear'

interface FormState {
  id?: string
  name: string
  brand: string
  format: FilmFormat
  nominalIso: string
  exposedAtIso: string
  frameCount: string
  status: FilmRollStatus
  cameraBodyId: string
  dateLoaded: string
  dateFinished: string
  dateDeveloped: string
  labName: string
  developerNotes: string
  notes: string
}

const EMPTY_FORM: FormState = {
  name: '',
  brand: '',
  format: 'ThirtyFiveMm',
  nominalIso: '400',
  exposedAtIso: '',
  frameCount: '36',
  status: 'Loaded',
  cameraBodyId: '',
  dateLoaded: '',
  dateFinished: '',
  dateDeveloped: '',
  labName: '',
  developerNotes: '',
  notes: '',
}

function toFormState(roll: FilmRoll): FormState {
  return {
    id: roll.id,
    name: roll.name,
    brand: roll.brand,
    format: roll.format,
    nominalIso: roll.nominalIso.toString(),
    exposedAtIso: roll.exposedAtIso?.toString() ?? '',
    frameCount: roll.frameCount.toString(),
    status: roll.status,
    cameraBodyId: roll.cameraBodyId ?? '',
    dateLoaded: roll.dateLoaded ?? '',
    dateFinished: roll.dateFinished ?? '',
    dateDeveloped: roll.dateDeveloped ?? '',
    labName: roll.labName ?? '',
    developerNotes: roll.developerNotes ?? '',
    notes: roll.notes ?? '',
  }
}

const STATUS_TONE: Record<FilmRollStatus, 'neutral' | 'success' | 'warning'> = {
  Loaded: 'neutral',
  InProgress: 'neutral',
  ShotCompleted: 'warning',
  SentToLab: 'warning',
  Developed: 'success',
  Scanned: 'success',
  Archived: 'neutral',
}

export function FilmRollSection() {
  const [statusFilter, setStatusFilter] = useState<FilmRollStatus | undefined>(undefined)
  const { data, isLoading, cameraBodies, create, update, remove } = useFilmRolls(statusFilter)
  const [form, setForm] = useState<FormState | null>(null)

  const isSaving = create.isPending || update.isPending

  const submit = () => {
    if (!form) return
    const onDone = () => setForm(null)

    if (form.id) {
      update.mutate(
        {
          id: form.id,
          name: form.name,
          brand: form.brand,
          format: form.format,
          nominalIso: Number(form.nominalIso),
          exposedAtIso: form.exposedAtIso ? Number(form.exposedAtIso) : null,
          frameCount: Number(form.frameCount),
          status: form.status,
          cameraBodyId: form.cameraBodyId || null,
          dateLoaded: form.dateLoaded || null,
          dateFinished: form.dateFinished || null,
          dateDeveloped: form.dateDeveloped || null,
          labName: form.labName || null,
          developerNotes: form.developerNotes || null,
          notes: form.notes || null,
        },
        { onSuccess: onDone },
      )
    } else {
      create.mutate(
        {
          name: form.name,
          brand: form.brand,
          format: form.format,
          nominalIso: Number(form.nominalIso),
          exposedAtIso: form.exposedAtIso ? Number(form.exposedAtIso) : null,
          frameCount: Number(form.frameCount),
          cameraBodyId: form.cameraBodyId || null,
          dateLoaded: form.dateLoaded || null,
          notes: form.notes || null,
        },
        { onSuccess: onDone },
      )
    }
  }

  return (
    <div className="flex h-full flex-col">
      <div className="flex flex-wrap items-center justify-between gap-3 px-6 pt-6 pb-4">
        <div>
          <h2 className="text-xl font-semibold text-white">Film Rolls</h2>
          <p className="mt-1 text-sm text-neutral-400">Loaded, shot and developed roll tracking.</p>
        </div>
        <Button variant="primary" onClick={() => setForm(EMPTY_FORM)}>
          <Plus size={16} />
          Load a new roll
        </Button>
      </div>

      <div className="flex flex-wrap gap-1.5 px-6 pb-4">
        <button
          onClick={() => setStatusFilter(undefined)}
          className={`rounded-full px-2.5 py-1 text-xs font-medium ${!statusFilter ? 'bg-white text-neutral-900' : 'bg-neutral-800 text-neutral-400'}`}
        >
          All
        </button>
        {FILM_ROLL_STATUSES.map((status) => (
          <button
            key={status}
            onClick={() => setStatusFilter(status)}
            className={`rounded-full px-2.5 py-1 text-xs font-medium ${statusFilter === status ? 'bg-white text-neutral-900' : 'bg-neutral-800 text-neutral-400'}`}
          >
            {formatFilmRollStatus(status)}
          </button>
        ))}
      </div>

      <div className="flex-1 overflow-y-auto px-6 pb-6">
        {isLoading && <Spinner size={18} className="text-neutral-500" />}

        <div className="flex flex-col gap-2">
          {data?.map((roll) => (
            <div key={roll.id} className="flex items-center justify-between gap-4 rounded-lg border border-neutral-800 px-4 py-3">
              <div>
                <div className="flex items-center gap-2">
                  <p className="text-sm font-medium text-white">{roll.name}</p>
                  <Badge tone={STATUS_TONE[roll.status]}>{formatFilmRollStatus(roll.status)}</Badge>
                  <Badge>{formatFilmFormat(roll.format)}</Badge>
                </div>
                <p className="mt-0.5 text-xs text-neutral-500">
                  {roll.brand} · ISO {roll.exposedAtIso ?? roll.nominalIso} · {roll.frameCount} frames
                  {roll.cameraBodyName ? ` · ${roll.cameraBodyName}` : ''}
                </p>
                <p className="mt-0.5 text-xs text-neutral-600">Loaded {formatDate(roll.dateLoaded)}</p>
              </div>
              <div className="flex shrink-0 gap-1">
                <button
                  type="button"
                  onClick={() => setForm(toFormState(roll))}
                  className="rounded-md p-1.5 text-neutral-400 hover:bg-neutral-800 hover:text-white"
                  aria-label="Edit"
                >
                  <Pencil size={14} />
                </button>
                <button
                  type="button"
                  onClick={() => confirm(`Delete ${roll.name}?`) && remove.mutate(roll.id)}
                  className="rounded-md p-1.5 text-neutral-400 hover:bg-red-950 hover:text-red-400"
                  aria-label="Delete"
                >
                  <Trash2 size={14} />
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      <Dialog open={!!form} onClose={() => setForm(null)} title={form?.id ? 'Edit film roll' : 'Load a new roll'}>
        {form && (
          <form
            className="flex max-h-[70vh] flex-col gap-3 overflow-y-auto"
            onSubmit={(e) => {
              e.preventDefault()
              submit()
            }}
          >
            <FormField label="Name">
              <Input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="Kodak Portra 400"
                required
              />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Brand">
                <Input value={form.brand} onChange={(e) => setForm({ ...form, brand: e.target.value })} required />
              </FormField>
              <FormField label="Format">
                <Select value={form.format} onChange={(e) => setForm({ ...form, format: e.target.value as FilmFormat })}>
                  {FILM_FORMATS.map((format) => (
                    <option key={format} value={format}>
                      {formatFilmFormat(format)}
                    </option>
                  ))}
                </Select>
              </FormField>
            </div>
            <div className="grid grid-cols-3 gap-3">
              <FormField label="Nominal ISO">
                <Input
                  type="number"
                  value={form.nominalIso}
                  onChange={(e) => setForm({ ...form, nominalIso: e.target.value })}
                  required
                />
              </FormField>
              <FormField label="Exposed at ISO">
                <Input
                  type="number"
                  value={form.exposedAtIso}
                  onChange={(e) => setForm({ ...form, exposedAtIso: e.target.value })}
                  placeholder="Push/pull"
                />
              </FormField>
              <FormField label="Frame count">
                <Input
                  type="number"
                  value={form.frameCount}
                  onChange={(e) => setForm({ ...form, frameCount: e.target.value })}
                  required
                />
              </FormField>
            </div>
            <FormField label="Camera body">
              <Select value={form.cameraBodyId} onChange={(e) => setForm({ ...form, cameraBodyId: e.target.value })}>
                <option value="">Unassigned</option>
                {cameraBodies.data?.map((body) => (
                  <option key={body.id} value={body.id}>
                    {body.brand} {body.model}
                  </option>
                ))}
              </Select>
            </FormField>

            {!form.id && (
              <FormField label="Date loaded">
                <Input type="date" value={form.dateLoaded} onChange={(e) => setForm({ ...form, dateLoaded: e.target.value })} />
              </FormField>
            )}

            {form.id && (
              <>
                <FormField label="Status">
                  <Select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value as FilmRollStatus })}>
                    {FILM_ROLL_STATUSES.map((status) => (
                      <option key={status} value={status}>
                        {formatFilmRollStatus(status)}
                      </option>
                    ))}
                  </Select>
                </FormField>
                <div className="grid grid-cols-3 gap-3">
                  <FormField label="Loaded">
                    <Input type="date" value={form.dateLoaded} onChange={(e) => setForm({ ...form, dateLoaded: e.target.value })} />
                  </FormField>
                  <FormField label="Finished">
                    <Input type="date" value={form.dateFinished} onChange={(e) => setForm({ ...form, dateFinished: e.target.value })} />
                  </FormField>
                  <FormField label="Developed">
                    <Input type="date" value={form.dateDeveloped} onChange={(e) => setForm({ ...form, dateDeveloped: e.target.value })} />
                  </FormField>
                </div>
                <FormField label="Lab name">
                  <Input value={form.labName} onChange={(e) => setForm({ ...form, labName: e.target.value })} />
                </FormField>
                <FormField label="Developer notes">
                  <Textarea value={form.developerNotes} onChange={(e) => setForm({ ...form, developerNotes: e.target.value })} />
                </FormField>
              </>
            )}

            <FormField label="Notes">
              <Textarea value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
            </FormField>

            <div className="flex justify-end gap-2 pt-1">
              <Button type="button" variant="ghost" onClick={() => setForm(null)}>
                Cancel
              </Button>
              <Button type="submit" variant="primary" disabled={isSaving}>
                {isSaving && <Spinner size={14} />}
                Save
              </Button>
            </div>
          </form>
        )}
      </Dialog>
    </div>
  )
}
