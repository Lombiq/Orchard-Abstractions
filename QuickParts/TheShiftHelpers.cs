using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Utility;
using System.Linq;

namespace Orchard.ContentManagement
{
    public class ReflectionHelper<T>
    {
        private static readonly ConcurrentDictionary<string, Delegate> _getterCache =
            new ConcurrentDictionary<string, Delegate>();

        public delegate TProperty PropertyGetterDelegate<out TProperty>(T target);

        /// <summary>
        /// Gets property info out of a Lambda.
        /// </summary>
        /// <typeparam name="TProperty">The return type of the Lambda.</typeparam>
        /// <param name="expression">The Lambda expression.</param>
        /// <returns>The property info.</returns>
        public static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new InvalidOperationException("Expression is not a member expression.");
            }
            var propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("Expression is not for a property.");
            }
            return propertyInfo;
        }

        /// <summary>
        /// Gets a delegate from a property expression.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="targetExpression">The property expression.</param>
        /// <returns>The delegate.</returns>
        public static PropertyGetterDelegate<TProperty> GetGetter<TProperty>(
            Expression<Func<T, TProperty>> targetExpression)
        {

            var propertyInfo = GetPropertyInfo(targetExpression);
            return (PropertyGetterDelegate<TProperty>)_getterCache
                .GetOrAdd(propertyInfo.Name,
                    s => Delegate.CreateDelegate(typeof(PropertyGetterDelegate<TProperty>), propertyInfo.GetGetMethod()));
        }
    }

    public static class XmlHelper
    {
        /// <summary>
        /// Like Add, but chainable.
        /// </summary>
        /// <param name="el">The parent element.</param>
        /// <param name="children">The elements to add.</param>
        /// <returns>Itself</returns>
        public static XElement AddEl(this XElement el, params XElement[] children)
        {
            el.Add(children.Cast<object>());
            return el;
        }

        /// <summary>
        /// Gets the string value of an attribute, and null if the attribute doesn't exist.
        /// </summary>
        /// <param name="el">The element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The string value of the attribute if it exists, null otherwise.</returns>
        public static string Attr(this XElement el, string name)
        {
            var attr = el.Attribute(name);
            return attr == null ? null : attr.Value;
        }

        /// <summary>
        /// Gets a typed value from an attribute.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The attribute value</returns>
        public static T Attr<T>(this XElement el, string name)
        {

            var attr = el.Attribute(name);
            return attr == null ? default(T) : Parse<T>(attr.Value);
        }

        /// <summary>
        /// Sets an attribute value. This is chainable.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>Itself</returns>
        public static XElement Attr<T>(this XElement el, string name, T value)
        {
            el.SetAttributeValue(name, ToString(value));
            return el;
        }

        /// <summary>
        /// Returns the text contents of a child element.
        /// </summary>
        /// <param name="el">The parent element.</param>
        /// <param name="name">The name of the child element.</param>
        /// <returns>The text for the child element, and null if it doesn't exist.</returns>
        public static string El(this XElement el, string name)
        {
            var childElement = el.Element(name);
            return childElement == null ? null : childElement.Value;
        }

        /// <summary>
        /// Creates and sets the value of a child element. This is chainable.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="el">The parent element.</param>
        /// <param name="name">The name of the child element.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>Itself</returns>
        public static XElement El<T>(this XElement el, string name, T value)
        {
            el.SetElementValue(name, value);
            return el;
        }

        /// <summary>
        /// Sets a property value from an attribute of the same name.
        /// </summary>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <typeparam name="TProperty">The type of the target property</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="target">The target object.</param>
        /// <param name="targetExpression">The property expression.</param>
        /// <returns>Itself</returns>
        public static XElement FromAttr<TTarget, TProperty>(this XElement el, TTarget target,
            Expression<Func<TTarget, TProperty>> targetExpression)
        {

            if (target == null) return el;
            var propertyInfo = ReflectionHelper<TTarget>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;
            var attr = el.Attribute(name);

            if (attr == null) return el;
            propertyInfo.SetValue(target, el.Attr<TProperty>(name), null);
            return el;
        }

        /// <summary>
        /// Sets an attribute with the value of a property of the same name.
        /// </summary>
        /// <typeparam name="TTarget">The type of the object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="target">The object.</param>
        /// <param name="targetExpression">The property expression.</param>
        /// <returns>Itself</returns>
        public static XElement ToAttr<TTarget, TProperty>(this XElement el, TTarget target,
            Expression<Func<TTarget, TProperty>> targetExpression)
        {

            if (target == null) return el;
            var propertyInfo = ReflectionHelper<TTarget>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;
            var val = (TProperty)propertyInfo.GetValue(target, null);

            el.Attr(name, ToString(val));
            return el;
        }

        /// <summary>
        /// Gets the text value of an element as the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type to parse the element as.</typeparam>
        /// <param name="el">The element.</param>
        /// <returns>The value of the element as type TValue.</returns>
        public static TValue Val<TValue>(this XElement el)
        {
            return Parse<TValue>(el.Value);
        }

        /// <summary>
        /// Sets the value of an element.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to set.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="value">The value.</param>
        /// <returns>The element.</returns>
        public static XElement Val<TValue>(this XElement el, TValue value)
        {
            el.SetValue(ToString(value));
            return el;
        }

        /// <summary>
        /// Serializes the provided value as a string.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The string representation of the value.</returns>
        public static string ToString<T>(T value)
        {
            var type = typeof(T);
            if (type == typeof(string))
            {
                return Convert.ToString(value);
            }
            if ((!type.IsValueType || Nullable.GetUnderlyingType(type) != null) &&
                value == null &&
                type != typeof(string))
            {

                return "null";
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return XmlConvert.ToString(Convert.ToDateTime(value),
                    XmlDateTimeSerializationMode.Utc);
            }

            if (type == typeof(bool) ||
                type == typeof(bool?))
            {
                return Convert.ToBoolean(value) ? "true" : "false";
            }

            if (type == typeof(int) ||
                type == typeof(int?))
            {

                return Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(double) ||
                type == typeof(double?))
            {

                var doubleValue = (double)(object)value;
                if (double.IsPositiveInfinity(doubleValue))
                {
                    return "infinity";
                }
                if (double.IsNegativeInfinity(doubleValue))
                {
                    return "-infinity";
                }
                return doubleValue.ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(float) ||
                type == typeof(float?))
            {

                var floatValue = (float)(object)value;
                if (float.IsPositiveInfinity(floatValue))
                {
                    return "infinity";
                }
                if (float.IsNegativeInfinity(floatValue))
                {
                    return "-infinity";
                }
                return floatValue.ToString(CultureInfo.InvariantCulture);
            }

            if (type == typeof(decimal) ||
                type == typeof(decimal?))
            {

                var decimalValue = Convert.ToDecimal(value);
                return decimalValue.ToString(CultureInfo.InvariantCulture);
            }

            if (type.IsEnum)
            {
                return value.ToString();
            }

            throw new NotSupportedException(String.Format("Could not handle type {0}", type.Name));
        }

        /// <summary>
        /// Parses a string value as the provided type.
        /// </summary>
        /// <typeparam name="T">The destination type</typeparam>
        /// <param name="value">The string representation of the value to parse.</param>
        /// <returns>The parsed value with type T.</returns>
        public static T Parse<T>(string value)
        {
            var type = typeof(T);

            if (type == typeof(string))
            {
                return (T)(object)value;
            }
            if (value == null ||
                "null".Equals(value, StringComparison.Ordinal) &&
                ((!type.IsValueType || Nullable.GetUnderlyingType(type) != null)))
            {

                return default(T);
            }

            if ("infinity".Equals(value, StringComparison.Ordinal))
            {
                if (type == typeof(float) || type == typeof(float?)) return (T)(object)float.PositiveInfinity;
                if (type == typeof(double) || type == typeof(double?)) return (T)(object)double.PositiveInfinity;
                throw new NotSupportedException(String.Format("Infinity not supported for type {0}", type.Name));
            }
            if ("-infinity".Equals(value, StringComparison.Ordinal))
            {
                if (type == typeof(float)) return (T)(object)float.NegativeInfinity;
                if (type == typeof(double)) return (T)(object)double.NegativeInfinity;
                throw new NotSupportedException(String.Format("Infinity not supported for type {0}", type.Name));
            }
            if (type == typeof(int) || type == typeof(int?))
            {
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(bool) || type == typeof(bool?))
            {
                return (T)(object)value.Equals("true", StringComparison.Ordinal);
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return (T)(object)XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
            }
            if (type == typeof(double) || type == typeof(double?))
            {
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(float) || type == typeof(float?))
            {
                return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
            }
            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, value);
            }

            throw new NotSupportedException(String.Format("Could not handle type {0}", type.Name));
        }

        /// <summary>
        /// Gives context to an XElement, enabling chained property operations.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="el">The element.</param>
        /// <param name="context">The context.</param>
        /// <returns>The element with context.</returns>
        public static XElementWithContext<TContext> With<TContext>(this XElement el, TContext context)
        {
            return new XElementWithContext<TContext>(el, context);
        }

        /// <summary>
        /// A wrapper for XElement, with context, for strongly-typed manipulation
        /// of an XElement.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        public class XElementWithContext<TContext>
        {
            public XElementWithContext(XElement element, TContext context)
            {
                Element = element;
                Context = context;
            }

            public XElement Element { get; private set; }
            public TContext Context { get; private set; }

            public static implicit operator XElement(XElementWithContext<TContext> elementWithContext)
            {
                return elementWithContext.Element;
            }

            /// <summary>
            /// Replaces the current context with a new one, enabling chained action on different objects.
            /// </summary>
            /// <typeparam name="TNewContext">The type of the new context.</typeparam>
            /// <param name="context">The new context.</param>
            /// <returns>A new XElementWithContext, that has the new context.</returns>
            public XElementWithContext<TNewContext> With<TNewContext>(TNewContext context)
            {
                return new XElementWithContext<TNewContext>(Element, context);
            }

            /// <summary>
            /// Sets the value of a context property as an attribute of the same name on the element.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="targetExpression">The property expression.</param>
            /// <returns>Itself</returns>
            public XElementWithContext<TContext> ToAttr<TProperty>(
                Expression<Func<TContext, TProperty>> targetExpression)
            {
                Element.ToAttr(Context, targetExpression);
                return this;
            }

            /// <summary>
            /// Gets an attribute on the element and sets the property of the same name on the context with its value.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="targetExpression">The property expression.</param>
            /// <returns>Itself</returns>
            public XElementWithContext<TContext> FromAttr<TProperty>(
                Expression<Func<TContext, TProperty>> targetExpression)
            {
                Element.FromAttr(Context, targetExpression);
                return this;
            }

            /// <summary>
            /// Evaluates an attribute from an expression.
            /// It's a nice strongly-typed way to read attributes.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="expression">The property expression.</param>
            /// <returns>The attribute, ready to be cast.</returns>
            public TProperty Attr<TProperty>(Expression<Func<TContext, TProperty>> expression)
            {
                var propertyInfo = ReflectionHelper<TContext>.GetPropertyInfo(expression);
                var name = propertyInfo.Name;
                return Element.Attr<TProperty>(name);
            }
        }
    }

    public static class InfosetHelper
    {

        public static TProperty Retrieve<TPart, TProperty>(this TPart contentPart,
            Expression<Func<TPart, TProperty>> targetExpression,
            bool versioned = false) where TPart : ContentPart
        {

            var propertyInfo = ReflectionHelper<TPart>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;

            var infosetPart = contentPart.As<InfosetPart>();
            var el = infosetPart == null
                ? null
                : (versioned ? infosetPart.VersionInfoset.Element : infosetPart.Infoset.Element)
                .Element(contentPart.GetType().Name);
            return el == null ? default(TProperty) : el.Attr<TProperty>(name);
        }

        public static TProperty Retrieve<TProperty>(this ContentPart contentPart, string name,
            bool versioned = false)
        {
            var infosetPart = contentPart.As<InfosetPart>();
            var el = infosetPart == null
                ? null
                : (versioned ? infosetPart.VersionInfoset.Element : infosetPart.Infoset.Element)
                .Element(contentPart.GetType().Name);
            return el == null ? default(TProperty) : el.Attr<TProperty>(name);
        }

        public static void Store<TPart, TProperty>(this TPart contentPart,
            Expression<Func<TPart, TProperty>> targetExpression,
            TProperty value, bool versioned = false) where TPart : ContentPart
        {

            var partName = contentPart.GetType().Name;
            var infosetPart = contentPart.As<InfosetPart>();
            var propertyInfo = ReflectionHelper<TPart>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;

            Store(infosetPart, partName, name, value, versioned);
        }

        public static void Store<TProperty>(this ContentPart contentPart, string name,
            TProperty value, bool versioned = false)
        {

            var partName = contentPart.GetType().Name;
            var infosetPart = contentPart.As<InfosetPart>();

            Store(infosetPart, partName, name, value, versioned);
        }

        public static void Store<TProperty>(this InfosetPart infosetPart, string partName, string name, TProperty value, bool versioned = false)
        {

            var infoset = (versioned ? infosetPart.VersionInfoset : infosetPart.Infoset);
            var partElement = infoset.Element.Element(partName);
            if (partElement == null)
            {
                partElement = new XElement(partName);
                infoset.Element.Add(partElement);
            }
            partElement.Attr(name, value);
        }

        public static TProperty Retrieve<TPart, TRecord, TProperty>(this TPart contentPart,
            Expression<Func<TRecord, TProperty>> targetExpression)
            where TPart : ContentPart<TRecord>
        {

            var getter = ReflectionHelper<TRecord>.GetGetter(targetExpression);
            return contentPart.Retrieve(targetExpression, getter);
        }

        public static TProperty Retrieve<TPart, TRecord, TProperty>(this TPart contentPart,
            Expression<Func<TRecord, TProperty>> targetExpression,
            Delegate defaultExpression)
            where TPart : ContentPart<TRecord>
        {

            var propertyInfo = ReflectionHelper<TRecord>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;

            var infosetPart = contentPart.As<InfosetPart>();
            var el = infosetPart == null
                ? null
                : infosetPart.Infoset.Element.Element(contentPart.GetType().Name);
            if (el == null || el.Attribute(name) == null)
            {
                // Property has never been stored. Get it from the default expression and store that.
                var defaultValue = defaultExpression == null
                    ? default(TProperty)
                    : (TProperty)defaultExpression.DynamicInvoke(contentPart.Record);
                contentPart.Store(name, defaultValue);
                return defaultValue;
            }
            return el.Attr<TProperty>(name);
        }

        public static void Store<TPart, TRecord, TProperty>(this TPart contentPart,
            Expression<Func<TRecord, TProperty>> targetExpression,
            TProperty value)
            where TPart : ContentPart<TRecord>
        {

            var propertyInfo = ReflectionHelper<TRecord>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;
            propertyInfo.SetValue(contentPart.Record, value, null);
            contentPart.Store(name, value);
        }
    }
}
