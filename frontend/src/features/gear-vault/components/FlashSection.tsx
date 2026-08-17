import { useState } from 'react'
import { Plus } from 'lucide-react'
import { useFlashes } from '@/features/gear-vault/hooks/useGear'
import { GearRow } from '@/features/gear-vault/components/GearRow'
import { Dialog } from '@/components/ui/Dialog'
import { FormField, Input, Textarea } from '@/components/ui/Field'
import { Button } from '@/components/ui/Button'
import { Spinner } from '@/components/ui/Spinner'
import type { Flash } from '@/types/gear'

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
  guideNumber: string
  hasTtl: boolean
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
  guideNumber: '',
  hasTtl: false,
}

function toFormState(flash: Flash): FormState {
  return {
    id: flash.id,
    name: flash.name,
    brand: flash.brand,
    model: flash.model,
    serialNumber: flash.serialNumber ?? '',
    mountType: flash.mountType ?? '',
    notes: flash.notes ?? '',
    acquiredOn: flash.acquiredOn ?? '',
    isActive: flash.isActive,
    guideNumber: flash.guideNumber?.toString() ?? '',
    hasTtl: flash.hasTtl,
  }
}

export function FlashSection() {
  const { data, isLoading, create, update, remove } = useFlashes()
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
      guideNumber: form.guideNumber ? Number(form.guideNumber) : null,
      hasTtl: form.hasTtl,
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
        Add flash
      </Button>

      {isLoading && <Spinner size={18} className="text-neutral-500" />}

      <div className="flex flex-col gap-2">
        {data?.map((flash) => (
          <GearRow
            key={flash.id}
            title={`${flash.brand} ${flash.model}`}
            subtitle={flash.name}
            meta={[flash.guideNumber ? `GN ${flash.guideNumber}` : null, flash.hasTtl ? 'TTL' : null].filter(
              (v): v is string => !!v,
            )}
            isActive={flash.isActive}
            onEdit={() => setForm(toFormState(flash))}
            onDelete={() => confirm(`Delete ${flash.name}?`) && remove.mutate(flash.id)}
          />
        ))}
      </div>

      <Dialog open={!!form} onClose={() => setForm(null)} title={form?.id ? 'Edit flash' : 'Add flash'}>
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
              <FormField label="Guide number">
                <Input
                  type="number"
                  value={form.guideNumber}
                  onChange={(e) => setForm({ ...form, guideNumber: e.target.value })}
                />
              </FormField>
              <label className="flex items-center gap-2 self-end pb-2 text-sm text-neutral-300">
                <input
                  type="checkbox"
                  checked={form.hasTtl}
                  onChange={(e) => setForm({ ...form, hasTtl: e.target.checked })}
                  className="accent-white"
                />
                TTL support
              </label>
            </div>
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
