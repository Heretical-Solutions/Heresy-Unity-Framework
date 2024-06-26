using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HereticalSolutions.Allocations;

using HereticalSolutions.Pools;
using HereticalSolutions.Pools.Factories;

using HereticalSolutions.Repositories;

using HereticalSolutions.ResourceManagement;

using HereticalSolutions.Logging;

namespace HereticalSolutions.AssetImport
{
	public class AssetImportManager
		: IAssetImportManager
	{
		private const int DEFAULT_IMPORTER_POOL_CAPACITY = 4;

		private static readonly AllocationCommandDescriptor initialAllocation = new AllocationCommandDescriptor
		{
			Rule = EAllocationAmountRule.ADD_PREDEFINED_AMOUNT,

			Amount = DEFAULT_IMPORTER_POOL_CAPACITY
		};

		private static readonly AllocationCommandDescriptor additionalAllocation = new AllocationCommandDescriptor
		{
			Rule = EAllocationAmountRule.DOUBLE_AMOUNT,

			Amount = 0
		};

		private readonly IRepository<Type, List<AAssetImportPostProcessor>> postProcessorRepository;

		private readonly IRepository<Type, INonAllocDecoratedPool<AAssetImporter>> importerPoolRepository;

		private readonly ILoggerResolver loggerResolver;

		private readonly ILogger logger;

		public AssetImportManager(
			IRepository<Type, List<AAssetImportPostProcessor>> postProcessorRepository,
			IRepository<Type, INonAllocDecoratedPool<AAssetImporter>> importerPoolRepository,
			ILoggerResolver loggerResolver = null,
			ILogger logger = null)
		{
			this.postProcessorRepository = postProcessorRepository;

			this.importerPoolRepository = importerPoolRepository;

			this.loggerResolver = loggerResolver;

			this.logger = logger;
		}

		#region IAssetImportManager

		public async Task<IResourceVariantData> Import<TImporter>(
			Action<TImporter> initializationDelegate = null,
			IProgress<float> progress = null)
			where TImporter : AAssetImporter
		{
			logger?.Log<AssetImportManager>($"IMPORTING {typeof(TImporter).Name} INITIATED");

			var pooledImporter = PopImporterSync<TImporter>();

			initializationDelegate?.Invoke(
				pooledImporter.Value as TImporter);

			var result = await pooledImporter.Value.Import(
				progress)
				.ThrowExceptions<IResourceVariantData, AssetImportManager>(
					logger);

			if (postProcessorRepository.Has(typeof(TImporter)))
			{
				var postProcessors = postProcessorRepository.Get(
					typeof(TImporter));

				for (int i = 0; i < postProcessors.Count; i++)
				{
					//This one should NOT be paralleled
					//Imagine one post processor adding 10 exrta vertices to the mesh while the other one substracts 20
					//Why? For any fucking reason. PostProcessors are exposed as API and I have zero idea what kind of shit
					//would a user come up with its usage
					await postProcessors[i].OnImport(
						result,
						progress)
						.ThrowExceptions<AssetImportManager>(
							logger);
				}
			}

			pooledImporter.Value.Cleanup();

			pooledImporter.Push();

			logger?.Log<AssetImportManager>($"IMPORTING {typeof(TImporter).Name} FINISHED");

			return result;
		}

		public async Task RegisterPostProcessor<TImporter, TPostProcessor>(
			TPostProcessor instance)
			where TImporter : AAssetImporter
			where TPostProcessor : AAssetImportPostProcessor
		{
			if (!postProcessorRepository.Has(typeof(TImporter)))
			{
				postProcessorRepository.Add(
					typeof(TImporter),
					new List<AAssetImportPostProcessor>());
			}

			var postProcessors = postProcessorRepository.Get(
				typeof(TImporter));

			postProcessors.Add(
				instance);
		}

		#region IAssetImporterPool

		public async Task<IPoolElement<AAssetImporter>> PopImporter<TImporter>()
			where TImporter : AAssetImporter
		{
			return PopImporterSync<TImporter>();
		}

		public async Task PushImporter(
			IPoolElement<AAssetImporter> pooledImporter)
		{
			pooledImporter.Value.Cleanup();

			pooledImporter.Push();
		}

		#endregion

		#endregion

		private IPoolElement<AAssetImporter> PopImporterSync<TImporter>()
			where TImporter : AAssetImporter
		{
			INonAllocDecoratedPool<AAssetImporter> importerPool;

			if (!importerPoolRepository.Has(typeof(TImporter)))
			{
				importerPool = PoolsFactory.BuildSimpleResizableObjectPool<AAssetImporter, TImporter>(
					initialAllocation,
					additionalAllocation,
					loggerResolver,
					new object[]
					{
						loggerResolver,
						(loggerResolver?.GetLogger<TImporter>() ?? null)
					});

				importerPoolRepository.Add(
					typeof(TImporter),
					importerPool);
			}
			else
			{
				importerPool = importerPoolRepository.Get(
					typeof(TImporter));
			}

			return importerPool.Pop(null);
		}
	}
}