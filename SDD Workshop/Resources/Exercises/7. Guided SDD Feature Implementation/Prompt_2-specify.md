/speckit.specify Add CSV export to the Expenses page filter area.

Add an Export CSV button as part of the existing ExpensesFilters component on the Expenses page.

When the user clicks Export CSV, the app should download a CSV file containing all expenses that match the currently selected filter options. The export must include the full filtered result set, not only the currently visible pagination page.

The CSV file must use this column format:

Name,Category,Date,Amount