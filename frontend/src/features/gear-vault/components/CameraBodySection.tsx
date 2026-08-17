import { useState } from 'react'
import { Plus } from 'lucide-react'
import { useCameraBodies } from '@/features/gear-vault/hooks/useGear'
import { GearRow } from '@/features/gear-vault/components/GearRow'
import { Dialog } from '@/components/ui/Dialog'
import { FormField, Input, Textarea } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import { formatFilmFormat } from '@/lib/format'
import { FILM_FORMATS, type CameraBody, type FilmFormat } from '@/types/gear'

interface FormState {
  id?: string
  name: string
  brand: string
  model: string
  serialNumber: string
  mountType: string
  notes: string
  acquiredOn: string
  isActive: boolean
  supportedFormats: FilmFormat[]
}

const EMPTY_FORM: FormState = {
  name: '',
  brand: '',
  model: '',
  serialNumber: '',
  mountType: '',
  notes: '',
  acquiredOn: '',
  isActive: true,
  supportedFormats: ['ThirtyFiveMm'],
}

function toFormState(body: CameraBody): FormState {
  return {
    id: body.id,
    name: body.name,
    brand: body.brand,
    model: body.model,
    serialNumber: body.serialNumber ?? '',
    mountType: body.mountType ?? '',
    notes: body.notes ?? '',
    acquiredOn: body.acquiredOn ?? '',
    isActive: body.isActive,
    supportedFormats: body.supportedFormats,
  }
}

export function CameraBodySection() {
  const { data, isLoading, create, update, remove } = useCameraBodies()
  const [form, setForm] = useState<FormState | null>(null)

  const isSaving = create.isPending || update.isPending

  const submit = () => {
    if (!form) return
    const payload = {
      name: form.name,
      brand: form.brand,
      model: form.model,
      serialNumber: form.serialNumber || null,
      mountType: form.mountType || null,
      notes: form.notes || null,
      acquiredOn: form.acquiredOn || null,
      supportedFormats: form.supportedFormats,
    }

    const onDone = () => setForm(null)
    if (form.id) {
      update.mutate({ ...payload, id: form.id, isActive: form.isActive }, { onSuccess: onDone })
    } else {
      create.mutate(payload, { onSuccess: onDone })
    }
  }

  const toggleFormat = (format: FilmFormat) => {
    if (!form) return
    setForm({
      ...form,
      supportedFormats: form.supportedFormats.includes(format)
        ? form.supportedFormats.filter((f) => f !== format)
        : [...form.supportedFormats, format],
    })
  }

  return (
    <div className="flex flex-col gap-3">
      <Button variant="secondary" size="sm" className="self-start" onClick={() => setForm(EMPTY_FORM)}>
        <Plus size={14} />
        Add camera body
      </Button>

      {isLoading && <Spinner size={18} className="text-neutral-500" />}

      <div className="flex flex-col gap-2">
        {data?.map((body) => (
          <GearRow
            key={body.id}
            title={`${body.brand} ${body.model}`}
            subtitle={body.name}
            meta={body.supportedFormats.map(formatFilmFormat)}
            isActive={body.isActive}
            onEdit={() => setForm(toFormState(body))}
            onDelete={() => confirm(`Delete ${body.name}?`) && remove.mutate(body.id)}
          />
        ))}
      </div>

      <Dialog open={!!form} onClose={() => setForm(null)} title={form?.id ? 'Edit camera body' : 'Add camera body'}>
        {form && (
          <form
            className="flex flex-col gap-3"
            onSubmit={(e) => {
              e.preventDefault()
              submit()
            }}
          >
            <FormField label="Name">
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            </FormField>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Brand">
                <Input value={form.brand} onChange={(e) => setForm({ ...form, brand: e.target.value })} required />
              </FormField>
              <FormField label="Model">
                <Input value={form.model} onChange={(e) => setForm({ ...form, model: e.target.value })} required />
              </FormField>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Serial number">
                <Input value={form.serialNumber} onChange={(e) => setForm({ ...form, serialNumber: e.target.value })} />
              </FormField>
              <FormField label="Mount type">
                <Input value={form.mountType} onChange={(e) => setForm({ ...form, mountType: e.target.value })} />
              </FormField>
            </div>
            <FormField label="Acquired on">
              <Input
                type="date"
                value={form.acquiredOn}
                onChange={(e) => setForm({ ...form, acquiredOn: e.target.value })}
              />
            </FormField>
            <FormField label="Supported film formats">
              <div className="flex flex-wrap gap-2">
                {FILM_FORMATS.map((format) => (
                  <button
                    key={format}
                    type="button"
                    onClick={() => toggleFormat(format)}
                    className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                      form.supportedFormats.includes(format)
                        ? 'bg-white text-neutral-900'
                        : 'bg-neutral-800 text-neutral-400 hover:bg-neutral-700'
                    }`}
                  >
                    {formatFilmFormat(format)}
                  </button>
                ))}
              </div>
            </FormField>
            <FormField label="Notes">
              <Textarea value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
            </FormField>
            {form.id && (
              <label className="flex items-center gap-2 text-sm text-neutral-300">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
                  className="accent-white"
                />
                Active
              </label>
            )}
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
