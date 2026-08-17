import { createBrowserRouter } from 'react-router-dom'
import { AppLayout } from '@/components/layout/AppLayout'
import { PhotoLibraryPage } from '@/features/photo-library/PhotoLibraryPage'
import { GearVaultPage } from '@/features/gear-vault/GearVaultPage'
import { RollManagerPage } from '@/features/roll-manager/RollManagerPage'
import { KnowledgeBasePage } from '@/features/knowledge-base/KnowledgeBasePage'
import { AuthPage } from '@/features/auth/AuthPage'
import { ProtectedRoute } from '@/features/auth/ProtectedRoute'

export const router = createBrowserRouter([
  { path: '/login', element: <AuthPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/',
        element: <AppLayout />,
        children: [
          { index: true, element: <PhotoLibraryPage /> },
          { path: 'gear', element: <GearVaultPage /> },
          { path: 'rolls', element: <RollManagerPage /> },
          { path: 'knowledge-base', element: <KnowledgeBasePage /> },
        ],
      },
    ],
  },
])
