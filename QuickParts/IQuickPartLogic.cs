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
        /// be used in the display shape template.
        /// </summary>
        /// <param name="part">The part object</param>
        /// <param name="shape">The display shape to add parameters to</param>
        void ComputeDisplayParameters(TPart part, dynamic shape);
    }
}
