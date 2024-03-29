using System;
using System.Collections.Generic;
using System.Reflection;

using DefaultEcs;

using HereticalSolutions.Logging;

using HereticalSolutions.Repositories.Factories;

namespace HereticalSolutions.Entities.Factories
{
	/// <summary>
	/// Class containing methods to build entities and their components.
	/// </summary>
	public static partial class NetworkEntityFactory
	{
		public static DefaultECSEntityManager<TEntityID> BuildDefaultECSNetworkEntityManager<TEntityID, TEntityIDComponent>(
			Func<TEntityID> allocateIDDelegate,

			Func<TEntityIDComponent, TEntityID> getEntityIDFromIDComponentDelegate,
			Func<TEntityID, TEntityIDComponent> createIDComponentDelegate,

			ILoggerResolver loggerResolver = null)
		{
			var registryEntityRepository = RepositoriesFactory.BuildDictionaryRepository<TEntityID, Entity>();

			var entityWorldsRepository = DefaultECSEntityFactory.BuildDefaultECSEntityWorldsRepository(loggerResolver);


			entityWorldsRepository.AddWorld(
				WorldConstants.REGISTRY_WORLD_ID,
				DefaultECSEntityFactory.BuildDefaultECSRegistryWorldController(
					createIDComponentDelegate,
					DefaultECSEntityFactory.BuildDefaultECSPrototypesRepository(),
					loggerResolver));

			entityWorldsRepository.AddWorld(
				WorldConstants.EVENT_WORLD_ID,
				DefaultECSEntityFactory.BuildDefaultECSEventWorldController(
					loggerResolver));

			entityWorldsRepository.AddWorld(
				WorldConstants.SIMULATION_WORLD_ID,
				DefaultECSEntityFactory.BuildDefaultECSWorldController
					<TEntityID,
					TEntityIDComponent,
					SimulationEntityComponent,
					ResolveSimulationComponent>(
						getEntityIDFromIDComponentDelegate,
						createIDComponentDelegate,

						(component) => { return component.SimulationEntity; },
						(component) => { return component.PrototypeID; },
						(prototypeID, entity) =>
						{
							return new SimulationEntityComponent
							{
								PrototypeID = prototypeID,

								SimulationEntity = entity
							};
						},

						(source) => { return new ResolveSimulationComponent { Source = source }; },

						loggerResolver));

			entityWorldsRepository.AddWorld(
				WorldConstants.VIEW_WORLD_ID,
				DefaultECSEntityFactory.BuildDefaultECSWorldController
					<TEntityID,
					TEntityIDComponent,
					ViewEntityComponent,
					ResolveViewComponent>(
						getEntityIDFromIDComponentDelegate,
						createIDComponentDelegate,

						(component) => { return component.ViewEntity; },
						(component) => { return component.PrototypeID; },
						(prototypeID, entity) =>
						{
							return new ViewEntityComponent
							{
								PrototypeID = prototypeID,

								ViewEntity = entity
							};
						},

						(source) => { return new ResolveViewComponent { Source = source }; },

						loggerResolver));

			entityWorldsRepository.AddWorld(
				NetworkWorldConstants.NETWORKING_SERVER_DATA_WORLD_ID,
				DefaultECSEntityFactory.BuildDefaultECSWorldController
					<TEntityID,
					TEntityIDComponent,
					ServerDataEntityComponent,
					ResolveServerDataComponent>(
						getEntityIDFromIDComponentDelegate,
						createIDComponentDelegate,

						(component) => { return component.ServerDataEntity; },
						(component) => { return component.PrototypeID; },
						(prototypeID, entity) =>
						{
							return new ServerDataEntityComponent
							{
								PrototypeID = prototypeID,

								ServerDataEntity = entity
							};
						},

						(source) => { return new ResolveServerDataComponent { Source = source }; },

						loggerResolver));

			entityWorldsRepository.AddWorld(
				NetworkWorldConstants.NETWORKING_PREDICTION_WORLD_ID,
				DefaultECSEntityFactory.BuildDefaultECSWorldController
					<TEntityID,
					TEntityIDComponent,
					PredictionEntityComponent,
					ResolvePredictionComponent>(
						getEntityIDFromIDComponentDelegate,
						createIDComponentDelegate,

						(component) => { return component.PredictionEntity; },
						(component) => { return component.PrototypeID; },
						(prototypeID, entity) =>
						{
							return new PredictionEntityComponent
							{
								PrototypeID = prototypeID,

								PredictionEntity = entity
							};
						},

						(source) => { return new ResolvePredictionComponent { Source = source }; },

						loggerResolver));

			List<World> childEntityWorlds = new List<World>();

			childEntityWorlds.Add(entityWorldsRepository.GetWorld(WorldConstants.SIMULATION_WORLD_ID));
			childEntityWorlds.Add(entityWorldsRepository.GetWorld(WorldConstants.VIEW_WORLD_ID));
			childEntityWorlds.Add(entityWorldsRepository.GetWorld(NetworkWorldConstants.NETWORKING_SERVER_DATA_WORLD_ID));
			childEntityWorlds.Add(entityWorldsRepository.GetWorld(NetworkWorldConstants.NETWORKING_PREDICTION_WORLD_ID));

			ILogger logger = 
				loggerResolver?.GetLogger<DefaultECSEntityManager<TEntityID>>()
				?? null;

			return new DefaultECSEntityManager<TEntityID>(
				allocateIDDelegate,
				registryEntityRepository,
				entityWorldsRepository,
				childEntityWorlds,
				logger);
		}

		public static NetworkEventEntityBuilder<TEntityID> BuildEventEntityBuilder<TEntityID>(
			World world)
		{
			return new NetworkEventEntityBuilder<TEntityID>(world);
		}

		/*
		/// <summary>
		/// Builds an ECSWorldFullStateVisitor.
		/// </summary>
		/// <param name="entityManager">The entity manager to use.</param>
		/// <returns>The built ECSWorldFullStateVisitor.</returns>
		public static ECSWorldFullStateVisitor BuildECSWorldFullStateVisitor<TEntityID>(
			IEntityManager<World, TEntityID, Entity> entityManager,
			ILoggerResolver loggerResolver = null)
		{
			DefaultECSEntityFactory.BuildComponentTypesListWithAttribute<NetworkComponentAttribute>(
				out var componentTypes,
				out var hashToType,
				out var typeToHash);

			var readComponentMethodInfo =
				typeof(DefaultECSEntityFactory).GetMethod(
					"ReadComponent",
					BindingFlags.Static | BindingFlags.Public);

			var writeComponentMethodInfo =
				typeof(DefaultECSEntityFactory).GetMethod(
					"WriteComponent",
					BindingFlags.Static | BindingFlags.Public);

			ILogger logger =
				loggerResolver?.GetLogger<ECSWorldFullStateVisitor>()
				?? null;

			return new ECSWorldFullStateVisitor(
				entityManager,
				hashToType,
				typeToHash,
				DefaultECSEntityFactory.BuildComponentReaders(
					readComponentMethodInfo,
					componentTypes),
				DefaultECSEntityFactory.BuildComponentWriters(
					writeComponentMethodInfo,
					componentTypes),
				logger);
		}

		public static ECSWorldMementoVisitor BuildECSWorldMementoVisitor(
			IEntityManager<World, Entity> entityManager,
			ILoggerResolver loggerResolver = null)
		{
			DefaultECSEntityFactory.BuildComponentTypesListWithAttribute<NetworkComponentAttribute>(
				out var componentTypes,
				out var hashToType,
				out var typeToHash);

			var readComponentMethodInfo =
				typeof(DefaultECSEntityFactory).GetMethod(
					"ReadComponent",
					BindingFlags.Static | BindingFlags.Public);

			var writeComponentMethodInfo =
				typeof(DefaultECSEntityFactory).GetMethod(
					"WriteComponent",
					BindingFlags.Static | BindingFlags.Public);

			ILogger logger = 
				loggerResolver?.GetLogger<ECSWorldMementoVisitor>()
				?? null;

			return new ECSWorldMementoVisitor(
				entityManager,
				hashToType,
				typeToHash,
				DefaultECSEntityFactory.BuildComponentReaders(
					readComponentMethodInfo,
					componentTypes),
				DefaultECSEntityFactory.BuildComponentWriters(
					writeComponentMethodInfo,
					componentTypes),
				new ECSWorldMemento(
					RepositoriesFactory.BuildDictionaryRepository<Guid, ECSEntityMemento>(),
					new List<ECSEntityCreatedDeltaDTO>(),
					new List<ECSEntityDestroyedDeltaDTO>(),
					new List<ECSComponentDeltaDTO>(),
					new List<ECSComponentDeltaDTO>()),
				new HashSet<Guid>(),
				new List<ECSComponentDTO>(),
				logger);
		}
		*/

		public static ECSEventWorldVisitor<TEntityID> BuildECSEventWorldVisitor<TEntityID>(
			IEventEntityBuilder<Entity, TEntityID> eventEntityBuilder,
			bool host,
			ILoggerResolver loggerResolver = null)
		{
			DefaultECSEntityFactory.BuildComponentTypesListWithAttribute<NetworkEventComponentAttribute>(
				out var componentTypes,
				out var hashToType,
				out var typeToHash);

			var readComponentMethodInfo =
				typeof(DefaultECSEntityFactory).GetMethod(
					"ReadComponent",
					BindingFlags.Static | BindingFlags.Public);

			var writeComponentMethodInfo =
				typeof(DefaultECSEntityFactory).GetMethod(
					"WriteComponent",
					BindingFlags.Static | BindingFlags.Public);

			ILogger logger =
				loggerResolver?.GetLogger<ECSEventWorldVisitor<TEntityID>>()
				?? null;

			return new ECSEventWorldVisitor<TEntityID>(
				eventEntityBuilder,
				hashToType,
				typeToHash,
				DefaultECSEntityFactory.BuildComponentReaders(
					readComponentMethodInfo,
					componentTypes),
				DefaultECSEntityFactory.BuildComponentWriters(
					writeComponentMethodInfo,
					componentTypes),
				host,
				logger);
		}
	}
}