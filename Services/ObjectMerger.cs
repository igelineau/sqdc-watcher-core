using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SqdcWatcher.Services
{
    public static class ObjectMerger
    {
        private static PropertyInfo UnwrapPropertyExpression(LambdaExpression expression)
        {
            if (expression.Body is UnaryExpression unaryExpression)
            {
                return (PropertyInfo) ((MemberExpression) unaryExpression.Operand).Member;
            }
            
            if (!(expression.Body is MemberExpression memberExpression))
            {
                throw new InvalidOperationException("The reduced expression must be a member expression.");
            }

            return (PropertyInfo) memberExpression.Member;
        }
        
        public static void MergeListById<TTarget, TSource> (
            this ICollection<TTarget> destination,
            IEnumerable<TSource> source,
            Expression<Func<TTarget, object>> idSelector) where TTarget: new()
        {
            PropertyInfo targetIdProp = UnwrapPropertyExpression(idSelector);
            PropertyInfo sourceIdProp =
                typeof(TSource).GetProperty(targetIdProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                ?? throw new ArgumentException($"ID property not found in source type {typeof(TSource)}, Id property name={targetIdProp.Name}");
            foreach (TSource sourceItem in source)
            {
                object sourceId = sourceIdProp.GetValue(sourceItem);
                TTarget targetItem = destination.FirstOrDefault(tItem =>
                {
                    object targetId = targetIdProp.GetValue(tItem);
                    if (object.ReferenceEquals(sourceId, targetId))
                    {
                        return true;
                    }

                    return sourceId.Equals(targetId);
                });

                if (targetItem == null)
                {
                    destination.Add(MergeObjectsProperties(sourceItem, new TTarget()));
                }
                else
                {
                    MergeObjectsProperties(sourceItem, targetItem);
                }
            }
        }

        public static TTarget MergeObjectsProperties<TSource, TTarget>(this TSource sourceObject, TTarget target)
        {
            IEnumerable<PropertyInfo> sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<string, PropertyInfo> targetWriteableProperties = typeof(TTarget).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => (!p.PropertyType.IsClass || p.PropertyType == typeof(string)) && p.GetSetMethod() != null).ToDictionary(p => p.Name);
            foreach (PropertyInfo sourceProp in sourceProperties)
            {
                if (targetWriteableProperties.TryGetValue(sourceProp.Name, out PropertyInfo targetProp))
                {
                    targetProp.SetValue(target, sourceProp.GetValue(sourceObject));
                }
            }

            return target;
        }
    }
}