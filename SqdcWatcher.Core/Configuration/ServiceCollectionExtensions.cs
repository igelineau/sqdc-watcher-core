using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Core.Abstractions;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Mappers;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Core.SiteCrawling;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCannaWatch(this IServiceCollection services)
        {
            services.AddSqdcAutoMapper();
            services.AddSqdcWatcherDbContext();
            
            services.AddGenericOpenTypeTransient(typeof(VisitorBase<>));
            services.AddGenericOpenTypeTransient(typeof(IMapper<,>));
            services.AddGenericOpenTypeTransient(typeof(IMappingFilter<,>));

            services.AddScoped<BecameInStockTriggerPolicy>();

            services.AddFactory<IScanOperation, ScanOperation>();
            services.AddScoped<ISqdcWatcher, SqdcHttpWatcher>();

            services.AddTransient<IProductsCachingProxy, ProductsFileCacheProxy>();
        }
        
        private static void AddSqdcAutoMapper(this IServiceCollection collection)
        {
            collection.AddSingleton(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductDto, Product>();
                cfg.CreateMap<ProductVariantDto, ProductVariant>();
                cfg.CreateMap<SpecificationAttributeDto, SpecificationAttribute>();
            }));
        }

        private static void AddGenericOpenTypeTransient(this IServiceCollection collection, Type baseType)
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
                    currentBaseType.IsGenericType && currentBaseType.GetGenericTypeDefinition() == baseTypeToFind)
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
                .FirstOrDefault(i => i == interfaceType || i.IsConstructedGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}