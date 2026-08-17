import { cn } from '@/lib/cn'

export interface TabItem {
  key: string
  label: string
}

interface TabsProps {
  items: TabItem[]
  active: string
  onChange: (key: string) => void
}

export function Tabs({ items, active, onChange }: TabsProps) {
  return (
    <div className="flex gap-1 border-b border-neutral-800">
      {items.map((item) => (
        <button
          key={item.key}
          type="button"
          onClick={() => onChange(item.key)}
          className={cn(
            'border-b-2 px-3 py-2 text-sm font-medium transition-colors',
            active === item.key
              ? 'border-white text-white'
              : 'border-transparent text-neutral-400 hover:text-neutral-200',
          )}
        >
          {item.label}
        </button>
      ))}
    </div>
  )
}
