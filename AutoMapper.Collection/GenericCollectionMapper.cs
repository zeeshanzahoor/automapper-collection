using AutoMapper;
using AutoMapper.Configuration.Internal;
using AutoMapper.Mappers;
using AutoMapper.Mappers.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Etx.Collection.Mapper
{
    /// <summary>
    /// This mapper is injected in automapper's mapping pipeline to overrid enumerable mappings and does the job in a more smarter way. This class is not intended to be used directly from outside of this library.
    /// </summary>
    public class GenericCollectionMapper<TSourceItem, TDestinationItem> : EnumerableMapperBase, IConfigurationObjectMapper
    {
        private readonly Func<TSourceItem, object> _sourceKeySelector;
        private readonly Func<TDestinationItem, object> _destinationKeySelector;
        private readonly CollectionMapper CollectionMapper = new CollectionMapper();

        public IConfigurationProvider ConfigurationProvider { get; set; }
        public GenericCollectionMapper(Func<TSourceItem, object> sourceKeySelector, Func<TDestinationItem, object> destinationKeySelector)
        {
            _sourceKeySelector = sourceKeySelector;
            _destinationKeySelector = destinationKeySelector;
        }
        public TDestination Map<TSource, ITSourceItem, TDestination, ITDestinationItem>(TSource source, TDestination destination, ResolutionContext context)
           where TSource : IEnumerable<ITSourceItem>
           where TDestination : ICollection<ITDestinationItem>
           where ITSourceItem : TSourceItem
           where ITDestinationItem : TDestinationItem
        {
            if (source == null || destination == null)
            {
                return destination;
            }

            ITDestinationItem mapper(ITSourceItem s, ITDestinationItem d) => context.Mapper.Map(s, d, context);

            var deleted = (from destItem in destination
                           join srcItem in source on _destinationKeySelector(destItem) equals _sourceKeySelector(srcItem) into srcItems
                           from srcItem in srcItems.DefaultIfEmpty() where srcItem == null
                           select destItem);

            var updated = (from destItem in destination
                           join srcItem in source on _destinationKeySelector(destItem) equals _sourceKeySelector(srcItem)
                           select mapper(srcItem, destItem)).ToList();

            var inserted = (from srcItem in source
                            join destItem in destination on _sourceKeySelector(srcItem) equals _destinationKeySelector(destItem) into destItems
                            from destItem in destItems.DefaultIfEmpty()
                            where destItem == null
                            select mapper(srcItem, destItem));

            return (TDestination)destination.Concat(inserted).Except(deleted).ToList().AsEnumerable();

        }

        public override bool IsMatch(TypePair context) =>
                PrimitiveHelper.IsEnumerableType(context.SourceType) &
                    PrimitiveHelper.IsCollectionType(context.DestinationType) &
                           context.SourceType.GetGenericArguments()?.FirstOrDefault()?.IsSubclassOf(typeof(TSourceItem)) &
                                context.DestinationType.GetGenericArguments()?.FirstOrDefault()?.IsSubclassOf(typeof(TDestinationItem)) ?? false;


        private static readonly MethodInfo MapMethodInfo = typeof(GenericCollectionMapper<TSourceItem, TDestinationItem>).GetRuntimeMethods().First(_ => _.Name == nameof(Map));

        public override Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {

            var method = MapMethodInfo.MakeGenericMethod(sourceExpression.Type, 
                                                        ElementTypeHelper.GetElementType(sourceExpression.Type), 
                                                        destExpression.Type, ElementTypeHelper.GetElementType(sourceExpression.Type));

            var map = Expression.Call(Expression.Constant(this), method, sourceExpression, destExpression, contextExpression);
            var collectionMap = CollectionMapper.MapExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression, contextExpression);
            var notNull = Expression.NotEqual(destExpression, Expression.Constant(null));
            return Expression.Condition(notNull, map, Expression.Convert(collectionMap, destExpression.Type));
        }
    }
}
