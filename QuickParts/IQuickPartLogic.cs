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
        IEnumerable<KeyValuePair<string, object>> ComputeDisplayParameters(T part);
    }
}
