using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Environment;
using Orchard.Environment.Extensions;

namespace Lombiq.Abstractions.QuickParts
{
    [OrchardFeature("Lombiq.Abstractions.QuickParts")]
    public class QuickPartsSetup : IOrchardShellEvents
    {
        private readonly IQuickPartsManager _quickPartsManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public QuickPartsSetup(
            IQuickPartsManager quickPartsManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            _quickPartsManager = quickPartsManager;
            _contentDefinitionManager = contentDefinitionManager;
        }


        public void Activated()
        {
            foreach (var partName in _quickPartsManager.GetPartNames())
            {
                var partDefinition = _contentDefinitionManager.GetPartDefinition(partName);
                if (partDefinition == null)
                {
                    _contentDefinitionManager.AlterPartDefinition(partName, builder => builder.Attachable());
                }
            }
        }

        public void Terminating()
        {
        }
    }
}