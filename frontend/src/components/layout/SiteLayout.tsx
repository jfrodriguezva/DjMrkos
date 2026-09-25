import { Outlet } from 'react-router-dom'
import { Footer } from './Footer'
import { Header } from './Header'

/** Wraps every public page. The QR and admin routes intentionally render outside this shell. */
export function SiteLayout() {
  return (
    <>
      <Header />
      <main>
        <Outlet />
      </main>
      <Footer />
    </>
  )
}
