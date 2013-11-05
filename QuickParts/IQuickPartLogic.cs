using System.Collections.Generic;
using Orchard;

namespace Lombiq.Abstractions.QuickParts
{
    /// <summary>
    /// Describes a background logic for quick parts.
    /// </summary>
    public interface IQuickPartLogic : IDependency
    {
    }

    /// <summary>
    /// Describes a background logic for a specific quick part type.
    /// </summary>
    /// <typeparam name="TPart">The type of the quick part</typeparam>
    public interface IQuickPartLogic<TPart> : IQuickPartLogic
        where TPart : IQuickPart
    {
        /// <summary>
        /// When the part is displayed, this method is called on every corresponding logic to compute additional parameters for
        /// be used in the display template.
        /// </summary>
        /// <param name="part">The part object</param>
        /// <returns>The collection of parameters that will be reachable from the template</returns>
        IEnumerable<KeyValuePair<string, object>> ComputeDisplayParameters(TPart part);
    }
}
