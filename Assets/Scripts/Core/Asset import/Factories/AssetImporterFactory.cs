using System;
using System.Collections.Generic;
using System.Threading;

using HereticalSolutions.Repositories;
using HereticalSolutions.Repositories.Factories;

using HereticalSolutions.Pools;

using HereticalSolutions.Logging;

namespace HereticalSolutions.AssetImport.Factories
{
	public static class AssetImporterFactory
	{
		public static AssetImportManager BuildAssetImportManager(
			ILoggerResolver loggerResolver = null)
		{
			var logger =
				loggerResolver?.GetLogger<AssetImportManager>()
				?? null;

			return new AssetImportManager(
				(IRepository<Type, List<AAssetImportPostProcessor>>)
					RepositoriesFactory.BuildDictionaryRepository<Type, List<AAssetImportPostProcessor>>(),
				(IRepository<Type, INonAllocDecoratedPool<AAssetImporter>>)
					RepositoriesFactory.BuildDictionaryRepository<Type, INonAllocDecoratedPool<AAssetImporter>>(),
				loggerResolver,
				logger);
		}

		public static ConcurrentAssetImportManager BuildConcurrentAssetImportManager(
			ILoggerResolver loggerResolver = null)
		{
			var logger =
				loggerResolver?.GetLogger<AssetImportManager>()
				?? null;

			return new ConcurrentAssetImportManager(
				(IRepository<Type, List<AAssetImportPostProcessor>>)
					RepositoriesFactory.BuildDictionaryRepository<Type, List<AAssetImportPostProcessor>>(),
				(IRepository<Type, INonAllocDecoratedPool<AAssetImporter>>)
					RepositoriesFactory.BuildDictionaryRepository<Type, INonAllocDecoratedPool<AAssetImporter>>(),
				new SemaphoreSlim(1, 1),
				new SemaphoreSlim(1, 1),
				loggerResolver,
				logger);
		}
	}
}