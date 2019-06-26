using System;
using System.Linq;
using AutoMapper.Mappers;
using AutoMapper;

namespace Etx.Collection.Mapper
{
    public static class Extensions
    {
        /// <summary>
        /// Lets you to inject generic collection mapper in automapper's mapping pipeline. The generic collection mapper uses a smart way to map collections. It removes the missing ones,
        /// updates the existing ones and adds the new Items. The Items are compared according to the key selectors required in the parameters.
        /// </summary>
        /// <typeparam name="TSourceItem">The base type of the source models that you want this mapper to work for</typeparam>
        /// <typeparam name="TDestinationItem">The base type of the destination entites that you want this mapper to work for</typeparam>
        /// <param name="cfg"></param>
        /// <param name="sourceKeySelector">Key selector for source model</param>
        /// <param name="destinationKeySelector">Key selector for destination model</param>
        public static void AddGenericCollectionMapper<TSourceItem, TDestinationItem>
            (this IMapperConfigurationExpression cfg, Func<TSourceItem, object> sourceKeySelector, Func<TDestinationItem, object> destinationKeySelector)
        {
            var mappers = cfg.Mappers;
            var targetMapper = cfg.Mappers.FirstOrDefault(om => om is ReadOnlyCollectionMapper);
            var index = targetMapper == null ? 0 : mappers.IndexOf(targetMapper);
            IConfigurationObjectMapper mapper = new GenericCollectionMapper<TSourceItem, TDestinationItem>(sourceKeySelector, destinationKeySelector);
            mappers.Insert(index, mapper);
            cfg.Advanced.BeforeSeal(configurationProvider =>
            {
                mapper.ConfigurationProvider = configurationProvider;
            });
        }
    }
}
