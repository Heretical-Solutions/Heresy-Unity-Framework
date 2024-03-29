using System;

using HereticalSolutions.Delegates;

using HereticalSolutions.Repositories;

using HereticalSolutions.LifetimeManagement;

namespace HereticalSolutions.Synchronization
{
	public class SynchronizationContextGeneric<TDelta>
		: ISynchronizableGenericArg<TDelta>,
		  ISynchronizationProvider,
		  ICleanUppable,
		  IDisposable
	{
		private readonly IReadOnlyObjectRepository metadata;

		private readonly IPublisherSingleArgGeneric<TDelta> broadcasterAsPublisher;

		private readonly INonAllocSubscribableSingleArgGeneric<TDelta> broadcasterAsSubscribable;


		private SynchronizationDescriptor descriptor;

		public SynchronizationContextGeneric(
			SynchronizationDescriptor descriptor,
			IReadOnlyObjectRepository metadata,
			IPublisherSingleArgGeneric<TDelta> broadcasterAsPublisher,
			INonAllocSubscribableSingleArgGeneric<TDelta> broadcasterAsSubscribable)
		{
			this.descriptor = descriptor;

			this.metadata = metadata;

			this.broadcasterAsPublisher = broadcasterAsPublisher;

			this.broadcasterAsSubscribable = broadcasterAsSubscribable;


			if (metadata.Has<IHasFixedDelta<TDelta>>())
			{
				var fixedDeltaMetadata = metadata.Get<IHasFixedDelta<TDelta>>();

				((IPublisherDependencyRecipient<TDelta>)fixedDeltaMetadata).BroadcasterAsPublisher = broadcasterAsPublisher;
			}
		}

		#region ISynchronizableGenericArg

		#region ISynchronizable

		public SynchronizationDescriptor Descriptor { get => descriptor; }

		public IReadOnlyObjectRepository Metadata { get => metadata; }

		#endregion

		public void Synchronize(TDelta delta)
		{
			if (metadata.Has<ITogglable>())
			{
				var togglable = metadata.Get<ITogglable>();

				if (!togglable.Active)
				{
					return;
				}
			}

			TDelta deltaActual = delta;

			if (metadata.Has<IScalable<TDelta>>())
			{
				var scalable = metadata.Get<IScalable<TDelta>>();

				deltaActual = scalable.Scale(deltaActual);
			}

			if (metadata.Has<IHasFixedDelta<TDelta>>())
			{
				var fixedDeltaMetadata = metadata.Get<IHasFixedDelta<TDelta>>();

				fixedDeltaMetadata.Tick(deltaActual);

				return;
			}

			broadcasterAsPublisher.Publish(deltaActual);
		}

		#endregion

		#region ISynchronizationProvider

		public void Subscribe(ISubscription subscription)
		{
			broadcasterAsSubscribable.Subscribe(
				(ISubscriptionHandler<
					INonAllocSubscribableSingleArgGeneric<TDelta>,
					IInvokableSingleArgGeneric<TDelta>>)
				subscription);
		}

		public void Unsubscribe(ISubscription subscription)
		{
			broadcasterAsSubscribable.Unsubscribe(
				(ISubscriptionHandler<
					INonAllocSubscribableSingleArgGeneric<TDelta>,
					IInvokableSingleArgGeneric<TDelta>>)
				subscription);
		}

		public void UnsubscribeAll()
		{
			broadcasterAsSubscribable.UnsubscribeAll();
		}

		#endregion

		#region ICleanUppable

		public void Cleanup()
		{
			if (metadata is ICleanUppable)
				(metadata as ICleanUppable).Cleanup();

			if (broadcasterAsPublisher is ICleanUppable)
				(broadcasterAsPublisher as ICleanUppable).Cleanup();
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (metadata is IDisposable)
				(metadata as IDisposable).Dispose();

			if (broadcasterAsPublisher is IDisposable)
				(broadcasterAsPublisher as IDisposable).Dispose();
		}

		#endregion
	}
}