using System;

using HereticalSolutions.Repositories;

using HereticalSolutions.Pools.Arguments;
using HereticalSolutions.Pools.Behaviours;

using HereticalSolutions.RandomGeneration;

using HereticalSolutions.LifetimeManagement;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Pools.Decorators
{
    public class NonAllocPoolWithVariants<T>
        : INonAllocDecoratedPool<T>,
          ICleanUppable,
          IDisposable
    {
        private readonly IReadOnlyRepository<int, VariantContainer<T>> innerPoolsRepository;

        private readonly IRandomGenerator randomGenerator;

        private readonly IPushBehaviourHandler<T> pushBehaviourHandler;

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonAllocPoolWithVariants{T}"/> class.
        /// </summary>
        /// <param name="innerPoolsRepository">The repository containing the inner pools.</param>
        /// <param name="randomGenerator">The random number generator.</param>
        public NonAllocPoolWithVariants(
            IReadOnlyRepository<int, VariantContainer<T>> innerPoolsRepository,
            IRandomGenerator randomGenerator,
            ILogger logger = null)
        {
            this.innerPoolsRepository = innerPoolsRepository;

            this.randomGenerator = randomGenerator;

            this.logger = logger;

            pushBehaviourHandler = new PushToDecoratedPoolBehaviour<T>(this);
        }

        #region Pop

        public IPoolElement<T> Pop(IPoolDecoratorArgument[] args)
        {
            #region Variant from argument

            if (args.TryGetArgument<VariantArgument>(out var arg))
            {
                if (!innerPoolsRepository.TryGet(arg.Variant, out var variant))
                    throw new Exception(
                        logger.TryFormat<NonAllocPoolWithVariants<T>>(
                            $"INVALID VARIANT {{ {arg.Variant} }}"));

                var concreteResult = variant.Pool.Pop(args);

                // Update element data
                var variantElementAsPushable = (IPushable<T>)concreteResult;

                variantElementAsPushable.UpdatePushBehaviour(pushBehaviourHandler);

                return concreteResult;
            }

            #endregion

            #region Validation

            if (!innerPoolsRepository.TryGet(0, out var currentVariant))
                throw new Exception(
                    logger.TryFormat<NonAllocPoolWithVariants<T>>(
                        "NO VARIANTS PRESENT"));

            #endregion

            #region Random variant

            var hitDice = randomGenerator.Random(0, 1f);
            int index = 0;

            while (currentVariant.Chance < hitDice)
            {
                hitDice -= currentVariant.Chance;
                index++;

                if (!innerPoolsRepository.TryGet(index, out currentVariant))
                    throw new Exception(
                        logger.TryFormat<NonAllocPoolWithVariants<T>>(
                            "INVALID VARIANT CHANCES"));
            }

            var result = currentVariant.Pool.Pop(args);

            // Update element data
            var elementAsPushable = (IPushable<T>)result;

            elementAsPushable.UpdatePushBehaviour(pushBehaviourHandler);

            return result;

            #endregion
        }

        #endregion

        #region Push

        public void Push(
            IPoolElement<T> instance,
            bool decoratorsOnly = false)
        {
            int variant = instance.Metadata.Get<IContainsVariant>().Variant;

            if (!innerPoolsRepository.TryGet(variant, out var poolByVariant))
                throw new Exception(
                    logger.TryFormat<NonAllocPoolWithVariants<T>>(
                        $"INVALID VARIANT {{variant}}"));

            poolByVariant.Pool.Push(instance, decoratorsOnly);
        }

        #endregion

        #region ICleanUppable

        public void Cleanup()
        {
            if (innerPoolsRepository is ICleanUppable)
                (innerPoolsRepository as ICleanUppable).Cleanup();

            if (pushBehaviourHandler is ICleanUppable)
                (pushBehaviourHandler as ICleanUppable).Cleanup();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (innerPoolsRepository is IDisposable)
                (innerPoolsRepository as IDisposable).Dispose();

            if (pushBehaviourHandler is IDisposable)
                (pushBehaviourHandler as IDisposable).Dispose();
        }

        #endregion
    }
}