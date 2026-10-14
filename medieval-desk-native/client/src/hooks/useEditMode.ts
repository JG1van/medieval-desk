import { useState, useCallback } from 'react'

export function useEditMode() {
  const [isEditMode, setIsEditMode] = useState(false)
  const toggle = useCallback(() => setIsEditMode((v) => !v), [])
  return { isEditMode, toggle, setEditMode: setIsEditMode }
}