import { Route, Routes } from 'react-router-dom'
import { SiteLayout } from './components/layout/SiteLayout'
import { AboutPage } from './pages/AboutPage'
import { AdminDashboardPage } from './pages/admin/AdminDashboardPage'
import { AdminLoginPage } from './pages/admin/AdminLoginPage'
import { ContactPage } from './pages/ContactPage'
import { HomePage } from './pages/HomePage'
import { NotFoundPage } from './pages/NotFoundPage'
import { ServiceDetailPage } from './pages/ServiceDetailPage'
import { ServicesPage } from './pages/ServicesPage'
import { SongRequestPage } from './pages/SongRequestPage'
import { TestimonialsPage } from './pages/TestimonialsPage'

export default function App() {
  return (
    <Routes>
      {/* Public QR flow and admin panel intentionally skip the marketing site's header/footer. */}
      <Route path="/evento/:token" element={<SongRequestPage />} />
      <Route path="/admin/login" element={<AdminLoginPage />} />
      <Route path="/admin" element={<AdminDashboardPage />} />

      <Route element={<SiteLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/servicios" element={<ServicesPage />} />
        <Route path="/servicios/:moduleSlug" element={<ServiceDetailPage />} />
        <Route path="/quienes-somos" element={<AboutPage />} />
        <Route path="/testimonios" element={<TestimonialsPage />} />
        <Route path="/contacto" element={<ContactPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
