import styles from './AboutPage.module.css'

const OBJECTIVES = [
  'Ofrecer un catálogo de servicios claro y configurable — desde audio e iluminación hasta efectos especiales, pantallas y animación — adaptable a cualquier tipo de evento, sin depender de cambios de código.',
  'Digitalizar la experiencia del invitado mediante solicitud de canciones por código QR, con entrega en tiempo real al DJ durante el evento.',
  'Mantener un estándar técnico consistente — audio sin distorsión, iluminación sincronizada — verificable en el 100% de los eventos contratados.',
  'Convertir visitas del portal en cotizaciones agendadas, con un flujo de agendado, cotización y contratación simple de principio a fin.',
  'Construir reputación medible mediante testimonios, calificaciones y portafolio audiovisual por tipo de servicio.',
  'Escalar el inventario de equipo — audio, iluminación, efectos, pantallas y personajes — reflejándolo dinámicamente en el portal según se amplía el negocio.',
]

export function AboutPage() {
  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Quiénes somos</span>
          <h1>DJ MrKos</h1>
          <p className={styles.role}>DJ y productor musical</p>
          <p className={styles.motto}>"El pulso de tu evento."</p>
        </div>
      </div>

      <div className="container section">
        <div className={styles.grid2}>
          <div className={`card ${styles.panel}`}>
            <h2>Misión</h2>
            <p>
              Elevar cada evento a una experiencia sonora y visual inolvidable, combinando audio impecable, iluminación de
              alto impacto y una producción integral — desde el staff y los efectos especiales hasta la animación — con un
              trato cercano y una ejecución cuidada en cada detalle, desde la primera cotización hasta el último tema de la
              noche.
            </p>
          </div>
          <div className={`card ${styles.panel}`}>
            <h2>Visión</h2>
            <p>
              Ser el proveedor de referencia en producción de eventos — audio, iluminación y experiencias en vivo — reconocido
              por fusionar talento y tecnología — como la solicitud de canciones en vivo por QR — para crear experiencias que
              la gente recuerda, comparte y recomienda.
            </p>
          </div>
        </div>

        <div className={`card ${styles.panel}`} style={{ marginTop: 16 }}>
          <h2>Objetivos</h2>
          <ul className={styles.objectives} style={{ marginTop: 16 }}>
            {OBJECTIVES.map((text, i) => (
              <li key={i}>
                <span className={styles.objectiveNumber}>{String(i + 1).padStart(2, '0')}</span>
                <p>{text}</p>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </>
  )
}
