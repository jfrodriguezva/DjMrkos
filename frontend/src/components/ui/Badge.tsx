import type { ReactNode } from 'react'

interface BadgeProps {
  variant?: 'default' | 'live' | 'success' | 'muted'
  children: ReactNode
}

export function Badge({ variant = 'default', children }: BadgeProps) {
  const classes = ['badge', variant !== 'default' ? `badge--${variant}` : ''].filter(Boolean).join(' ')
  return <span className={classes}>{children}</span>
}
