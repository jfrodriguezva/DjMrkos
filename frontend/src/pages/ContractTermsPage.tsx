import { Link } from 'react-router-dom'
import styles from './ContractTermsPage.module.css'

const CLAUSES = [
  'El PRESTADOR se compromete a brindar los servicios técnicos y de entretenimiento detallados en el presente documento, en la fecha y lugar acordados.',
  'El CLIENTE deberá cubrir el anticipo estipulado para reservar la fecha. El saldo restante deberá ser liquidado a más tardar al inicio del evento.',
  'En caso de cancelación por parte del CLIENTE, el anticipo no será reembolsable bajo ninguna circunstancia, considerándose como indemnización por bloqueo de fecha.',
  'Si el evento se extiende más allá del horario contratado, cada hora extra será cobrada según la tarifa vigente estipulada por el PRESTADOR.',
  'El CLIENTE es responsable de proveer una toma de corriente eléctrica estable y exclusiva para el equipo, así como un espacio seguro para el montaje.',
  'El PRESTADOR no se hace responsable por fallas eléctricas del lugar o cortes de energía ajenos a su equipo.',
  'Cualquier daño físico o avería al equipo de audio o iluminación causado por invitados, accidentes o mal manejo del lugar será cubierto al 100% por el CLIENTE.',
  'El PRESTADOR y su personal operativo deberán recibir alimentos y bebidas no alcohólicas (staff meal) durante el evento, a cargo del CLIENTE.',
  'Ninguna persona ajena al staff del PRESTADOR podrá manipular el equipo de sonido, iluminación o cabina de DJ.',
  'En caso fortuito o de fuerza mayor (desastres naturales, contingencias) que impidan la realización del evento, las partes acordarán una nueva fecha sin penalización.',
  'El PRESTADOR se reserva el derecho de tomar fotografías y videos cortos del montaje y ambiente para uso exclusivo en su portafolio y redes sociales.',
  'Para la interpretación y cumplimiento de este contrato, ambas partes se someten a la jurisdicción de las leyes aplicables vigentes.',
]

export function ContractTermsPage() {
  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Contratación</span>
          <h1>Cláusulas del contrato</h1>
          <p className={styles.lead}>
            Estas son las condiciones que forman parte de todo contrato con DJ MrKos. Léelas antes de reservar: se incluyen al final del contrato
            formal que firmamos contigo.
          </p>
        </div>
      </div>

      <div className="container section">
        <ol className={`card ${styles.clauses}`}>
          {CLAUSES.map((text, i) => (
            <li key={i}>
              <span className={styles.number}>Cláusula {i + 1}</span>
              <p>{text}</p>
            </li>
          ))}
        </ol>

        <p className={styles.footnote}>
          ¿Dudas sobre alguna cláusula? <Link to="/contacto">Escríbenos</Link> antes de <Link to="/contratar">contratar</Link>.
        </p>
      </div>
    </>
  )
}
