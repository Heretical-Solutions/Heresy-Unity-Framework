using System;

using HereticalSolutions.Collections;

using HereticalSolutions.Allocations;
using HereticalSolutions.Allocations.Factories;

using HereticalSolutions.Delegates.Pinging;

using HereticalSolutions.Pools;
using HereticalSolutions.Pools.Factories;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Delegates.Factories
{
    public static partial class DelegatesFactory
    {
        private const int DEFAULT_PINGER_CAPACITY = 16;

        #region Pinger
        
        public static Pinger BuildPinger()
        {
            return new Pinger();
        }

        #endregion
        
        #region Non alloc pinger
        
        public static NonAllocPinger BuildNonAllocPinger(
            ILoggerResolver loggerResolver = null)
        {
            Func<ISubscription> valueAllocationDelegate = AllocationsFactory.NullAllocationDelegate<ISubscription>;

            var subscriptionsPool = PoolsFactory.BuildResizableNonAllocPool<ISubscription>(
                valueAllocationDelegate,
                false,

                new []
                {
                    PoolsFactory.BuildIndexedMetadataDescriptor()
                },
                new AllocationCommandDescriptor
                {
                    Rule = EAllocationAmountRule.ADD_PREDEFINED_AMOUNT,

                    Amount = DEFAULT_PINGER_CAPACITY
                },
                new AllocationCommandDescriptor
                {
                    Rule = EAllocationAmountRule.DOUBLE_AMOUNT
                },
                loggerResolver);

            return BuildNonAllocPinger(
                subscriptionsPool,
                loggerResolver);
        }

        public static NonAllocPinger BuildNonAllocPinger(
            AllocationCommandDescriptor initial,
            AllocationCommandDescriptor additional,
            ILoggerResolver loggerResolver = null)
        {
            Func<ISubscription> valueAllocationDelegate = AllocationsFactory.NullAllocationDelegate<ISubscription>;

            var subscriptionsPool = PoolsFactory.BuildResizableNonAllocPool<ISubscription>(
                valueAllocationDelegate,
                false,

                new []
                {
                    PoolsFactory.BuildIndexedMetadataDescriptor()
                },
                initial,
                additional,
                loggerResolver);

            return BuildNonAllocPinger(
                subscriptionsPool,
                loggerResolver);
        }

        public static NonAllocPinger BuildNonAllocPinger(
            INonAllocDecoratedPool<ISubscription> subscriptionsPool,
            ILoggerResolver loggerResolver = null)
        {
            var contents = ((IModifiable<INonAllocPool<ISubscription>>)subscriptionsPool).Contents;

            ILogger logger =
                loggerResolver?.GetLogger<NonAllocPinger>()
                ?? null;

            return new NonAllocPinger(
                subscriptionsPool,
                contents,
                logger);
        }
        
        #endregion
    }
}