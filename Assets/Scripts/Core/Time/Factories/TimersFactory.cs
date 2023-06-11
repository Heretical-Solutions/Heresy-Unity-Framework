using System;
using HereticalSolutions.Delegates.Factories;

using HereticalSolutions.Repositories;
using HereticalSolutions.Repositories.Factories;

using HereticalSolutions.Time.Strategies;
using HereticalSolutions.Time.Timers;

namespace HereticalSolutions.Time.Factories
{
    public static partial class TimeFactory
    {
        #region Persistent timer

        public static PersistentTimer BuildPersistentTimer(
            string id,
            TimeSpan defaultDurationSpan)
        {
            var onStart = DelegatesFactory.BuildNonAllocBroadcasterGeneric<IPersistentTimer>();
            
            var onFinish = DelegatesFactory.BuildNonAllocBroadcasterGeneric<IPersistentTimer>();
            
            return new PersistentTimer(
                id,
                defaultDurationSpan,
                
                onStart,
                onStart,
                
                onFinish,
                onFinish,
                
                BuildPersistentStrategyRepository());
        }

        private static IReadOnlyRepository<ETimerState, ITimerStrategy<IPersistentTimerContext, TimeSpan>>
            BuildPersistentStrategyRepository()
        {
            var repository = RepositoriesFactory.BuildDictionaryRepository<ETimerState, ITimerStrategy<IPersistentTimerContext, TimeSpan>>(
                new ETimerStateComparer());
            
            repository.Add(ETimerState.INACTIVE, new PersistentInactiveStrategy());
            
            repository.Add(ETimerState.STARTED, new PersistentStartedStrategy());
            
            repository.Add(ETimerState.PAUSED, new PersistentPausedStrategy());
            
            repository.Add(ETimerState.FINISHED, new PersistentFinishedStrategy());

            return repository;
        }

        #endregion
        
        #region Runtime timer
        
        public static RuntimeTimer BuildRuntimeTimer(
            string id,
            float defaultDuration)
        {
            var onStart = DelegatesFactory.BuildNonAllocBroadcasterGeneric<IRuntimeTimer>();
            
            var onFinish = DelegatesFactory.BuildNonAllocBroadcasterGeneric<IRuntimeTimer>();
            
            return new RuntimeTimer(
                id,
                defaultDuration,
                
                onStart,
                onStart,
                
                onFinish,
                onFinish,
                
                BuildRuntimeStrategyRepository());
        }

        private static IReadOnlyRepository<ETimerState, ITimerStrategy<IRuntimeTimerContext, float>>
            BuildRuntimeStrategyRepository()
        {
            var repository = RepositoriesFactory.BuildDictionaryRepository<ETimerState, ITimerStrategy<IRuntimeTimerContext, float>>(
                new ETimerStateComparer());
            
            repository.Add(ETimerState.INACTIVE, new RuntimeInactiveStrategy());
            
            repository.Add(ETimerState.STARTED, new RuntimeStartedStrategy());
            
            repository.Add(ETimerState.PAUSED, new RuntimePausedStrategy());
            
            repository.Add(ETimerState.FINISHED, new RuntimeFinishedStrategy());

            return repository;
        }
        
        #endregion
    }
}