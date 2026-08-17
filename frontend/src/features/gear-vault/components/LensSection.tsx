import { useState } from 'react'
import { Plus } from 'lucide-react'
import { useLenses } from '@/features/gear-vault/hooks/useGear'
import { GearRow } from '@/features/gear-vault/components/GearRow'
import { Dialog } from '@/components/ui/Dialog'
import { FormField, Input, Textarea } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import type { Lens } from '@/types/gear'

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
  focalLengthMinMm: string
  focalLengthMaxMm: string
  maxAperture: string
  minAperture: string
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
  focalLengthMinMm: '',
  focalLengthMaxMm: '',
  maxAperture: '',
  minAperture: '',
}

function toFormState(lens: Lens): FormState {
  return {
    id: lens.id,
    name: lens.name,
    brand: lens.brand,
    model: lens.model,
    serialNumber: lens.serialNumber ?? '',
    mountType: lens.mountType ?? '',
    notes: lens.notes ?? '',
    acquiredOn: lens.acquiredOn ?? '',
    isActive: lens.isActive,
    focalLengthMinMm: lens.focalLengthMinMm?.toString() ?? '',
    focalLengthMaxMm: lens.focalLengthMaxMm?.toString() ?? '',
    maxAperture: lens.maxAperture?.toString() ?? '',
    minAperture: lens.minAperture?.toString() ?? '',
  }
}

function focalLengthLabel(lens: Lens): string | null {
  if (!lens.focalLengthMinMm) return null
  return lens.focalLengthMaxMm && lens.focalLengthMaxMm !== lens.focalLengthMinMm
    ? `${lens.focalLengthMinMm}-${lens.focalLengthMaxMm}mm`
    : `${lens.focalLengthMinMm}mm`
}

export function LensSection() {
  const { data, isLoading, create, update, remove } = useLenses()
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
      focalLengthMinMm: form.focalLengthMinMm ? Number(form.focalLengthMinMm) : null,
      focalLengthMaxMm: form.focalLengthMaxMm ? Number(form.focalLengthMaxMm) : null,
      maxAperture: form.maxAperture ? Number(form.maxAperture) : null,
      minAperture: form.minAperture ? Number(form.minAperture) : null,
    }

    const onDone = () => setForm(null)
    if (form.id) {
      update.mutate({ ...payload, id: form.id, isActive: form.isActive }, { onSuccess: onDone })
    } else {
      create.mutate(payload, { onSuccess: onDone })
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <Button variant="secondary" size="sm" className="self-start" onClick={() => setForm(EMPTY_FORM)}>
        <Plus size={14} />
        Add lens
      </Button>

      {isLoading && <Spinner size={18} className="text-neutral-500" />}

      <div className="flex flex-col gap-2">
        {data?.map((lens) => (
          <GearRow
            key={lens.id}
            title={`${lens.brand} ${lens.model}`}
            subtitle={lens.name}
            meta={[focalLengthLabel(lens), lens.maxAperture ? `f/${lens.maxAperture}` : null].filter(
              (v): v is string => !!v,
            )}
            isActive={lens.isActive}
            onEdit={() => setForm(toFormState(lens))}
            onDelete={() => confirm(`Delete ${lens.name}?`) && remove.mutate(lens.id)}
          />
        ))}
      </div>

      <Dialog open={!!form} onClose={() => setForm(null)} title={form?.id ? 'Edit lens' : 'Add lens'}>
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
              <FormField label="Min focal length (mm)">
                <Input
                  type="number"
                  value={form.focalLengthMinMm}
                  onChange={(e) => setForm({ ...form, focalLengthMinMm: e.target.value })}
                />
              </FormField>
              <FormField label="Max focal length (mm)">
                <Input
                  type="number"
                  value={form.focalLengthMaxMm}
                  onChange={(e) => setForm({ ...form, focalLengthMaxMm: e.target.value })}
                />
              </FormField>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <FormField label="Max aperture (f/)">
                <Input
                  type="number"
                  step="0.1"
                  value={form.maxAperture}
                  onChange={(e) => setForm({ ...form, maxAperture: e.target.value })}
                />
              </FormField>
              <FormField label="Min aperture (f/)">
                <Input
                  type="number"
                  step="0.1"
                  value={form.minAperture}
                  onChange={(e) => setForm({ ...form, minAperture: e.target.value })}
                />
              </FormField>
            </div>
            <FormField label="Mount type">
              <Input value={form.mountType} onChange={(e) => setForm({ ...form, mountType: e.target.value })} />
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
