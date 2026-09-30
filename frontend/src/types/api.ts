// Mirrors DjMrkos.Application's DTOs one-to-one — keep this file and the C# records in sync.

export interface MenuCategory {
  id: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  price: number | null
  originalPrice: number | null
  discountPercentage: number | null
  promotionLabel: string | null
}

export interface MenuModule {
  id: string
  name: string
  slug: string
  icon: string | null
  categories: MenuCategory[]
}

export interface ModuleAdmin {
  id: string
  name: string
  slug: string
  icon: string | null
  displayOrder: number
  isActive: boolean
  createdAtUtc: string
}

export interface CategoryAdmin {
  id: string
  moduleId: string
  name: string
  slug: string
  description: string | null
  imageUrl: string | null
  price: number | null
  displayOrder: number
  isActive: boolean
  createdAtUtc: string
}

export type EventStatus = 0 | 1 | 2 | 3 // Scheduled | Live | Completed | Cancelled

export interface EventAdmin {
  id: string
  clientName: string
  location: string | null
  eventDateUtc: string
  status: EventStatus
  qrToken: string
  qrValidFromUtc: string
  qrValidUntilUtc: string
}

export interface EventWithQr {
  event: EventAdmin
  qrCodeDataUrl: string
}

export interface EventPublic {
  id: string
  clientName: string
  eventDateUtc: string
  isRequestWindowOpen: boolean
}

export type SongRequestStatus = 0 | 1 | 2 | 3 // Pending | Queued | Played | Rejected

export interface SongRequest {
  id: string
  eventId: string
  songTitle: string
  artist: string | null
  requesterName: string | null
  dedication: string | null
  status: SongRequestStatus
  createdAtUtc: string
}

export interface Testimonial {
  id: string
  clientName: string
  eventId: string | null
  rating: number
  comment: string
  isApproved: boolean
  createdAtUtc: string
}

export type LeadStatus = 0 | 1 | 2 | 3 // New | Contacted | Won | Lost

export interface Lead {
  id: string
  name: string
  email: string
  phone: string | null
  eventDate: string | null
  message: string
  status: LeadStatus
  createdAtUtc: string
}

export interface PromotionAdmin {
  id: string
  moduleId: string | null
  categoryId: string | null
  label: string
  discountPercentage: number
  isActive: boolean
  createdAtUtc: string
}

export interface ApiProblem {
  title: string
  status: number
  errors?: Record<string, string[]>
}
