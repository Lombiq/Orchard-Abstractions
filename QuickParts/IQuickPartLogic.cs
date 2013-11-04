using System.Collections.Generic;
using Orchard;

namespace Lombiq.Abstractions.QuickParts
{
    public interface IQuickPartLogic : IDependency
    {
    }

    public interface IQuickPartLogic<T> : IQuickPartLogic
        where T : IQuickPart
    {
        // TODO: How will generic methods be called? What about using the event bus (no real type information available then!)?
        IEnumerable<KeyValuePair<string, object>> ComputeDisplayParameters(T part);
    }
}
