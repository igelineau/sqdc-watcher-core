using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using XFactory.SqdcWatcher.Core.RestApiModels;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Data.Entities;

namespace XFactory.SqdcWatcher.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureSqdcAutoMapper(this IServiceCollection collection)
        {
            collection.AddSingleton(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductDto, Product>();
                cfg.CreateMap<ProductVariantDto, ProductVariant>();
                cfg.CreateMap<SpecificationAttributeDto, SpecificationAttribute>();
            }));
        }
        
        public static void AddGenericOpenTypeTransient(this IServiceCollection collection, Type baseType)
        {
            IEnumerable<Type> typesToAdd = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && FindBaseType(t, baseType) != null);
            foreach (Type type in typesToAdd)
            {
                collection.AddTransient(type);
                collection.AddTransient(FindBaseType(type, baseType), type);
            }
        }
        
        private static Type FindBaseType(Type derivedType, Type baseTypeToFind)
        {
            Type implementedInterface = FindImplementedInterface(derivedType, baseTypeToFind);
            if (implementedInterface != null)
            {
                return implementedInterface;
            }

            // concrete type
            Type currentBaseType = derivedType.BaseType;
            while (currentBaseType != null)
            {
                if (currentBaseType == baseTypeToFind ||
                    (currentBaseType.IsGenericType && currentBaseType.GetGenericTypeDefinition() == baseTypeToFind))
                {
                    return currentBaseType;
                }

                currentBaseType = currentBaseType.BaseType;
            }

            return null;
        }
        
        private static Type FindImplementedInterface(Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                return null;
            }

            return type
                .GetInterfaces()
                .FirstOrDefault(i => i == interfaceType || (i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType));
        }
    }
}