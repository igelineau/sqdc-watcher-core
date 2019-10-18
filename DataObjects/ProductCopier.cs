using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.DataObjects
{
    public class ProductCopier
    {
        public static bool CopyAllSimpleProperties(Product destination, ProductDto source)
        {
            bool hasChanged = false;
            IEnumerable<PropertyInfo> publicProperties = typeof(Product)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pInfo => pInfo.GetSetMethod() != null);
            foreach(PropertyInfo propInfo in publicProperties)
            {
                hasChanged = hasChanged || CopyPropertyValue(source, destination, propInfo);
            }

            return hasChanged;
        }

        
        public static bool CopyPropertyValue(ProductDto source, Product destination, Expression<Func<Product, object>> propertySelector)
        {
            var memberExpression = (MemberExpression) propertySelector.Body;
            var propertyInfo = (PropertyInfo) memberExpression.Member;
            return CopyPropertyValue(source, destination, propertyInfo);
        }

        public static bool CopyPropertyValue(ProductDto source, Product destination, PropertyInfo property)
        {
            object sourceValue = property.GetValue(source);
            object destinationValue = property.GetValue(destination);
            bool hasValueChanged = !object.Equals(sourceValue, destinationValue);
            if(hasValueChanged)
            {
                property.SetValue(destination, sourceValue);
            }

            return hasValueChanged;
        }
    }
}