﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Web;
using Castle.Core.Interceptor;
using Lombiq.Abstractions.QuickParts;
using Orchard;
using Orchard.Environment.Extensions;
using Castle.DynamicProxy;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Lombiq.Abstractions.QuickParts
{
    [OrchardFeature("Lombiq.Abstractions.QuickParts")]
    public class QuickPartsManager : IQuickPartsManager
    {
        private readonly IEnumerable<IQuickPart> _parts;
        private readonly IEnumerable<IQuickPartLogic> _logics;

        private readonly ProxyGenerator _proxyGenerator;
        private readonly StorageInterceptor _storageInterceptor;


        public QuickPartsManager(
            IEnumerable<IQuickPart> parts,
            IEnumerable<IQuickPartLogic> logics)
        {
            _parts = parts;
            _logics = logics;

            _proxyGenerator = new ProxyGenerator();
            _storageInterceptor = new StorageInterceptor();
        }


        public IEnumerable<string> GetPartNames()
        {
            return _parts.Select(part => part.GetType().Name);
        }

        public QuickPart Factory(string quickPartName)
        {
            var part = _parts.Where(p => p.GetType().Name == quickPartName).FirstOrDefault();

            if (part == null) throw new InvalidOperationException("There is no QuickPart with the name " + quickPartName + " registered.");

            return (QuickPart)_proxyGenerator.CreateClassProxy(part.GetType(), _storageInterceptor);
        }

        public IDictionary<string, object> ComputeDisplayParameters(QuickPart part)
        {
            var parameters = new Dictionary<string, object>();

            var logicInterface = typeof(IQuickPartLogic<>).MakeGenericType(part.GetType().BaseType);
            foreach (var logic in _logics.Where(l => logicInterface.IsAssignableFrom(l.GetType())))
            {
                var context = logic.GetType().InvokeMember("ComputeDisplayParameters", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance, null, logic, new[] { part }) as IEnumerable<KeyValuePair<string, object>>;
                if (context != null)
                {
                    foreach (var item in context)
                    {
                        parameters[item.Key] = item.Value;
                    }
                }
            }

            return parameters;
        }


        [Serializable]
        public class StorageInterceptor : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                // Only catch calls to the developer's implementations.
                if (invocation.Method.DeclaringType.FullName != "Orchard.ContentManagement.ContentPart")
                {
                    var part = (QuickPart)invocation.InvocationTarget;
                    var partType = part.GetType();
                    var infosetPart = part.As<InfosetPart>();
                    
                    // Is set accessor? Taken from http://stackoverflow.com/a/7819577/220230
                    if (invocation.Method.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == invocation.Method))
                    {
                        var propertyName = invocation.Method.Name.Substring(4);
                        var storeMethod = partType.GetMethod("StoreInInfoset");
                        var genericStoreMethod = storeMethod.MakeGenericMethod(partType.GetProperty(propertyName).PropertyType);
                        genericStoreMethod.Invoke(part, new object[] { propertyName, invocation.Arguments[0] });
                    }
                    // Is get accessor?
                    else if (invocation.Method.DeclaringType.GetProperties().Any(prop => prop.GetGetMethod() == invocation.Method))
                    {
                        var propertyName = invocation.Method.Name.Substring(4);
                        var retrieveMethod = partType.GetMethod("RetrieveFromInfoset");
                        var genericRetrieveMethod = retrieveMethod.MakeGenericMethod(partType.GetProperty(propertyName).PropertyType);
                        invocation.ReturnValue = genericRetrieveMethod.Invoke(part, new object[] { propertyName });
                    }
                    else
                    {
                        invocation.Proceed();
                    }
                }
                else
                {
                    invocation.Proceed();
                }
            }
        }
    }
}