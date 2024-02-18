using System;

using HereticalSolutions.Allocations.Factories;

using HereticalSolutions.Pools;
using HereticalSolutions.Pools.AllocationCallbacks;
using HereticalSolutions.Pools.Factories;

using HereticalSolutions.Synchronization;

using HereticalSolutions.Metadata.Allocations;

using HereticalSolutions.Logging;

using UnityEngine;

using Zenject;

namespace HereticalSolutions.Samples.PoolWithAddressVariantAndTimerSample
{
    public static class SamplePoolFactory
    {
        public static INonAllocDecoratedPool<GameObject> BuildPool(
            DiContainer container,
            SamplePoolSettings settings,
            ISynchronizationProvider synchronizationProvider,
            Transform parentTransform = null,
            ILoggerResolver loggerResolver = null)
        {
            #region Builders

            // Create a builder for pools with address.
            var poolWithAddressBuilder = new PoolWithAddressBuilder<GameObject>(
                loggerResolver,
                loggerResolver?.GetLogger<PoolWithAddressBuilder<GameObject>>());

            // Create a builder for pools with variants.
            var poolWithVariantsBuilder = new PoolWithVariantsBuilder<GameObject>(
                loggerResolver,
                loggerResolver?.GetLogger<PoolWithVariantsBuilder<GameObject>>());

            // Create a builder for resizable pools.
            var resizablePoolBuilder = new ResizablePoolBuilder<GameObject>(
                loggerResolver,
                loggerResolver?.GetLogger<ResizablePoolBuilder<GameObject>>());

            #endregion

            #region Callbacks

            // Create a push to decorated pool callback.
            PushToDecoratedPoolCallback<GameObject> pushCallback =
                PoolsFactory.BuildPushToDecoratedPoolCallback<GameObject>(
                    PoolsFactory.BuildDeferredCallbackQueue<GameObject>());

            #endregion

            #region Metadata descriptor builders

            // Create an array of metadata descriptor builder functions.
            var metadataDescriptorBuilders = new Func<MetadataAllocationDescriptor>[]
            {
                PoolsFactory.BuildIndexedMetadataDescriptor,
                AddressDecoratorsPoolsFactory.BuildAddressMetadataDescriptor,
                VariantsDecoratorsPoolsFactory.BuildVariantMetadataDescriptor,
                TimersDecoratorsPoolsFactory.BuildRuntimeTimerMetadataDescriptor
            };

            #endregion

            // Initialize the pool with address builder.
            poolWithAddressBuilder.Initialize();

            foreach (var element in settings.Elements)
            {
                #region Address

                // Get the full address of the element
                string fullAddress = element.Name;

                // Convert the address to hashes.
                int[] addressHashes = fullAddress.AddressToHashes();

                // Build a set address callback.
                SetAddressCallback<GameObject> setAddressCallback = AddressDecoratorsPoolsFactory.BuildSetAddressCallback<GameObject>(
                    fullAddress,
                    addressHashes);

                #endregion

                // Initialize the pool with variants builder.
                poolWithVariantsBuilder.Initialize();

                for (int i = 0; i < element.Variants.Length; i++)
                {
                    #region Variant

                    var currentVariant = element.Variants[i];

                    // Build the name of the current variant.
                    string currentVariantName = $"{fullAddress} (Variant {i.ToString()})";

                    // Build a set variant callback.
                    SetVariantCallback<GameObject> setVariantCallback =
                        VariantsDecoratorsPoolsFactory.BuildSetVariantCallback<GameObject>(i);

                    // Build a set runtime timer callback.
                    SetRuntimeTimerCallback<GameObject> setRuntimeTimerCallback =
                        TimersDecoratorsPoolsFactory.BuildSetRuntimeTimerCallback<GameObject>(
                            $"{currentVariantName} Timer",
                            currentVariant.Duration,
                            loggerResolver);

                    // Build a rename callback.
                    RenameByStringAndIndexCallback renameCallback =
                        UnityDecoratorsPoolsFactory.BuildRenameByStringAndIndexCallback(currentVariantName);

                    #endregion

                    #region Allocation callbacks initialization

                    // Create an array of allocation callbacks.
                    var callbacks = new IAllocationCallback<GameObject>[]
                    {
                        renameCallback,
                        setAddressCallback,
                        setVariantCallback,
                        setRuntimeTimerCallback,
                        pushCallback
                    };

                    #endregion

                    #region Value allocation delegate initialization

                    // Get the prefab of the current variant.
                    var prefab = currentVariant.Prefab;

                    // Create a value allocation delegate.
                    Func<GameObject> valueAllocationDelegate =
                        () => UnityZenjectAllocationsFactory.DIResolveOrInstantiateAllocationDelegate(
                            container,
                            prefab);

                    #endregion

                    // Initialize the resizable pool builder.
                    resizablePoolBuilder.Initialize(
                        valueAllocationDelegate,
                        metadataDescriptorBuilders,
                        currentVariant.Initial,
                        currentVariant.Additional,
                        callbacks);

                    // Build the resizable pool.
                    var resizablePool = resizablePoolBuilder.BuildResizablePool();

                    // Build the game object pool.
                    var gameObjectPool = UnityDecoratorsPoolsFactory.BuildNonAllocGameObjectPool(
                        resizablePool,
                        parentTransform);

                    // Build the prefab instance pool.
                    var prefabInstancePool = UnityDecoratorsPoolsFactory.BuildNonAllocPrefabInstancePool(
                        gameObjectPool,
                        prefab);

                    // Build the pool with runtime timers.
                    var poolWithRuntimeTimers = TimersDecoratorsPoolsFactory.BuildNonAllocPoolWithRuntimeTimer(
                        prefabInstancePool,
                        synchronizationProvider,
                        loggerResolver);

                    // Add the variant to the pool with variants builder.
                    poolWithVariantsBuilder.AddVariant(
                        i,
                        currentVariant.Chance,
                        poolWithRuntimeTimers);
                }

                // Build the variant pool.
                var variantPool = poolWithVariantsBuilder.Build();

                // Parse the address and variant pool to the pool with address builder.
                poolWithAddressBuilder.Parse(
                    fullAddress,
                    variantPool);
            }

            // Build the pool with address.
            var poolWithAddress = poolWithAddressBuilder.Build();

            // Build the pool with ID.
            var poolWithID = IDDecoratorsPoolsFactory.BuildNonAllocPoolWithID(
                poolWithAddress,
                settings.ID,
                loggerResolver);

            // Set the root of the push callback.
            pushCallback.Root = poolWithID;

            return poolWithID;
        }
    }
}