using System;
using System.Linq;
using AutoMapper.Mappers;
using AutoMapper;

namespace Etx.Collection.Mapper
{
    public static class Extensions
    {
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
