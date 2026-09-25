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

const MIN_GUESTS = 10
const MAX_GUESTS = 2000

/** Guards every entry point (typed input, +/− steppers) the same way, so none of them can bypass the bounds the others enforce. */
export function clampGuestCount(value: number): number {
  if (Number.isNaN(value)) return MIN_GUESTS
  return Math.min(MAX_GUESTS, Math.max(MIN_GUESTS, Math.round(value)))
}

function loadState(): PlannerState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return DEFAULT_STATE
    const merged = { ...DEFAULT_STATE, ...(JSON.parse(raw) as Partial<PlannerState>) }
    // Re-clamps on load too — a value saved before this bound existed (or edited by hand) shouldn't survive a refresh.
    return { ...merged, guestCount: clampGuestCount(merged.guestCount) }
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
    setState((prev) => ({ ...prev, guestCount: clampGuestCount(guestCount) }))
  }, [])

  const setPreferredDate = useCallback((preferredDate: string | null) => {
    setState((prev) => ({ ...prev, preferredDate }))
  }, [])

  const clearCart = useCallback(() => {
    setState((prev) => ({ ...prev, cart: [] }))
  }, [])

  return { ...state, addItem, changeQty, removeItem, setGuestCount, setPreferredDate, clearCart }
}
