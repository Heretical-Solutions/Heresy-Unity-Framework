using System.Linq; //error CS1061: 'IEnumerable<World>' does not contain a definition for 'Contains'

using HereticalSolutions.Repositories;

using DefaultEcs;

using IEntityWorldsRepository =
	HereticalSolutions
	.Entities
	.IEntityWorldsRepository<
		DefaultEcs.World,
		DefaultEcs.System.ISystem<DefaultEcs.Entity>,
		DefaultEcs.Entity>;

using IWorldController =
	HereticalSolutions
	.Entities
	.IWorldController<
		DefaultEcs.World,
		DefaultEcs.System.ISystem<DefaultEcs.Entity>,
		DefaultEcs.Entity>;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Entities
{
	public class DefaultECSEntityWorldsRepository
		: IEntityWorldsRepository
	{
		private readonly IRepository<string, World> worldsRepository;

		private readonly IRepository<World, IWorldController> worldControllersRepository;

		private readonly ILogger logger;

		public DefaultECSEntityWorldsRepository(
			IRepository<string, World> worldsRepository,
			IRepository<World, IWorldController> worldControllersRepository,
			ILogger logger = null)
		{
			this.worldsRepository = worldsRepository;

			this.worldControllersRepository = worldControllersRepository;

			this.logger = logger;
		}

		#region IEntityWorldsRepository

		#region IReadOnlyEntityWorldsRepository

		public World GetWorld(string worldID)
		{
			if (!worldsRepository.TryGet(
				worldID,
				out var result))
			{
				logger?.LogError<DefaultECSEntityWorldsRepository>(
					$"NO WORLD REGISTERED BY ID {worldID}");

				return default;
			}

			return result;
		}

		public IWorldController GetWorldController(string worldID)
		{
			var world = GetWorld(worldID);

			if (world == default)
			{
				return default;
			}

			if (!worldControllersRepository.TryGet(
				world,
				out var result))
			{
				logger?.LogError<DefaultECSEntityWorldsRepository>(
					$"NO WORLD CONTROLLER REGISTERED BY ID {worldID}");

				return default;
			}

			return result;
		}

		public IWorldController GetWorldController(World world)
		{
			if (!worldsRepository.Values.Contains(world))
			{
				logger?.LogError<DefaultECSEntityWorldsRepository>(
					$"WORLD {world} NOT REGISTERED");

				return default;
			}

			if (!worldControllersRepository.TryGet(
				world,
				out var result))
			{
				logger?.LogError<DefaultECSEntityWorldsRepository>(
					$"NO WORLD CONTROLLER REGISTERED FOR THE WORLD {world.ToString()}");

				return default;
			}

			return result;
		}

		#endregion

		public bool HasWorld(string worldID)
		{
			return worldsRepository.Has(worldID);
		}

		public bool HasWorld(World world)
		{
			return worldControllersRepository.Has(world);
		}


		public void AddWorld(
			string worldID,
			IWorldController worldController)
		{
			var world = worldController.World;

			worldsRepository.TryAdd(
				worldID,
				world);

			worldControllersRepository.TryAdd(
				world,
				worldController);
		}


		public void RemoveWorld(string worldID)
		{
			if (!worldsRepository.TryGet(
				worldID,
				out var world))
			{
				return;
			}

			worldsRepository.TryRemove(
				worldID);

			worldControllersRepository.TryRemove(
				world);
		}

		public void RemoveWorld(World world)
		{
			string worldID = string.Empty;

			foreach (var key in worldsRepository.Keys)
			{
				if (worldsRepository.Get(key) == world)
				{
					worldID = key;

					break;
				}
			}

			if (string.IsNullOrEmpty(worldID))
			{
				return;
			}

			worldsRepository.TryRemove(
				worldID);

			worldControllersRepository.TryRemove(
				world);
		}

		#endregion
	}
}