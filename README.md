# automapper-collection
Collection mapping library that works perfectly with EntityFramework.


## Usage 

        Mapper.Initialize(cfg => {
            cfg.AddGenericCollectionMapper<SourceBase, DestinationBase>(s => s.Key, s => s.PrimaryKey);
        });
..more to come
