import { useCallback, useEffect, useState } from 'react'

export interface CartLine {
  categoryId: string
  name: string
  moduleName: string
  price: number | null
  quantity: number
}

interface PlannerState {
  cart: CartLine[]
  guestCount: number
  preferredDate: string | null // YYYY-MM-DD
}

const STORAGE_KEY = 'djmrkos.planner'
const DEFAULT_STATE: PlannerState = { cart: [], guestCount: 100, preferredDate: null }

function loadState(): PlannerState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return DEFAULT_STATE
    return { ...DEFAULT_STATE, ...(JSON.parse(raw) as Partial<PlannerState>) }
  } catch {
    return DEFAULT_STATE
  }
}

/**
 * The quote a visitor builds on /cotizador is the same one they review on /contratar, and
 * the date they pick on /agendar is the one that shows up pre-filled there too — all without
 * an account. localStorage is the entire "session"; each page just reads it fresh on mount
 * since Cotizador/Agendar/Contratar are never mounted at the same time.
 */
export function usePlanner() {
  const [state, setState] = useState<PlannerState>(loadState)

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
    } catch {
      // Private browsing or blocked storage — the plan just won't survive a page change.
    }
  }, [state])

  const addItem = useCallback((payload: Omit<CartLine, 'quantity'>) => {
    setState((prev) => {
      const existing = prev.cart.find((l) => l.categoryId === payload.categoryId)
      const cart = existing
        ? prev.cart.map((l) => (l.categoryId === payload.categoryId ? { ...l, quantity: l.quantity + 1 } : l))
        : [...prev.cart, { ...payload, quantity: 1 }]
      return { ...prev, cart }
    })
  }, [])

  const changeQty = useCallback((categoryId: string, delta: number) => {
    setState((prev) => ({
      ...prev,
      cart: prev.cart.map((l) => (l.categoryId === categoryId ? { ...l, quantity: l.quantity + delta } : l)).filter((l) => l.quantity > 0),
    }))
  }, [])

  const removeItem = useCallback((categoryId: string) => {
    setState((prev) => ({ ...prev, cart: prev.cart.filter((l) => l.categoryId !== categoryId) }))
  }, [])

  const setGuestCount = useCallback((guestCount: number) => {
    setState((prev) => ({ ...prev, guestCount }))
  }, [])

  const setPreferredDate = useCallback((preferredDate: string | null) => {
    setState((prev) => ({ ...prev, preferredDate }))
  }, [])

  const clearCart = useCallback(() => {
    setState((prev) => ({ ...prev, cart: [] }))
  }, [])

  return { ...state, addItem, changeQty, removeItem, setGuestCount, setPreferredDate, clearCart }
}
