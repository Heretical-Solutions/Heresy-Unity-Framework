using System;
using System.Threading;
using System.Threading.Tasks;

using HereticalSolutions.Logging;

namespace HereticalSolutions.ResourceManagement
{
	public class ConcurrentPreallocatedResourceStorageHandle<TResource>
		: AConcurrentReadOnlyResourceStorageHandle<TResource>
	{
		private TResource value;

		public ConcurrentPreallocatedResourceStorageHandle(
			TResource value,
			SemaphoreSlim semaphore,
			IRuntimeResourceManager runtimeResourceManager,
			ILogger logger = null)
			: base(
				semaphore,
				runtimeResourceManager,
				logger)
		{
			this.value = value;
		}

        protected override async Task<TResource> AllocateResource(
			IProgress<float> progress = null)
        {
			return value;
        }

        protected override async Task FreeResource(
			TResource resource,
			IProgress<float> progress = null)
        {
        }
    }
}