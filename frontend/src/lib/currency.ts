const formatter = new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN', maximumFractionDigits: 0 })

export function formatMxn(amount: number): string {
  return formatter.format(amount)
}

export interface AudienceTier {
  label: string
  audio: string
  lighting: string
}

const TIERS: { max: number; tier: AudienceTier }[] = [
  {
    max: 60,
    tier: {
      label: 'Evento íntimo',
      audio: '2 bocinas activas cubren el salón sin problema.',
      lighting: '1 torre de luces o el set de LED RGB es suficiente ambiente.',
    },
  },
  {
    max: 150,
    tier: {
      label: 'Evento mediano',
      audio: '4 bocinas + subwoofer, para que el sonido llegue parejo hasta el fondo.',
      lighting: '2 torres de luces, o torre + láser, para cubrir pista y salón.',
    },
  },
  {
    max: 300,
    tier: {
      label: 'Evento grande',
      audio: 'Refuerzo de sonido (line array) — el audio de una torre ya no alcanza a cubrir bien.',
      lighting: 'Kit completo: torres + LED RGB + láser, para que se note a distancia.',
    },
  },
  {
    max: Infinity,
    tier: {
      label: 'Evento masivo',
      audio: 'Sonido de gran formato — te recomendamos agendar una visita técnica.',
      lighting: 'Diseño de iluminación a la medida — escríbenos con el plano del lugar.',
    },
  },
]

export function audienceTierFor(guestCount: number): AudienceTier {
  return (TIERS.find((t) => guestCount <= t.max) ?? TIERS[TIERS.length - 1]).tier
}
