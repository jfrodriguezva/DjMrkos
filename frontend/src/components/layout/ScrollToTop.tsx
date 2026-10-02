import { useEffect } from 'react'
import { useLocation } from 'react-router-dom'

/**
 * BrowserRouter keeps the scroll position across navigations, so a footer link would load
 * the new page but leave the visitor staring at the footer. Jump to the top whenever the
 * path changes.
 */
export function ScrollToTop() {
  const { pathname } = useLocation()

  useEffect(() => {
    window.scrollTo(0, 0)
  }, [pathname])

  return null
}
