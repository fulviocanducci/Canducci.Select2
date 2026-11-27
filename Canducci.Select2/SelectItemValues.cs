using System.Collections.Generic;

namespace Canducci.Select2
{
    /// <summary>
    /// Defines the identifier values used to mark select items as selected or disabled.
    /// </summary>
    /// <typeparam name="TId">The type of the select item identifier.</typeparam>
    public sealed class SelectItemValues<TId>
    {
        /// <summary>
        /// Gets or sets the identifiers of the items that should be selected.
        /// </summary>
        public IEnumerable<TId> SelectedValues { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the items that should be disabled.
        /// </summary>
        public IEnumerable<TId> DisabledValues { get; set; }

        /// <summary>
        /// Creates a new instance with the identifiers of the items that should be selected.
        /// </summary>
        /// <param name="selectedValues">The selected item identifiers.</param>
        /// <returns>A new instance containing the selected item identifiers.</returns>
        public static SelectItemValues<TId> Create(IEnumerable<TId> selectedValues)
        {
            return new SelectItemValues<TId>
            {
                SelectedValues = selectedValues
            };
        }

        /// <summary>
        /// Creates a new instance with the identifiers of the items that should be selected or disabled.
        /// </summary>
        /// <param name="selectedValues">The selected item identifiers.</param>
        /// <param name="disabledValues">The disabled item identifiers.</param>
        /// <returns>A new instance containing the selected and disabled item identifiers.</returns>
        public static SelectItemValues<TId> Create(IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues)
        {
            return new SelectItemValues<TId>
            {
                SelectedValues = selectedValues,
                DisabledValues = disabledValues
            };
        }

        /// <summary>
        /// Sets the identifiers of the items that should be selected.
        /// </summary>
        /// <param name="values">The selected item identifiers.</param>
        /// <returns>The current instance, allowing fluent configuration.</returns>
        public SelectItemValues<TId> SetSelectValues(IEnumerable<TId> values)
        {
            SelectedValues = values;
            return this;
        }

        /// <summary>
        /// Sets the identifiers of the items that should be disabled.
        /// </summary>
        /// <param name="values">The disabled item identifiers.</param>
        /// <returns>The current instance, allowing fluent configuration.</returns>
        public SelectItemValues<TId> SetDisabledValues(IEnumerable<TId> values)
        {
            DisabledValues = values;
            return this;
        }
    }
}
