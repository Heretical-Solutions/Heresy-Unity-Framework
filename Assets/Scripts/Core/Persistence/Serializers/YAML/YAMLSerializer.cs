using System;

using HereticalSolutions.Repositories;

using HereticalSolutions.Logging;

using YamlDotNet.Serialization;

namespace HereticalSolutions.Persistence.Serializers
{
    public class YAMLSerializer : ISerializer
    {
        private readonly YamlDotNet.Serialization.ISerializer yamlSerializer;

        private readonly YamlDotNet.Serialization.IDeserializer yamlDeserializer;

        private readonly IReadOnlyObjectRepository strategyRepository;

        private readonly ILogger logger;

        public YAMLSerializer(
            IReadOnlyObjectRepository strategyRepository,
            ILogger logger = null)
        {
            yamlSerializer = new SerializerBuilder().Build();

            yamlDeserializer = new DeserializerBuilder().Build();

            this.strategyRepository = strategyRepository;

            this.logger = logger;
        }

        #region ISerializer
        
        public bool Serialize<TValue>(
            ISerializationArgument argument,
            TValue DTO)
        {
            string yaml = yamlSerializer.Serialize(DTO);
            
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<YAMLSerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IYamlSerializationStrategy)strategyObject;

            return concreteStrategy.Serialize(
                argument,
                yaml);
        }

        public bool Serialize(
            ISerializationArgument argument,
            Type DTOType,
            object DTO)
        {
            string yaml = yamlSerializer.Serialize(DTO);
            
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<YAMLSerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IYamlSerializationStrategy)strategyObject;

            return concreteStrategy.Serialize(
                argument,
                yaml);
        }

        public bool Deserialize<TValue>(
            ISerializationArgument argument,
            out TValue DTO)
        {
            DTO = (TValue)Activator.CreateInstance(typeof(TValue));
            
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<YAMLSerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IYamlSerializationStrategy)strategyObject;

            if (!concreteStrategy.Deserialize(
                argument,
                out var yaml))
                return false;

            DTO = yamlDeserializer.Deserialize<TValue>(yaml);
            
            return true;
        }

        public bool Deserialize(
            ISerializationArgument argument,
            Type DTOType,
            out object DTO)
        {
            DTO = Activator.CreateInstance(DTOType);
            
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<YAMLSerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IYamlSerializationStrategy)strategyObject;

            if (!concreteStrategy.Deserialize(
                argument,
                out var yaml))
                return false;

            DTO = yamlDeserializer.Deserialize(yaml, DTOType);

            return true;
        }

        public void Erase(ISerializationArgument argument)
        {
            if (!strategyRepository.TryGet(
                argument.GetType(),
                out var strategyObject))
                throw new Exception(
                    logger.TryFormat<YAMLSerializer>(
                        $"COULD NOT RESOLVE STRATEGY BY ARGUMENT: {argument.GetType().Name}"));

            var concreteStrategy = (IYamlSerializationStrategy)strategyObject;
            
            concreteStrategy.Erase(argument);
        }
        
        #endregion
    }
}