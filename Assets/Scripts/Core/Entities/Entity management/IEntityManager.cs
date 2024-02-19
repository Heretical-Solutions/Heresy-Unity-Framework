using System;

namespace HereticalSolutions.Entities
{
    public interface IEntityManager<TWorld, TEntityID, TEntity>
        : IReadOnlyEntityManager<TEntityID, TEntity>
    {
        #region Spawn entity

        TEntityID SpawnEntity(
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT);

        bool SpawnEntity(
            TEntityID entityID,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT);

        TEntity SpawnWorldLocalEntity(
            string prototypeID,
            string worldID);

        TEntity SpawnWorldLocalEntity(
            string prototypeID,
            TWorld world);

        #endregion

        #region Resolve entity

        TEntityID ResolveEntity(
            object source,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT);

        bool ResolveEntity(
            TEntityID entityID,
            object source,
            string prototypeID,
            EEntityAuthoringPresets authoringPreset = EEntityAuthoringPresets.DEFAULT);

        TEntity ResolveWorldLocalEntity(
            string prototypeID,
            object source,
            string worldID);

        TEntity ResolveWorldLocalEntity(
            string prototypeID,
            object source,
            TWorld world);

        #endregion

        #region Despawn entity

        void DespawnEntity(TEntityID entityID);

        void DespawnWorldLocalEntity(TEntity entity);

        #endregion
    }
}