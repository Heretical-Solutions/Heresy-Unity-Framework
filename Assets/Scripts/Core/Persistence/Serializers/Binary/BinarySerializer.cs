using System;

using System.Runtime.Serialization.Formatters.Binary;

using HereticalSolutions.Repositories;

using HereticalSolutions.Logging;

namespace HereticalSolutions.Persistence.Serializers
{
    public class BinarySerializer : ISerializer
    {
        private readonly IReadOnlyObjectRepository strategyRepository;

        private readonly BinaryFormatter formatter = new BinaryFormatter();

        private readonly ILogger logger;

        public BinarySerializer(
            IReadOnlyObjectRepository strategyRepository,
            ILogger logger = null)
        {
            this.strategyRepository = strategyRepository;

            this.logger = logger;
        }

        #region ISerializer

        public bool Serialize<TValue>(
            ISerializationArgument argument,
            TValue DTO)
        {
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<BinarySerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IBinarySerializationStrategy)strategyObject;

            return concreteStrategy.Serialize(
                argument,
                formatter,
                DTO);
        }

        public bool Serialize(
            ISerializationArgument argument,
            Type DTOType,
            object DTO)
        {
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<BinarySerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IBinarySerializationStrategy)strategyObject;

            return concreteStrategy.Serialize(
                argument,
                formatter,
                DTO);
        }

        public bool Deserialize<TValue>(
            ISerializationArgument argument,
            out TValue DTO)
        {
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<BinarySerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IBinarySerializationStrategy)strategyObject;

            var result = concreteStrategy.Deserialize(
                argument,
                formatter,
                out object dtoObject);

            DTO = (TValue)dtoObject;

            return result;
        }

        public bool Deserialize(
            ISerializationArgument argument,
            Type DTOType,
            out object DTO)
        {
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<BinarySerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IBinarySerializationStrategy)strategyObject;

            return concreteStrategy.Deserialize(argument, formatter, out DTO);
        }

        public void Erase(ISerializationArgument argument)
        {
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<BinarySerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IBinarySerializationStrategy)strategyObject;

            concreteStrategy.Erase(argument);
        }

        #endregion
    }
}