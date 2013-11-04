using Lombiq.Abstractions.QuickParts;
using Orchard.Environment.Extensions;

namespace Lombiq.Abstractions.QuickWidgets
{
    [OrchardFeature("Lombiq.Abstractions.QuickWidgets")]
    public abstract class QuickWidgetPart : QuickPart, IQuickWidget
    {
    }
}