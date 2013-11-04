using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;

namespace Lombiq.Abstractions.QuickParts
{
    public interface IQuickPartsManager : ISingletonDependency
    {
        IEnumerable<Type> GetPartTypes();
        QuickPart Factory(string quickPartName);
        IDictionary<string, object> ComputeDisplayParameters(QuickPart part);
    }


    public static class QuickPartsManagerExtensions
    {
        public static IEnumerable<string> GetPartNames(this IQuickPartsManager quickPartsManager)
        {
            return quickPartsManager.GetPartTypes().Select(type => type.Name);
        } 
    }
}
