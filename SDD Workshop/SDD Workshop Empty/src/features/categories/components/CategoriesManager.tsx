import { zodResolver } from '@hookform/resolvers/zod'
import { X } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useAppDispatch, useAppState } from '../../../app/useAppContext'
import type { Category } from '../../../domain'
import { CategoryDialog } from './CategoryDialog'
import styles from './CategoriesManager.module.css'
import {
  createCategorySchema,
  type CategoryFormValues,
} from '../schemas/categorySchema'
import {
  canDeleteCategory,
  getNextCategoryColor,
} from '../utils/categoryRules'

type DialogState =
  | { category: Category; type: 'confirm-delete' }
  | { category: Category; type: 'used-category' }
  | null

export function CategoriesManager() {
  const { categories, expenses } = useAppState()
  const dispatch = useAppDispatch()
  const [dialogState, setDialogState] = useState<DialogState>(null)

  const schema = useMemo(() => createCategorySchema(categories), [categories])

  const {
    formState: { errors, isValid },
    handleSubmit,
    register,
    reset,
  } = useForm<CategoryFormValues>({
    defaultValues: {
      name: '',
    },
    mode: 'onChange',
    resolver: zodResolver(schema),
  })

  const onSubmit = handleSubmit((values) => {
    const trimmedName = values.name.trim()

    dispatch({
      type: 'category/add',
      payload: {
        id: crypto.randomUUID(),
        name: trimmedName,
        color: getNextCategoryColor(categories),
        createdAt: new Date().toISOString(),
      },
    })

    reset({ name: '' })
  })

  const handleDeleteRequest = (category: Category) => {
    if (canDeleteCategory(category.id, expenses)) {
      setDialogState({ category, type: 'confirm-delete' })
      return
    }

    setDialogState({ category, type: 'used-category' })
  }

  const handleDeleteConfirm = () => {
    if (dialogState?.type !== 'confirm-delete') {
      return
    }

    dispatch({
      type: 'category/delete',
      payload: { categoryId: dialogState.category.id },
    })
    setDialogState(null)
  }

  return (
    <>
      <section className={styles.page}>
        <div className={styles.hero}>
          <div>
            <p className={styles.kicker}>Category management</p>
            <h1 className={styles.title}>Categories</h1>
            <p className={styles.description}>
              Add custom categories and manage the ones already attached to your
              expense data.
            </p>
          </div>

          <form className={styles.form} onSubmit={onSubmit}>
            <label className={styles.field}>
              <span className={styles.fieldLabel}>Category name</span>
              <input
                {...register('name')}
                aria-invalid={errors.name ? 'true' : 'false'}
                aria-describedby={errors.name ? 'category-name-error' : undefined}
                className={styles.input}
                placeholder="Add a category"
              />
            </label>
            <button type="submit" className={styles.addButton} disabled={!isValid}>
              Add
            </button>
          </form>
          {errors.name ? (
            <p id="category-name-error" className={styles.errorMessage}>
              {errors.name.message}
            </p>
          ) : null}
        </div>

        <section className={styles.panel} aria-labelledby="category-list-title">
          <h2 id="category-list-title" className={styles.panelTitle}>
            Category Pills
          </h2>
          {categories.length > 0 ? (
            <div className={styles.pills}>
              {categories.map((category) => (
                <div key={category.id} className={styles.pill}>
                  <span
                    className={styles.colorSwatch}
                    style={{ backgroundColor: category.color }}
                    aria-hidden="true"
                  />
                  <span>{category.name}</span>
                  <button
                    type="button"
                    className={styles.deleteButton}
                    aria-label={`Delete ${category.name}`}
                    onClick={() => handleDeleteRequest(category)}
                  >
                    <X aria-hidden="true" className={styles.deleteIcon} />
                  </button>
                </div>
              ))}
            </div>
          ) : (
            <p className={styles.emptyState}>No categories available yet.</p>
          )}
        </section>
      </section>

      {dialogState?.type === 'confirm-delete' ? (
        <CategoryDialog
          title="Delete the category?"
          description={`Remove "${dialogState.category.name}" from your category list.`}
          confirmLabel="Yes"
          onClose={() => setDialogState(null)}
          onConfirm={handleDeleteConfirm}
        />
      ) : null}

      {dialogState?.type === 'used-category' ? (
        <CategoryDialog
          title="The category can not be deleted, it's used by at least one expense."
          description={`"${dialogState.category.name}" is linked to existing expenses and must stay available.`}
          confirmLabel="Ok"
          onClose={() => setDialogState(null)}
        />
      ) : null}
    </>
  )
}
