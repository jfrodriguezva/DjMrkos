import type { ReactNode } from 'react'

const PATHS: Record<string, ReactNode> = {
  users: (
    <>
      <circle cx="8.5" cy="8" r="2.6" />
      <path d="M3.5 18c0-3 2.2-5 5-5s5 2 5 5" />
      <circle cx="16.2" cy="8.6" r="2.1" />
      <path d="M14.8 13.2c2.3.2 4 1.9 4.7 4.8" />
    </>
  ),
  mixer: (
    <>
      <path d="M6 4v16M12 4v16M18 4v16" />
      <circle cx="6" cy="15" r="1.7" />
      <circle cx="12" cy="8" r="1.7" />
      <circle cx="18" cy="11" r="1.7" />
    </>
  ),
  lightbulb: (
    <>
      <path d="M9 18h6M10 21h4" />
      <path d="M12 3a6 6 0 0 0-3.4 10.9c.6.5.9 1.2.9 2.1h5a2.6 2.6 0 0 1 .9-2.1A6 6 0 0 0 12 3Z" />
    </>
  ),
  sparkles: (
    <>
      <path d="M12 3v5M12 16v5M3 12h5M16 12h5" />
      <path d="M12 8c0 2.2-1.8 4-4 4 2.2 0 4 1.8 4 4 0-2.2 1.8-4 4-4-2.2 0-4-1.8-4-4Z" />
    </>
  ),
  monitor: (
    <>
      <rect x="3" y="4.5" width="18" height="12" rx="1.4" />
      <path d="M9 20.5h6M12 16.5v4" />
    </>
  ),
  speaker: (
    <>
      <rect x="6" y="2.5" width="12" height="19" rx="2" />
      <circle cx="12" cy="8" r="2.2" />
      <circle cx="12" cy="15" r="3.4" />
    </>
  ),
  mask: (
    <>
      <path d="M4 9c0-3.9 3.6-6.5 8-6.5S20 5.1 20 9c0 5-3.2 10-8 10S4 14 4 9Z" />
      <path d="M8 9.5c0-1 .8-1.5 1.7-1.5s1.7.5 1.7 1.5M12.6 9.5c0-1 .8-1.5 1.7-1.5s1.7.5 1.7 1.5M9.5 14c1.6 1 3.4 1 5 0" />
    </>
  ),
}

/** Maps a module's `icon` slug (set from the admin panel) to a small line-art glyph. */
export function ModuleIcon({ name, size = 20 }: { name: string | null; size?: number }) {
  const path = (name && PATHS[name]) || null

  if (!path) {
    return <span aria-hidden="true">{'✦'}</span>
  }

  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      {path}
    </svg>
  )
}
