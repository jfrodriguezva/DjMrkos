// Datos de contacto del negocio. Plain links only (tel:, mailto:, wa.me): nothing from
// Meta is embedded in the site — wa.me just opens the visitor's own WhatsApp app.
export const CONTACT = {
  whatsappNumber: '525524976505', // 52 + 10 dígitos, sin "+", el formato que espera wa.me
  phoneDisplay: '55 2497 6505',
  phoneHref: 'tel:+525524976505',
  email: 'djmrkos@gmail.com',
}

export function whatsappHref(message?: string): string {
  const base = `https://wa.me/${CONTACT.whatsappNumber}`
  return message ? `${base}?text=${encodeURIComponent(message)}` : base
}
