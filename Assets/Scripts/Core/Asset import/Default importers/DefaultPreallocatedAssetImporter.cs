using System;
using System.Threading.Tasks;

using HereticalSolutions.ResourceManagement;
using HereticalSolutions.ResourceManagement.Factories;

using HereticalSolutions.Logging;

namespace HereticalSolutions.AssetImport
{
	public class DefaultPreallocatedAssetImporter<TAsset> : AAssetImporter
	{
		private string resourcePath;

		private TAsset preallocatedAsset;

		public DefaultPreallocatedAssetImporter(
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
			TAsset preallocatedAsset)
		{
			InitializeInternal(runtimeResourceManager);

			this.resourcePath = resourcePath;

			this.preallocatedAsset = preallocatedAsset;
		}

		public override async Task<IResourceVariantData> Import(
			IProgress<float> progress = null)
		{
			logger?.Log<DefaultPreallocatedAssetImporter<TAsset>>(
				$"IMPORTING {resourcePath} INITIATED");

			progress?.Report(0f);

			var result = await AddAssetAsResourceVariant(
				await GetOrCreateResourceData(
					resourcePath)
					.ThrowExceptions<IResourceData, DefaultPreallocatedAssetImporter<TAsset>>(logger),
				new ResourceVariantDescriptor()
				{
					VariantID = string.Empty,
					VariantIDHash = string.Empty.AddressToHash(),
					Priority = 0,
					Source = EResourceSources.RUNTIME_GENERATED,
					Storage = EResourceStorages.RAM,
					ResourceType = typeof(TAsset),
				},
#if USE_THREAD_SAFE_RESOURCE_MANAGEMENT
				ResourceManagementFactory.BuildConcurrentPreallocatedResourceStorageHandle<TAsset>(
					preallocatedAsset,
					runtimeResourceManager,
					loggerResolver),
#else
				ResourceManagementFactory.BuildPreallocatedResourceStorageHandle<TAsset>(
					preallocatedAsset,
					runtimeResourceManager,
					loggerResolver),
#endif
				true,
				progress)
				.ThrowExceptions<IResourceVariantData, DefaultPreallocatedAssetImporter<TAsset>>(logger);

			progress?.Report(1f);

			logger?.Log<DefaultPreallocatedAssetImporter<TAsset>>(
				$"IMPORTING {resourcePath} FINISHED");

			return result;
		}

		public override void Cleanup()
		{
			resourcePath = null;

			preallocatedAsset = default;
		}
	}
}