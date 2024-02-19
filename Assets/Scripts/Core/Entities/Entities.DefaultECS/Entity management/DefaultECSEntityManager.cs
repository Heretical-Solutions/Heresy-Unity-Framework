using System;
using System.Linq; //error CS1061: 'IEnumerable<Guid>' does not contain a definition for 'Count'
using System.Collections.Generic;

using HereticalSolutions.Allocations.Factories;

using HereticalSolutions.Repositories;

using HereticalSolutions.Logging;

using World = DefaultEcs.World;

using Entity = DefaultEcs.Entity;

using DefaultEcs.System;

namespace HereticalSolutions.Entities
{
    public class DefaultECSEntityManager<TEntityID>
        : IEntityManager<World, TEntityID, Entity>,
          IContainsEntityWorlds<World, ISystem<Entity>, Entity>
    {
        private readonly Func<TEntityID> allocateIDDelegate;

        private readonly IRepository<TEntityID, Entity> registryEntitiesRepository;

        private readonly IReadOnlyEntityWorldsRepository<World, ISystem<Entity>, Entity> entityWorldsRepository;

        //TODO: ensure that it's what this class needs
        private readonly IReadOnlyList<World> childEntityWorlds;

        private readonly ILogger logger;

        public DefaultECSEntityManager(
            Func<TEntityID> allocateIDDelegate,
            IRepository<TEntityID, Entity> registryEntitiesRepository,
            IReadOnlyEntityWorldsRepository<World, ISystem<Entity>, Entity> entityWorldsRepository,
            IReadOnlyList<World> childEntityWorlds,
            ILogger logger = null)
        {
            this.allocateIDDelegate = allocateIDDelegate;

            this.registryEntitiesRepository = registryEntitiesRepository;

            this.entityWorldsRepository = entityWorldsRepository;

            this.childEntityWorlds = childEntityWorlds;

            this.logger = logger;
        }

        #region IEntityManager

        #region IReadOnlyEntityRepository

        public bool HasEntity(TEntityID entityID)
        {
            return registryEntitiesRepository.Has(entityID);
        }

        public Entity GetRegistryEntity(TEntityID entityID)
        {
            if (!registryEntitiesRepository.TryGet(
                entityID,
                out var result))
                return default(Entity);

            return result;
        }

        public EntityDescriptor<TEntityID>[] AllRegistryEntities
        {
            get
            {
                var keys = registryEntitiesRepository.Keys;
                
                var result = new EntityDescriptor<TEntityID>[keys.Count()];

                int index = 0;
                
                foreach (var key in keys)
                {
                    result[index] = new EntityDescriptor<TEntityID>
                    {
                        ID = key,
                        
                        PrototypeID = registryEntitiesRepository.Get(key).Get<RegistryEntityComponent>().PrototypeID
                    };
                }

                return result;
            }
        }
        
        public IEnumerable<TEntityID> AllAllocatedIDs => registryEntitiesRepository.Keys;

        #endregion

        #region Spawn entity
        
        public TEntityID SpawnEntity(
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT)
        {
            var newID = AllocateID();

            if (!SpawnEntityInAllRelevantWorlds(
                    newID,
                    prototypeID,
                    authoringPreset))
                return default(TEntityID);

            return newID;
        }

        public bool SpawnEntity(
            TEntityID entityID,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT)
        {
            return SpawnEntityInAllRelevantWorlds(
                entityID,
                prototypeID,
                authoringPreset);
        }

        public Entity SpawnWorldLocalEntity(
            string prototypeID,
            string worldID)
        {
            var worldController = (IPrototypeCompliantWorldController<World, Entity>)
                entityWorldsRepository.GetWorldController(worldID);

            if (worldController == null)
                return default;

            worldController.TrySpawnEntityFromPrototype(
                prototypeID,
                out var localEntity);

            return localEntity;
        }

        public Entity SpawnWorldLocalEntity(
            string prototypeID,
            World world)
        {
            var worldController = (IPrototypeCompliantWorldController<World, Entity>)
                entityWorldsRepository.GetWorldController(world);

            if (worldController == null)
                return default;

            worldController.TrySpawnEntityFromPrototype(
                prototypeID,
                out var localEntity);

            return localEntity;
        }

        #endregion

        #region Resolve entity

        public TEntityID ResolveEntity(
            object source,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT)
        {
            var newGUID = AllocateID();

            if (!SpawnAndResolveEntityInAllRelevantWorlds(
                newGUID,
                prototypeID,
                authoringPreset))
                return default(TEntityID);

            return newGUID;
        }

        public bool ResolveEntity(
            TEntityID entityID,
            object source,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT)
        {
            return SpawnAndResolveEntityInAllRelevantWorlds(
                entityID,
                prototypeID,
                source,
                authoringPreset);
        }

        public Entity ResolveWorldLocalEntity(
            string prototypeID,
            object source,
            string worldID)
        {
            var worldController = (IPrototypeCompliantWorldController<World, Entity>)
                entityWorldsRepository.GetWorldController(worldID);

            if (worldController == null)
                return default;

            worldController.TrySpawnAndResolveEntityFromPrototype(
                prototypeID,
                source,
                out var localEntity);

            return localEntity;
        }

        public Entity ResolveWorldLocalEntity(
            string prototypeID,
            object source,
            World world)
        {
            var worldController = (IPrototypeCompliantWorldController<World, Entity>)
                entityWorldsRepository.GetWorldController(world);

            if (worldController == null)
                return default;

            worldController.TrySpawnAndResolveEntityFromPrototype(
                prototypeID,
                source,
                out var localEntity);

            return localEntity;
        }

        #endregion

        #region Despawn entity

        public void DespawnEntity(TEntityID entityID)
        {
            if (!registryEntitiesRepository.TryGet(
                entityID,
                out var registryEntity))
            {
                logger?.LogError<DefaultECSEntityWorldsRepository>(
                    $"NO ENTITY REGISTERED BY ID {entityID}");

                return;
            }

            foreach (var entityWorld in childEntityWorlds)
            {
                var worldController = (IRegistryCompliantWorldController<Entity>)
                    entityWorldsRepository.GetWorldController(entityWorld);

                if (worldController == null)
                    continue;

                worldController.DespawnEntityAndUnlinkFromRegistry(
                    registryEntity);
            }

            var registryWorldController =
                entityWorldsRepository.GetWorldController(WorldConstants.REGISTRY_WORLD_ID);

            registryWorldController.DespawnEntity(
                registryEntity);

            registryEntitiesRepository.Remove(
                entityID);
        }

        public void DespawnWorldLocalEntity(Entity entity)
        {
            if (entity == default)
                return;

            var world = entity.World;

            var worldController = entityWorldsRepository.GetWorldController(world);

            worldController.DespawnEntity(
                entity);
        }

        #endregion

        #endregion

        #region IContainsEntityWorlds

        public IReadOnlyEntityWorldsRepository<World, ISystem<Entity>, Entity> EntityWorldsRepository { get => entityWorldsRepository; }

        #endregion

        private TEntityID AllocateID()
        {
            /*
            Guid newGUID;

            do
            {
                newGUID = IDAllocationsFactory.BuildGUID();
            }
            while (registryEntitiesRepository.Has(newGUID));

            return newGUID;
            */

            return allocateIDDelegate.Invoke();
        }

        /*
        public void SpawnEntityFromServer(
            Guid guid,
            string prototypeID)
        {
            SpawnEntity(
                guid,
                prototypeID,
                EEntityAuthoringPresets.NETWORKING_CLIENT);
        }
        */

        private bool SpawnEntityInAllRelevantWorlds(
            TEntityID entityID,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT)
        {
            var registryWorldController = (IEntityIDCompliantWorldController<TEntityID, Entity>)
                entityWorldsRepository.GetWorldController(WorldConstants.REGISTRY_WORLD_ID);
            
            if (!registryWorldController.TrySpawnEntityWithIDFromPrototype(
                prototypeID,
                entityID,
                out var registryEntity))
            {
                return false;
            }

            registryEntitiesRepository.Add(
                entityID,
                registryEntity);
            
            switch (authoringPreset)
            {
                case EEntityAuthoringPresets.DEFAULT:
                    foreach (var entityWorld in childEntityWorlds)
                    {
                        var worldController = (IRegistryCompliantWorldController<Entity>)
                            entityWorldsRepository.GetWorldController(entityWorld);

                        if (worldController == null)
                            continue;

                        worldController.TrySpawnEntityFromRegistry(
                            registryEntity,
                            out var localEntity);
                    }

                    break;
                
                default:
                    break;
            }

            return true;
        }

        /*
        private void RemoveEntityComponentFromRegistry<TEntity>(Entity registryEntity)
        {
            if (registryEntity.Has<TEntity>())
                registryEntity.Remove<TEntity>();
        }
        */

        private bool SpawnAndResolveEntityInAllRelevantWorlds(
            TEntityID entityID,
            string prototypeID,
            object source,
            EEntityAuthoringPresets authoring = EEntityAuthoringPresets.DEFAULT)
        {
            var registryWorldController = (IEntityIDCompliantWorldController<TEntityID, Entity>)
                entityWorldsRepository.GetWorldController(WorldConstants.REGISTRY_WORLD_ID);

            if (!registryWorldController.TrySpawnEntityWithIDFromPrototype(
                    prototypeID,
                    entityID,
                    out var registryEntity))
            {
                return false;
            }

            registryEntitiesRepository.Add(
                entityID,
                registryEntity);
            
            switch (authoring)
            {
                case EEntityAuthoringPresets.DEFAULT:
                    foreach (var entityWorld in childEntityWorlds)
                    {
                        var worldController = (IRegistryCompliantWorldController<Entity>)
                            entityWorldsRepository.GetWorldController(entityWorld);

                        if (worldController == null)
                            continue;

                        worldController.TrySpawnAndResolveEntityFromRegistry(
                            registryEntity,
                            source,
                            out var localEntity);
                    }

                    break;
                
                default:
                    break;
            }

            return true;
        }
    }
}