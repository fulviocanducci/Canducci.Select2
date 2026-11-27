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
