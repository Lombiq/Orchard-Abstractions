using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;

namespace Lombiq.Abstractions.QuickParts
{
    [OrchardFeature("Lombiq.Abstractions.QuickParts")]
    public class QuickPartsDriver : ContentPartDriver<ContentPart>, IShapeTableProvider, IContentPartDriver
    {
        private readonly IQuickPartsManager _quickPartsManager;
        private readonly IShapeFactory _shapeFactory;


        public QuickPartsDriver(
            IQuickPartsManager quickPartsManager,
            IShapeFactory shapeFactory)
        {
            _quickPartsManager = quickPartsManager;
            _shapeFactory = shapeFactory;
        }


        public void Discover(ShapeTableBuilder builder)
        {
            var placement = new PlacementInfo { Location = "Content:5" };

            foreach (var partName in _quickPartsManager.GetPartNames())
            {
                builder
                    .Describe(ShapeNameForPartName(partName))
                    .Placement(ctx => ctx.DisplayType == "Detail", placement);

                builder
                    .Describe(ShapeNameForPartName(partName) + "_Edit")
                    .Placement(ctx => placement);
            }
        }

        IEnumerable<ContentPartInfo> IContentPartDriver.GetPartInfo()
        {
            return _quickPartsManager.GetPartNames().Select(partName => new ContentPartInfo
            {
                PartName = partName,
                Factory = typePartDefinition =>
                    {
                        var part = _quickPartsManager.Factory(typePartDefinition.PartDefinition.Name);
                        part.TypePartDefinition = typePartDefinition;
                        return part;
                    }
            });
        }


        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper)
        {
            return Combined(
                GetQickParts(part).Select(p =>
                {
                    var shapeName = ShapeNameForPart(p);
                    
                    return ContentShape(shapeName,
                        () =>
                        {
                            var parameters = new Dictionary<string, object>(_quickPartsManager.ComputeDisplayParameters((QuickPart)p));

                            parameters["ContentPart"] = p;

                            return _shapeFactory.Create(shapeName, new ShapeParams(parameters));
                        });
                }).ToArray()
            );
        }

        protected override DriverResult Editor(ContentPart part, dynamic shapeHelper)
        {
            return Combined(
                GetQickParts(part).Select(p =>
                {
                    var shapeName = ShapeNameForPart(p);

                    return ContentShape(shapeName + "_Edit",
                        () => shapeHelper.EditorTemplate(
                                    TemplateName: shapeName.Replace("Parts_", "Parts/").Replace('_', '.'),
                                    Model: p,
                                    Prefix: p.TypePartDefinition.PartDefinition.Name));
                }).ToArray()
            );
        }

        protected override DriverResult Editor(ContentPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            foreach (var p in GetQickParts(part))
            {
                updater.TryUpdateModel((dynamic)p, p.TypePartDefinition.PartDefinition.Name, null, null);
            }

            return Editor(part, shapeHelper);
        }


        private static string ShapeNameForPart(ContentPart part)
        {
            return ShapeNameForPartName(part.TypePartDefinition.PartDefinition.Name);
        }

        private static string ShapeNameForPartName(string partName)
        {
            if (partName.EndsWith("Part")) partName = partName.Substring(0, partName.Length - 4);

            return "Parts_" + partName;
        }

        private static IEnumerable<ContentPart> GetQickParts(ContentPart part)
        {
            return part.ContentItem.Parts.Where(p => typeof(QuickPart).IsAssignableFrom(p.GetType()));
        }


        private class ShapeParams : INamedEnumerable<object>
        {
            public IEnumerable<object> Positional
            {
                get { return Enumerable.Empty<object>(); }
            }

            private readonly IDictionary<string, object> _named;
            public IDictionary<string, object> Named
            {
                get { return _named; }
            }


            public ShapeParams(IDictionary<string, object> parameters)
            {
                _named = parameters;
            }


            public IEnumerator<object> GetEnumerator()
            {
                return _named.Values.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}