namespace HereticalSolutions.Pools.AllocationCallbacks
{
	public class PushToDecoratedPoolCallback<T> : IAllocationCallback<T>
	{
		private INonAllocDecoratedPool<T> root; 
		public INonAllocDecoratedPool<T> Root
		{
			get => root;
			set
			{
				root = value;

				if (deferredCallbackQueue != null)
				{
					deferredCallbackQueue.Process();

					deferredCallbackQueue.Callback = null;

					deferredCallbackQueue = null;
				}
			}
		}

		//TODO: eliminate. Let the DryRun's go without this shit
		private DeferredCallbackQueue<T> deferredCallbackQueue;

		public PushToDecoratedPoolCallback(INonAllocDecoratedPool<T> root = null)
		{
			this.root = root;

			deferredCallbackQueue = null;
		}
        
		public PushToDecoratedPoolCallback(DeferredCallbackQueue<T> deferredCallbackQueue)
		{
			root = null;
            
			this.deferredCallbackQueue = deferredCallbackQueue;
			
			this.deferredCallbackQueue.Callback = this;
		}

		public void OnAllocated(IPoolElement<T> currentElement)
		{
			if (currentElement.Value == null)
				return;

			if (root == null)
			{
				deferredCallbackQueue?.Enqueue(currentElement);

				return;
			}

			root.Push(currentElement, true);
		}
	}
}