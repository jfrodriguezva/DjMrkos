import styles from './Equalizer.module.css'

const BAR_PROFILE = [0.4, 0.75, 1, 0.55, 0.85, 0.35, 0.65]

/** The brand's signature mark — also doubles as a loading indicator. */
export function Equalizer({ size = 'md' }: { size?: 'md' | 'sm' }) {
  const height = size === 'sm' ? 22 : 40
  return (
    <div className={styles.eq} style={{ height }} aria-hidden="true">
      {BAR_PROFILE.map((h, i) => (
        <span key={i} className={styles.bar} style={{ height: `${h * 100}%`, animationDelay: `${-i * 0.22}s` }} />
      ))}
    </div>
  )
}
