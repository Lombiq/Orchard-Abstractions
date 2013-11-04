using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Environment;
using Orchard.Core.Contents.Extensions;
using Orchard.Environment.Extensions;
using Lombiq.Abstractions.QuickParts;

namespace Lombiq.Abstractions.QuickWidgets
{
    [OrchardFeature("Lombiq.Abstractions.QuickWidgets")]
    public class QuickWidgetsSetup : IOrchardShellEvents
    {
        private readonly IQuickPartsManager _quickPartsManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;


        public QuickWidgetsSetup(
            IQuickPartsManager quickPartsManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            _quickPartsManager = quickPartsManager;
            _contentDefinitionManager = contentDefinitionManager;
        }


        public void Activated()
        {
            foreach (var widgetPart in _quickPartsManager.GetPartTypes().Where(type => typeof(IQuickWidget).IsAssignableFrom(type)))
            {
                var partName = widgetPart.Name;
                if (partName.EndsWith("Part")) partName = partName.Substring(0, partName.Length - 4);
                var widgetName = partName + "Widget";

                var widgetDefinition = _contentDefinitionManager.GetTypeDefinition(widgetName);
                if (widgetDefinition == null)
                {
                    _contentDefinitionManager.AlterTypeDefinition(widgetName,
                        cfg => cfg
                            .WithPart("WidgetPart")
                            .WithPart("CommonPart")
                            .WithPart(widgetPart.Name)
                            .WithSetting("Stereotype", "Widget")
                        );
                }
            }
        }

        public void Terminating()
        {
        }
    }
}