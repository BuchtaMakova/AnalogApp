import type { InputHTMLAttributes, PropsWithChildren, SelectHTMLAttributes, TextareaHTMLAttributes } from 'react'
import { cn } from '@/lib/cn'

const controlClasses =
  'w-full rounded-lg border border-neutral-700 bg-neutral-800 px-3 py-2 text-sm text-white ' +
  'placeholder:text-neutral-500 focus:border-neutral-500 focus:outline-none'

export function FormField({ label, children }: PropsWithChildren<{ label: string }>) {
  return (
    <label className="flex flex-col gap-1.5">
      <span className="text-xs font-medium text-neutral-400">{label}</span>
      {children}
    </label>
  )
}

export function Input(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={cn(controlClasses, props.className)} />
}

export function Textarea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea {...props} className={cn(controlClasses, 'min-h-20 resize-y', props.className)} />
}

export function Select(props: SelectHTMLAttributes<HTMLSelectElement>) {
  return <select {...props} className={cn(controlClasses, props.className)} />
}
