#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Models.EntityFramework;

#endregion

namespace SqdcWatcher
{
    public class SpecificationCopier
    {
        /// <summary>
        /// </summary>
        /// <param name="targetInstance"></param>
        /// <param name="specifications"></param>
        /// <returns>All the specifications that were copied.</returns>
        public static List<string> CopySpecificationsToObject(
            object targetInstance,
            List<SpecificationAttribute> specifications)
        {
            var copiedPropertyNames = new List<string>();
            Type type = targetInstance.GetType();
            Dictionary<string, PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetSetMethod() != null)
                .ToDictionary(p => p.Name);
            foreach (SpecificationAttribute spec in specifications)
            {
                if (properties.TryGetValue(spec.PropertyName, out PropertyInfo propInfo))
                {
                    propInfo.SetValue(targetInstance, Convert.ChangeType(spec.Value, propInfo.PropertyType));
                    copiedPropertyNames.Add(spec.PropertyName);
                }
            }

            return copiedPropertyNames;
        }
    }
}