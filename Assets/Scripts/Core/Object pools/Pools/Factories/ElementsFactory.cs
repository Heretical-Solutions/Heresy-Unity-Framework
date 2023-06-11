using System;

using HereticalSolutions.Collections;

using HereticalSolutions.Allocations;
using HereticalSolutions.Allocations.Factories;

using HereticalSolutions.Pools.Allocations;
using HereticalSolutions.Pools.Elements;

using HereticalSolutions.Repositories;
using HereticalSolutions.Repositories.Factories;

namespace HereticalSolutions.Pools.Factories
{
	public static partial class PoolsFactory
	{
		#region Pool element allocation command

		public static AllocationCommand<IPoolElement<T>> BuildPoolElementAllocationCommandWithCallback<T>(
			AllocationCommandDescriptor descriptor,
			Func<T> valueAllocationDelegate,
			MetadataAllocationDescriptor[] metadataDescriptors,
			IAllocationCallback<T> callback)
		{
			Func<IPoolElement<T>> poolElementAllocationDelegate = () =>
				BuildPoolElementWithAllocationCallback(
					valueAllocationDelegate,
					metadataDescriptors,
					callback);

			var poolElementAllocationCommand = new AllocationCommand<IPoolElement<T>>
			{
				Descriptor = descriptor,

				AllocationDelegate = poolElementAllocationDelegate
			};

			return poolElementAllocationCommand;
		}
		
		public static AllocationCommand<IPoolElement<T>> BuildPoolElementAllocationCommand<T>(
			AllocationCommandDescriptor descriptor,
			Func<T> valueAllocationDelegate,
			MetadataAllocationDescriptor[] metadataDescriptors)
		{
			Func<IPoolElement<T>> poolElementAllocationDelegate = () =>
				BuildPoolElement(
					valueAllocationDelegate,
					metadataDescriptors);

			var poolElementAllocationCommand = new AllocationCommand<IPoolElement<T>>
			{
				Descriptor = descriptor,

				AllocationDelegate = poolElementAllocationDelegate
			};

			return poolElementAllocationCommand;
		}

		#endregion

		#region Pool element

		public static IPoolElement<T> BuildPoolElementWithAllocationCallback<T>(
			Func<T> allocationDelegate,
			MetadataAllocationDescriptor[] metadataDescriptors,
			IAllocationCallback<T> callback)
		{
			var metadata = BuildMetadataRepository(metadataDescriptors);
			
			var result = new PoolElement<T>(
				AllocationsFactory.FuncAllocationDelegate(allocationDelegate),
				metadata);

			callback?.OnAllocated(result);
			
			return result;
		}
		
		public static IPoolElement<T> BuildPoolElement<T>(
			Func<T> allocationDelegate,
			MetadataAllocationDescriptor[] metadataDescriptors)
		{
			var metadata = BuildMetadataRepository(metadataDescriptors);
			
			return new PoolElement<T>(
				AllocationsFactory.FuncAllocationDelegate(allocationDelegate),
				metadata);
		}

		#endregion
		
		#region Metadata

		public static IReadOnlyObjectRepository BuildMetadataRepository(MetadataAllocationDescriptor[] metadataDescriptors)
		{
			IRepository<Type, object> repository = RepositoriesFactory.BuildDictionaryRepository<Type, object>();

			if (metadataDescriptors != null)
				foreach (var descriptor in metadataDescriptors)
				{
					if (descriptor == null)
						continue;
					
					repository.Add(
						descriptor.BindingType,
						AllocationsFactory.ActivatorAllocationDelegate(descriptor.ConcreteType));
				}

			return RepositoriesFactory.BuildDictionaryObjectRepository(repository);
		}

		public static IndexedMetadata BuildIndexedMetadata()
		{
			return new IndexedMetadata();
		}

		public static MetadataAllocationDescriptor BuildIndexedMetadataDescriptor()
		{
			return new MetadataAllocationDescriptor
			{
				BindingType = typeof(IIndexed),
				ConcreteType = typeof(IndexedMetadata)
			};
		}

		#endregion
	}
}