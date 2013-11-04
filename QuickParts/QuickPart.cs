using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Environment.Extensions;

namespace Lombiq.Abstractions.QuickParts
{
    [OrchardFeature("Lombiq.Abstractions.QuickParts")]
    public abstract class QuickPart : ContentPart, IQuickPart
    {
        public void StoreInInfoset<T>(string name, T value)
        {
            this.As<InfosetPart>().Set(TypePartDefinition.PartDefinition.Name, name, XmlHelper.ToString(value));
        }

        public T RetrieveFromInfoset<T>(string name)
        {
            return XmlHelper.Parse<T>(this.As<InfosetPart>().Get(TypePartDefinition.PartDefinition.Name, name));
        }
    }
}