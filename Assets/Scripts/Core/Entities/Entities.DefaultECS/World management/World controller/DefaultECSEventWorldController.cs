using DefaultEcs;
using DefaultEcs.System;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Entities
{
	public class DefaultECSEventWorldController
		: IWorldController<World, ISystem<Entity>, Entity>
	{
		private readonly ILogger logger;

		public DefaultECSEventWorldController(
			World world,
			ILogger logger = null)
		{
			World = world;

			this.logger = logger;
		}

		#region IWorldController

		public World World { get; private set; }


		public ISystem<Entity> EntityResolveSystems { get => null; }

		public ISystem<Entity> EntityInitializationSystems { get => null; }

		public ISystem<Entity> EntityDeinitializationSystems { get => null; }


		public bool TrySpawnEntity(
			out Entity entity)
		{
			entity = World.CreateEntity();

			return true;
		}

		public bool TrySpawnAndResolveEntity(
			object source,
			out Entity entity)
		{
			//There's no use in resolving in registry world (for now)
			return TrySpawnEntity(
				out entity);
		}

		public void DespawnEntity(
			Entity entity)
		{
			if (entity == default)
				return;

			if (entity.World != World)
				logger?.LogError<DefaultECSEventWorldController>(
					$"ATTEMPT TO DESPAWN ENTITY FROM THE WRONG WORLD");

			if (entity.Has<DespawnComponent>())
				return;

			entity.Set<DespawnComponent>();
		}

		#endregion
	}
}