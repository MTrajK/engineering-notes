import { z } from 'zod'
import type { Category } from '../../../domain'

export function createCategorySchema(categories: Category[]) {
  return z.object({
    name: z
      .string()
      .trim()
      .min(1, 'Category name is required.')
      .refine(
        (value) =>
          !categories.some(
            (category) => category.name.toLowerCase() === value.toLowerCase(),
          ),
        'Category name must be unique.',
      ),
  })
}

export type CategoryFormValues = z.infer<ReturnType<typeof createCategorySchema>>
