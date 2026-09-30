// Placeholder — reemplaza con los datos reales del negocio antes de publicar el sitio.
export const CONTACT = {
  whatsappNumber: '5215500000000', // formato E.164 sin "+", el que espera wa.me
  phoneDisplay: '+52 55 0000 0000',
  phoneHref: 'tel:+525500000000',
  email: 'hola@djmrkos.com',
}

export function whatsappHref(message?: string): string {
  const base = `https://wa.me/${CONTACT.whatsappNumber}`
  return message ? `${base}?text=${encodeURIComponent(message)}` : base
}
