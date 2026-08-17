import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createFilmRoll, deleteFilmRoll, listCameraBodies, listFilmRolls, updateFilmRoll } from '@/api/gear'
import type { CreateFilmRollPayload, FilmRollStatus, UpdateFilmRollPayload } from '@/types/gear'

export function useFilmRolls(status?: FilmRollStatus) {
  const queryClient = useQueryClient()
  const query = useQuery({ queryKey: ['film-rolls', status ?? 'all'], queryFn: () => listFilmRolls(status) })
  const cameraBodies = useQuery({ queryKey: ['camera-bodies'], queryFn: () => listCameraBodies() })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['film-rolls'] })

  const create = useMutation({
    mutationFn: (payload: CreateFilmRollPayload) => createFilmRoll(payload),
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: (payload: UpdateFilmRollPayload) => updateFilmRoll(payload),
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: (id: string) => deleteFilmRoll(id),
    onSuccess: invalidate,
  })

  return { ...query, cameraBodies, create, update, remove }
}
