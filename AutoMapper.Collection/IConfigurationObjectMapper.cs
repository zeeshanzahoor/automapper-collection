using AutoMapper;

namespace Etx.Collection.Mapper
{
    internal interface IConfigurationObjectMapper : IObjectMapper
    {
        IConfigurationProvider ConfigurationProvider { get; set; }
    }
}
