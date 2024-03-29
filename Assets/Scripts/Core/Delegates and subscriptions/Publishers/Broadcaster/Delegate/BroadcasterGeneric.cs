using System;
using System.Collections.Generic;
using System.Linq; //error CS1061: 'Delegate[]' does not contain a definition for 'Cast'

using HereticalSolutions.LifetimeManagement;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Delegates.Broadcasting
{
    public class BroadcasterGeneric<TValue>
        : IPublisherSingleArgGeneric<TValue>,
          IPublisherSingleArg,
          ISubscribableSingleArgGeneric<TValue>,
          ISubscribableSingleArg,
          ICleanUppable,
          IDisposable
    {
        private readonly ILogger logger;

        private Action<TValue> multicastDelegate;

        public BroadcasterGeneric(
            ILogger logger = null)
        {
            this.logger = logger;

            multicastDelegate = null;
        }

        #region IPublisherSingleArgGeneric

        public void Publish(TValue value)
        {
            //If any delegate that is invoked attempts to unsubscribe itself, it would produce an error because the collection
            //should NOT be changed during the invokation
            //That's why we'll copy the multicast delegate to a local variable and invoke it from there
            //multicastDelegate?.Invoke(value);

            var multicastDelegateCopy = multicastDelegate;

            multicastDelegateCopy?.Invoke(value);

            multicastDelegateCopy = null;
        }

        #endregion

        #region IPublisherSingleArg

        public void Publish<TArgument>(TArgument value)
        {
            //LOL, pattern matching to the rescue of converting TArgument to TValue
            switch (value)
            {
                case TValue tValue:

                    multicastDelegate?.Invoke(tValue);

                    break;

                default:

                    throw new Exception(
                        logger.TryFormat<BroadcasterGeneric<TValue>>(
                            $"INVALID ARGUMENT TYPE. EXPECTED: \"{typeof(TValue).Name}\" RECEIVED: \"{typeof(TArgument).Name}\""));
            }
        }

        public void Publish(Type valueType, object value)
        {
            //LOL, pattern matching to the rescue of converting TArgument to TValue
            switch (value)
            {
                case TValue tValue:

                    multicastDelegate?.Invoke(tValue);

                    break;

                default:

                    throw new Exception(
                        logger.TryFormat<BroadcasterGeneric<TValue>>(
                            $"INVALID ARGUMENT TYPE. EXPECTED: \"{typeof(TValue).Name}\" RECEIVED: \"{valueType.Name}\""));
            }
        }

        #endregion

        #region ISubscribableSingleArgGeneric

        public void Subscribe(Action<TValue> @delegate)
        {
            multicastDelegate += @delegate;
        }

        public void Subscribe(object @delegate)
        {
            multicastDelegate += (Action<TValue>)@delegate;
        }

        public void Unsubscribe(Action<TValue> @delegate)
        {
            multicastDelegate -= @delegate;
        }

        public void Unsubscribe(object @delegate)
        {
            multicastDelegate -= (Action<TValue>)@delegate;
        }

        IEnumerable<Action<TValue>> ISubscribableSingleArgGeneric<TValue>.AllSubscriptions
        {
            get
            {
                //Kudos to Copilot for Cast() and the part after the ?? operator
                return multicastDelegate?.GetInvocationList().Cast<Action<TValue>>() ?? Enumerable.Empty<Action<TValue>>();
            }
        }

        #endregion

        #region ISubscribableSingleArg

        public void Subscribe<TArgument>(Action<TArgument> @delegate)
        {
            //LOL, pattern matching to the rescue of converting TArgument to TValue
            switch (@delegate)
            {
                case Action<TValue> tValue:

                    multicastDelegate += tValue;

                    break;

                default:

                    throw new Exception(
                        logger.TryFormat<BroadcasterGeneric<TValue>>(
                            $"INVALID ARGUMENT TYPE. EXPECTED: \"{typeof(TValue).Name}\" RECEIVED: \"{typeof(TArgument).Name}\""));
            }
        }

        public void Subscribe(Type valueType, object @delegate)
        {
            //LOL, pattern matching to the rescue of converting TArgument to TValue
            switch (@delegate)
            {
                case Action<TValue> tValue:

                    multicastDelegate += tValue;

                    break;

                default:

                    throw new Exception(
                        logger.TryFormat<BroadcasterGeneric<TValue>>(
                            $"INVALID ARGUMENT TYPE. EXPECTED: \"{typeof(TValue).Name}\" RECEIVED: \"{valueType.Name}\""));
            }
        }

        public void Unsubscribe<TArgument>(Action<TArgument> @delegate)
        {
            //LOL, pattern matching to the rescue of converting TArgument to TValue
            switch (@delegate)
            {
                case Action<TValue> tValue:

                    multicastDelegate -= tValue; //TODO: ensure works properly

                    break;

                default:

                    throw new Exception(
                        logger.TryFormat<BroadcasterGeneric<TValue>>(
                            $"INVALID ARGUMENT TYPE. EXPECTED: \"{typeof(TValue).Name}\" RECEIVED: \"{typeof(TArgument).Name}\""));
            }
        }

        public void Unsubscribe(Type valueType, object @delegate)
        {
            //LOL, pattern matching to the rescue of converting TArgument to TValue
            switch (@delegate)
            {
                case Action<TValue> tValue:

                    multicastDelegate -= tValue;

                    break;

                default:

                    throw new Exception(
                        logger.TryFormat<BroadcasterGeneric<TValue>>(
                            $"INVALID ARGUMENT TYPE. EXPECTED: \"{typeof(TValue).Name}\" RECEIVED: \"{valueType.Name}\""));
            }
        }

        public IEnumerable<Action<TArgument>> GetAllSubscriptions<TArgument>()
        {
            //Kudos to Copilot for Cast() and the part after the ?? operator
            return multicastDelegate?.GetInvocationList().Cast<Action<TArgument>>() ?? Enumerable.Empty<Action<TArgument>>();
        }

        public IEnumerable<object> GetAllSubscriptions(Type valueType)
        {
            //Kudos to Copilot for Cast() and the part after the ?? operator
            return multicastDelegate?.GetInvocationList().Cast<object>() ?? Enumerable.Empty<object>();
        }

        #endregion

        #region ISubscribable

        IEnumerable<object> ISubscribable.AllSubscriptions
        {
            get
            {
                //Kudos to Copilot for Cast() and the part after the ?? operator
                return multicastDelegate?.GetInvocationList().Cast<object>() ?? Enumerable.Empty<object>();
            }
        }

        public void UnsubscribeAll()
        {
            multicastDelegate = null;
        }

        #endregion

        #region ICleanUppable

        public void Cleanup()
        {
            multicastDelegate = null;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            multicastDelegate = null;
        }

        #endregion
    }
}