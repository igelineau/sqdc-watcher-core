

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;



namespace XFactory.SqdcWatcher.Core.Services
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

        public static void MergeListById<TTarget, TSource>(
            this ICollection<TTarget> destination,
            IEnumerable<TSource> source,
            Expression<Func<TSource, object>> sourceIdSelector = null,
            Expression<Func<TTarget, object>> targetIdSelector = null,
            Action<TTarget> augmenter = null) where TTarget : new()
        {
            PropertyInfo sourceIdProp = UnwrapPropertyExpression(
                sourceIdSelector ?? CreateDynamicPropertyExpression<TSource, object>("Id"));
            PropertyInfo targetIdProp = UnwrapPropertyExpression(
                targetIdSelector ?? CreateDynamicPropertyExpression<TTarget, object>("Id"));

            foreach (TSource sourceItem in source)
            {
                object sourceId = sourceIdProp.GetValue(sourceItem);
                TTarget targetItem = destination.FirstOrDefault(tItem =>
                {
                    object targetId = targetIdProp.GetValue(tItem);
                    if (ReferenceEquals(sourceId, targetId))
                    {
                        return true;
                    }

                    return sourceId?.Equals(targetId) ?? false;
                });

                if (targetItem == null)
                {
                    targetItem = MergeObjectsProperties(sourceItem, new TTarget());
                    destination.Add(targetItem);
                }
                else
                {
                    MergeObjectsProperties(sourceItem, targetItem);
                }

                augmenter?.Invoke(targetItem);
            }
        }

        public static TTarget MergeObjectsProperties<TSource, TTarget>(this TSource sourceObject, TTarget target)
        {
            IEnumerable<PropertyInfo> sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<string, PropertyInfo> targetWriteableProperties = typeof(TTarget)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => (!p.PropertyType.IsClass || p.PropertyType == typeof(string)) && p.GetSetMethod() != null)
                .ToDictionary(p => p.Name);
            foreach (PropertyInfo sourceProp in sourceProperties)
            {
                if (targetWriteableProperties.TryGetValue(sourceProp.Name, out PropertyInfo targetProp))
                {
                    targetProp.SetValue(target, sourceProp.GetValue(sourceObject));
                }
            }

            return target;
        }

        private static Expression<Func<TObjectType, TPropertyType>> CreateDynamicPropertyExpression<TObjectType, TPropertyType>(string propertyName)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(TObjectType));
            return Expression.Lambda<Func<TObjectType, TPropertyType>>(
                Expression.Property(parameter, propertyName), parameter);
        }

        [Pure]
        private static PropertyInfo GetPropertyInfoByName(Type type, string propertyName)
        {
            return type.GetProperty(propertyName,
                       BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                   ?? throw new ArgumentException($"Property '{propertyName}' not found in type {type}");
        }
    }
}