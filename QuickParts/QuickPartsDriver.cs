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
        private readonly IEnumerable<IQuickPart> _parts;
        private readonly IEnumerable<IQuickPartLogic> _logics;
        private readonly IShapeFactory _shapeFactory;


        public QuickPartsDriver(
            IEnumerable<IQuickPart> parts,
            IEnumerable<IQuickPartLogic> logics,
            IShapeFactory shapeFactory)
        {
            _parts = parts;
            _logics = logics;
            _shapeFactory = shapeFactory;
        }


        public void Discover(ShapeTableBuilder builder)
        {
            foreach (var part in _parts)
            {
                builder
                    .Describe(ShapeNameFromPart(part))
                    .Placement(ctx => new PlacementInfo { Location = "Content:5" });
            }
        }

        IEnumerable<ContentPartInfo> IContentPartDriver.GetPartInfo()
        {
            return _parts.Select(part => new ContentPartInfo
            {
                PartName = part.GetType().Name,
                Factory = typePartDefinition => part as ContentPart
            });
        }


        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper)
        {
            return Combined(
                _parts.Select(p =>
                {
                    var partType = p.GetType();

                    var shapeName = ShapeNameFromPart(p);

                    return ContentShape(shapeName,
                        () =>
                        {
                            var parameters = new Dictionary<string, object>();

                            parameters["ContentPart"] = p;

                            var logicInterface = typeof(IQuickPartLogic<>).MakeGenericType(p.GetType());
                            foreach (var logic in _logics.Where(l => logicInterface.IsAssignableFrom(l.GetType())))
                            {
                                var context = logic.GetType().InvokeMember("ComputeContext", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance, null, logic, new[] { p }) as IEnumerable<KeyValuePair<string, object>>;
                                if (context != null)
                                {
                                    foreach (var item in context)
                                    {
                                        parameters[item.Key] = item.Value;
                                    }
                                }
                            }

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


        private static string ShapeNameFromPart(IQuickPart part)
        {
            var partName = part.GetType().Name;
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