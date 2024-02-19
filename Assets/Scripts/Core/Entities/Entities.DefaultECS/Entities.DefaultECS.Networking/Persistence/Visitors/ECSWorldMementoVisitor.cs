﻿/*
using System;
using System.Linq; //error CS1061: 'IEnumerable<Guid>' does not contain a definition for 'ToArray'
using System.Collections.Generic;

using HereticalSolutions.Repositories;
using HereticalSolutions.Repositories.Factories;

using HereticalSolutions.Persistence;

using HereticalSolutions.Logging;

using DefaultEcs;

namespace HereticalSolutions.Entities
{
    public class ECSWorldMementoVisitor :
        ASaveVisitor<World, ECSWorldMemento>
    {
        private readonly IEntityManager<World, Entity> entityManager;
        
        
        private readonly IReadOnlyRepository<int, Type> hashToType;
        
        private readonly IReadOnlyRepository<Type, int> typeToHash;
        
        
        private readonly VisitorReadComponentDelegate[] componentReaders;
        
        private readonly IReadOnlyRepository<Type, VisitorWriteComponentDelegate> componentWriters;


        private readonly ECSWorldMemento cachedMemento;

        private readonly HashSet<Guid> entitiesAlive;
        
        private readonly List<ECSComponentDTO> componentDTOCache;

        
        private World cachedWorld;

        private EntitySet cachedEntitySet;
        

        public ECSWorldMementoVisitor(
            IEntityManager<World, Entity> entityManager,
            
            IReadOnlyRepository<int, Type> hashToType,
            IReadOnlyRepository<Type, int> typeToHash,
            
            VisitorReadComponentDelegate[] componentReaders,
            IReadOnlyRepository<Type, VisitorWriteComponentDelegate> componentWriters,
            
            ECSWorldMemento cachedMemento,
            HashSet<Guid> entitiesAlive,
            List<ECSComponentDTO> componentDTOCache,
            
            ILogger logger = null)
            : base(logger)
        {
            this.entityManager = entityManager;
            
            this.hashToType = hashToType;
            
            this.typeToHash = typeToHash;
            
            this.componentReaders = componentReaders;
            
            this.componentWriters = componentWriters;

            this.cachedMemento = cachedMemento;

            this.entitiesAlive = entitiesAlive;
            
            this.componentDTOCache = componentDTOCache;

            cachedWorld = null;

            cachedEntitySet = null;
        }

        #region ISaveVisitorGeneric

        public override bool Save(
            World value, 
            out ECSWorldMemento worldMemento)
        {
            if (value != cachedWorld)
            {
                cachedWorld = value;
                
                cachedEntitySet = 
                    value
                        .GetEntities()
                        .AsSet();
            }

            ParseEntities();
            
            worldMemento = cachedMemento;

            return true;
        }

        #endregion

        private void ParseEntities()
        {
            cachedMemento.EntitiesCreated.Clear();

            cachedMemento.EntitiesDestroyed.Clear();

            cachedMemento.ComponentsCreated.Clear();

            cachedMemento.ComponentsModified.Clear();

            entitiesAlive.Clear();

            var entities = cachedEntitySet;

            foreach (ref readonly Entity entity in entities.GetEntities())
            {
                if (!entity.IsAlive)
                    continue;

                var gameEntityComponent = entity.Get<GUIDComponent>();

                var guid = gameEntityComponent.GUID;

                var registryEntity = entityManager.GetRegistryEntity(guid);

                var registryEntityComponent = registryEntity.Get<RegistryEntityComponent>();

                entitiesAlive.Add(guid);

                bool entityCreated = !cachedMemento.EntityMementos.Has(guid);

                ECSEntityMemento entityMemento;

                if (entityCreated)
                {
                    entityMemento = new ECSEntityMemento(
                        guid.ToString(),
                        registryEntityComponent.PrototypeID,
                        RepositoriesFactory.BuildDictionaryRepository<int, ECSComponentDTO>());

                    cachedMemento.EntitiesCreated.Add(
                        new ECSEntityCreatedDeltaDTO
                        {
                            EntityGUID = guid.ToString(),

                            PrototypeID = registryEntityComponent.PrototypeID
                        });

                    cachedMemento.EntityMementos.Add(
                        guid,
                        entityMemento);
                }
                else
                {
                    entityMemento = cachedMemento.EntityMementos.Get(guid);
                }

                List<ECSComponentDTO> componentDTOs = componentDTOCache;

                componentDTOs.Clear();

                foreach (var typeSerializer in componentReaders)
                {
                    typeSerializer.Invoke(
                        entity,
                        typeToHash,
                        componentDTOs);
                }

                foreach (var componentDTO in componentDTOs)
                {
                    if (entityCreated)
                    {
                        cachedMemento.ComponentsCreated.Add(
                            new ECSComponentDeltaDTO
                            {
                                EntityGUID = guid.ToString(),

                                ComponentDTO = componentDTO
                            });

                        entityMemento.ComponentDTOs.Add(
                            componentDTO.TypeHash,
                            componentDTO);
                    }
                    else
                    {
                        if (!entityMemento.ComponentDTOs.Has(componentDTO.TypeHash))
                        {
                            cachedMemento.ComponentsCreated.Add(
                                new ECSComponentDeltaDTO
                                {
                                    EntityGUID = guid.ToString(),

                                    ComponentDTO = componentDTO
                                });

                            entityMemento.ComponentDTOs.Add(
                                componentDTO.TypeHash,
                                componentDTO);
                        }
                        else
                        {
                            var previousDTO = entityMemento.ComponentDTOs.Get(componentDTO.TypeHash);

                            if (!ByteArraysEqual(
                                    previousDTO.Data,
                                    componentDTO.Data))
                            {
                                cachedMemento.ComponentsModified.Add(
                                    new ECSComponentDeltaDTO
                                    {
                                        EntityGUID = guid.ToString(),

                                        ComponentDTO = componentDTO
                                    });

                                entityMemento.ComponentDTOs.Update(
                                    componentDTO.TypeHash,
                                    componentDTO);
                            }
                        }
                    }
                }
            }

            foreach (var entityID in cachedMemento.EntityMementos.Keys.ToArray())
            {
                if (!entitiesAlive.Contains(entityID))
                {
                    cachedMemento.EntitiesDestroyed.Add(
                        new ECSEntityDestroyedDeltaDTO
                        {
                            EntityGUID = entityID.ToString()
                        });

                    cachedMemento.EntityMementos.Remove(entityID);
                }
            }
        }

        public void ReadComponent(
            Entity serverEntity,
            ECSComponentDTO componentDTO)
        {
            var componentType = hashToType.Get(componentDTO.TypeHash);

            componentWriters.Get(componentType).Invoke(
                serverEntity,
                componentDTO);
        }

        //Courtesy of https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net
        // byte[] is implicitly convertible to ReadOnlySpan<byte>
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}
*/