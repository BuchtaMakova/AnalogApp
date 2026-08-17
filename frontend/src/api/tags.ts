import { apiClient } from '@/api/client'
import type { Tag } from '@/types/tag'

export async function getTags(): Promise<Tag[]> {
  const { data } = await apiClient.get<Tag[]>('/tags')
  return data
}
