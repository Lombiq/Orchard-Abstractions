using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;

namespace Lombiq.Abstractions.QuickParts
{
    public interface IQuickPartsManager : ISingletonDependency
    {
        IEnumerable<string> GetPartNames();
        QuickPart Factory(string quickPartName);
        IDictionary<string, object> ComputeDisplayParameters(QuickPart part);
    }
}
