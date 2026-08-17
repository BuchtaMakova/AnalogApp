import { useState } from 'react'
import { Tabs } from '@/components/ui/Tabs'
import { CameraBodySection } from '@/features/gear-vault/components/CameraBodySection'
import { LensSection } from '@/features/gear-vault/components/LensSection'
import { FlashSection } from '@/features/gear-vault/components/FlashSection'

const TAB_ITEMS = [
  { key: 'camera-bodies', label: 'Camera bodies' },
  { key: 'lenses', label: 'Lenses' },
  { key: 'flashes', label: 'Flashes' },
]

export function GearVaultPage() {
  const [activeTab, setActiveTab] = useState('camera-bodies')

  return (
    <div className="flex h-full flex-col">
      <div className="px-6 pt-6 pb-4">
        <h2 className="text-xl font-semibold text-white">Gear Vault</h2>
        <p className="mt-1 text-sm text-neutral-400">Camera bodies, lenses and flashes inventory.</p>
      </div>

      <div className="px-6">
        <Tabs items={TAB_ITEMS} active={activeTab} onChange={setActiveTab} />
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        {activeTab === 'camera-bodies' && <CameraBodySection />}
        {activeTab === 'lenses' && <LensSection />}
        {activeTab === 'flashes' && <FlashSection />}
      </div>
    </div>
  )
}
