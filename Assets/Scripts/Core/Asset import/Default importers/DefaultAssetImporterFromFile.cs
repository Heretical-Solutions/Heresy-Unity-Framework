using System;

using System.Threading.Tasks;

using HereticalSolutions.Persistence;

using HereticalSolutions.ResourceManagement;
using HereticalSolutions.ResourceManagement.Factories;

using HereticalSolutions.Logging;

namespace HereticalSolutions.AssetImport
{
	public abstract class DefaultAssetImporterFromFile<TAsset, TDTO>
		: AAssetImporter
	{
		protected string resourcePath;

		protected ISerializer serializer;

		protected ISerializationArgument serializationArgument;

		protected ILoadVisitorGeneric<TAsset, TDTO> visitor;

		public DefaultAssetImporterFromFile(
			ILoggerResolver loggerResolver = null,
			ILogger logger = null)
			: base(
				loggerResolver,
				logger)
		{
		}

		public void Initialize(
			IRuntimeResourceManager runtimeResourceManager,
			string resourcePath,
			ISerializer serializer,
			ISerializationArgument serializationArgument,
			ILoadVisitorGeneric<TAsset, TDTO> visitor)
		{
			InitializeInternal(runtimeResourceManager);

			this.resourcePath = resourcePath;

			this.serializer = serializer;

			this.serializationArgument = serializationArgument;

			this.visitor = visitor;
		}

		public override async Task<IResourceVariantData> Import(
			IProgress<float> progress = null)
		{
			progress?.Report(0f);

			serializer.Deserialize<TDTO>(
				serializationArgument,
				out var dto);

			visitor.Load(
				dto,
				out var asset);

			var result = await AddAssetAsResourceToManager(
				asset,
				true,
				progress)
				.ThrowExceptions<IResourceVariantData>(
					GetType(),
					logger);

			progress?.Report(1f);

			return result;
		}

		public override void Cleanup()
		{
			resourcePath = null;

			serializer = null;

			serializationArgument = null;

			visitor = null;
		}

		protected virtual async Task<IResourceVariantData> AddAssetAsResourceToManager(
			TAsset asset,
			bool allocate = true,
			IProgress<float> progress = null)
		{
			progress?.Report(0f);

			var result = await AddAssetAsResourceVariant(
				await GetOrCreateResourceData(resourcePath)
					.ThrowExceptions<IResourceData>(
						GetType(),
						logger),
				new ResourceVariantDescriptor()
				{
					VariantID = string.Empty,
					VariantIDHash = string.Empty.AddressToHash(),
					Priority = 0,
					Source = EResourceSources.LOCAL_STORAGE,
					Storage = EResourceStorages.RAM,
					ResourceType = typeof(TAsset)
				},
#if USE_THREAD_SAFE_RESOURCE_MANAGEMENT
				ResourceManagementFactory
					.BuildConcurrentPreallocatedResourceStorageHandle<TAsset>(
						asset,
						runtimeResourceManager,
						loggerResolver),
#else
				ResourceManagementFactory
					.BuildPreallocatedResourceStorageHandle<TAsset>(
						asset,
						runtimeResourceManager,
						loggerResolver),
#endif
				allocate,
				progress)
				.ThrowExceptions<IResourceVariantData>(
					GetType(),
					logger);

			progress?.Report(1f);

			return result;
		}
	}
}