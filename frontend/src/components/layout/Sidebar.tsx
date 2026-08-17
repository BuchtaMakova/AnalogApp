import { NavLink } from 'react-router-dom'
import { Aperture, Camera, Images, Library } from 'lucide-react'

const navItems = [
  { to: '/', label: 'Library', icon: Images, end: true },
  { to: '/rolls', label: 'Rolls', icon: Aperture },
  { to: '/gear', label: 'Gear Vault', icon: Camera },
  { to: '/knowledge-base', label: 'Knowledge Base', icon: Library },
]

export function Sidebar() {
  return (
    <nav className="flex w-56 shrink-0 flex-col gap-1 border-r border-neutral-800 bg-neutral-900/50 p-4">
      <h1 className="mb-4 px-2 text-sm font-semibold tracking-wide text-neutral-400 uppercase">
        Analog Hub
      </h1>
      {navItems.map(({ to, label, icon: Icon, end }) => (
        <NavLink
          key={to}
          to={to}
          end={end}
          className={({ isActive }) =>
            `flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
              isActive
                ? 'bg-neutral-800 text-white'
                : 'text-neutral-400 hover:bg-neutral-800/60 hover:text-neutral-200'
            }`
          }
        >
          <Icon size={18} />
          {label}
        </NavLink>
      ))}
    </nav>
  )
}
