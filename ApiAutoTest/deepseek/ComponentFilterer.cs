// ComponentFilterer.cs
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestAutomationEngine.Core
{
    public static class ComponentFilterer
    {
        /// <summary>
        /// Applies component filters to a container, removing components that do NOT match any filter.
        /// If filters are empty, no filtering occurs.
        /// The operation modifies the container in‑place and returns whether any components remain.
        /// </summary>
        public static bool ApplyFilters(IContainerComponent container, List<ComponentFilter> filters)
        {
            if (filters == null || filters.Count == 0)
                return true; // No filtering requested

            // We need mutable access – all container components in the engine derive from ContainerComponentBase
            if (!(container is ContainerComponentBase mutableContainer))
            {
                // Should never happen in practice; but if it does, we cannot modify.
                return container.Children.Count > 0;
            }

            var childrenToKeep = new List<ITestComponent>();

            foreach (var child in mutableContainer.Children)
            {
                bool matches = MatchesAnyFilter(child, filters);

                if (child is IContainerComponent childContainer)
                {
                    // Recursively filter the child container
                    bool childHasRemainingChildren = ApplyFilters(childContainer, filters);
                    // Keep the container if it matches directly OR has remaining children after filtering
                    if (matches || childHasRemainingChildren)
                    {
                        childrenToKeep.Add(child);
                    }
                }
                else
                {
                    // Leaf component: keep only if it matches
                    if (matches)
                        childrenToKeep.Add(child);
                }
            }

            // Replace the children collection
            mutableContainer.Children.Clear();
            mutableContainer.Children.AddRange(childrenToKeep);

            return mutableContainer.Children.Count > 0;
        }

        private static bool MatchesAnyFilter(ITestComponent component, List<ComponentFilter> filters)
        {
            foreach (var filter in filters)
            {
                // Match by GUID
                if (!string.IsNullOrEmpty(filter.Guid))
                {
                    if (component.Guid.ToString().Equals(filter.Guid, System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                // Match by Name
                if (!string.IsNullOrEmpty(filter.Name))
                {
                    if (filter.UseRegex)
                    {
                        if (Regex.IsMatch(component.Name, filter.Name))
                            return true;
                    }
                    else
                    {
                        if (component.Name.Equals(filter.Name, System.StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}