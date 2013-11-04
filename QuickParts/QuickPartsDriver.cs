using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
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
            foreach (var partName in _quickPartsManager.GetPartNames())
            {
                builder
                    .Describe(ShapeNameFromPartName(partName))
                    .Placement(ctx => new PlacementInfo { Location = "Content:5" });
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
                part.ContentItem.Parts.Where(p => typeof(QuickPart).IsAssignableFrom(p.GetType())).Select(p =>
                {
                    var shapeName = ShapeNameFromPart(p);

                    return ContentShape(shapeName,
                        () =>
                        {
                            var quickPart = p;

                            var parameters = new Dictionary<string, object>(_quickPartsManager.ComputeDisplayParameters((QuickPart)quickPart));

                            parameters["ContentPart"] = quickPart;

                            return _shapeFactory.Create(shapeName, new ShapeParams(parameters));
                        });
                }).ToArray()
            );
        }

        protected override DriverResult Editor(ContentPart part, dynamic shapeHelper)
        {
            // TODO: Changing prefix before every ContentShape call
            return ContentShape("Parts_Content_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts.Content",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(ContentPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }


        private static string ShapeNameFromPart(ContentPart part)
        {
            return ShapeNameFromPartName(part.TypePartDefinition.PartDefinition.Name);
        }

        private static string ShapeNameFromPartName(string partName)
        {
            if (partName.EndsWith("Part")) partName = partName.Substring(0, partName.Length - 4);

            return "Parts_" + partName;
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