using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Delegates.Notifiers
{
	public class AsyncNotifierSingleArgGeneric<TArgument, TValue>
		: IAsyncNotifierSingleArgGeneric<TArgument, TValue>
		  where TArgument : IEquatable<TArgument>
	{
		private readonly List<NotifyRequestSingleArgGeneric<TArgument, TValue>> requests;

		private readonly SemaphoreSlim semaphore;

		private readonly ILogger logger;

		public AsyncNotifierSingleArgGeneric(
			List<NotifyRequestSingleArgGeneric<TArgument, TValue>> requests,
			SemaphoreSlim semaphore,
			ILogger logger = null)
		{
			this.requests = requests;

			this.semaphore = semaphore;

			this.logger = logger;
		}

		#region IAsyncNotifierSingleArgGeneric

		public async Task<TValue> GetValueWhenNotified(
			TArgument argument = default,
			bool ignoreKey = false)
		{
			TaskCompletionSource<TValue> completionSource = new TaskCompletionSource<TValue>();

			var request = new NotifyRequestSingleArgGeneric<TArgument, TValue>(
				argument,
				ignoreKey,
				completionSource);


			await semaphore.WaitAsync();

			//logger?.Log<AsyncNotifierSingleArgGeneric<TArgument, TValue>>($"GetValueWhenNotified SEMAPHORE ACQUIRED");

			requests.Add(request);

			semaphore.Release();

			//logger?.Log<AsyncNotifierSingleArgGeneric<TArgument, TValue>>($"GetValueWhenNotified SEMAPHORE RELEASED");


			await completionSource
				.Task
				.ThrowExceptions<TValue, AsyncNotifierSingleArgGeneric<TArgument, TValue>>(logger);

			return completionSource.Task.Result;
		}

		public async Task<Task<TValue>> GetWaitForNotificationTask(
			TArgument argument = default,
			bool ignoreKey = false)
		{
			TaskCompletionSource<TValue> completionSource = new TaskCompletionSource<TValue>();

			var request = new NotifyRequestSingleArgGeneric<TArgument, TValue>(
				argument,
				ignoreKey,
				completionSource);


			await semaphore.WaitAsync();

			//logger?.Log<AsyncNotifierSingleArgGeneric<TArgument, TValue>>($"GetWaitForNotificationTask SEMAPHORE ACQUIRED");

			requests.Add(request);

			semaphore.Release();

			//logger?.Log<AsyncNotifierSingleArgGeneric<TArgument, TValue>>($"GetWaitForNotificationTask SEMAPHORE RELEASED");


			return GetValueFromCompletionSource(completionSource);
		}

		private async Task<TValue> GetValueFromCompletionSource(
			TaskCompletionSource<TValue> completionSource)
		{
			await completionSource
				.Task
				.ThrowExceptions<TValue, AsyncNotifierSingleArgGeneric<TArgument, TValue>>(logger);

			return completionSource.Task.Result;
		}

		public async Task Notify(
			TArgument argument,
			TValue value)
		{
			await semaphore.WaitAsync();

			//logger?.Log<AsyncNotifierSingleArgGeneric<TArgument, TValue>>($"Notify SEMAPHORE ACQUIRED");

			for (int i = requests.Count - 1; i >= 0; i--)
			{
				var request = requests[i];

				if (request.IgnoreKey
					|| EqualityComparer<TArgument>.Default.Equals(request.Key, argument)) //if (request.Key.Equals(argument)) - bad for strings
				{
					requests.RemoveAt(i);

					request.TaskCompletionSource.TrySetResult(value);					
				}
			}

			semaphore.Release();

			//logger?.Log<AsyncNotifierSingleArgGeneric<TArgument, TValue>>($"Notify SEMAPHORE RELEASED");
		}

		#endregion
	}
}