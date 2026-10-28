import styles from './PagePlaceholder.module.css'

interface PagePlaceholderProps {
  description: string
  title: string
}

export function PagePlaceholder({
  description,
  title,
}: PagePlaceholderProps) {
  return (
    <section className={styles.page}>
      <div className={styles.card}>
        <p className={styles.kicker}>Placeholder page</p>
        <h1 className={styles.title}>{title}</h1>
        <p className={styles.description}>{description}</p>
      </div>
    </section>
  )
}
