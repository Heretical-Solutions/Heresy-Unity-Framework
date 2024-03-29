using System;
using System.IO;

using HereticalSolutions.Persistence.Arguments;
using HereticalSolutions.Persistence.IO;

using HereticalSolutions.Logging;

using ProtoBuf;
using ProtobufInternalSerializer = ProtoBuf.Serializer;

namespace HereticalSolutions.Persistence.Serializers
{
    public class SerializeProtobufIntoStreamStrategy
        : IProtobufSerializationStrategy
    {
        private readonly ILogger logger;

        public SerializeProtobufIntoStreamStrategy(
            ILogger logger = null)
        {
            this.logger = logger;
        }

        public bool Serialize(
            ISerializationArgument argument,
            Type valueType,
            object value)
        {
            FilePathSettings filePathSettings = ((StreamArgument)argument).Settings;
            
            if (!StreamIO.OpenWriteStream(
                filePathSettings,
                out FileStream fileStream,
                logger))
                return false;
            
            ProtobufInternalSerializer.Serialize(fileStream, value);
            
            //NOT WORKING
            //https://stackoverflow.com/questions/10510081/protobuf-net-argumentnullexception
            //ProtobufInternalSerializer.NonGeneric.SerializeWithLengthPrefix(fileStream, value, PrefixStyle.Base128, 1);
            
            StreamIO.CloseStream(fileStream);

            return true;
        }

        public bool Deserialize(
            ISerializationArgument argument,
            Type valueType,
            out object value)
        {
            FilePathSettings filePathSettings = ((StreamArgument)argument).Settings;

            if (!StreamIO.OpenReadStream(
                filePathSettings,
                out FileStream fileStream,
                logger))
            {
                value = default;
                
                return false;
            }

            value = ProtobufInternalSerializer.Deserialize(valueType, fileStream);
            
            //NOT WORKING
            //value = ProtobufInternalSerializer.NonGeneric.Deserialize(valueType, fileStream);
            
            StreamIO.CloseStream(fileStream);

            return true;
        }

        public void Erase(ISerializationArgument argument)
        {
            FilePathSettings filePathSettings = ((StreamArgument)argument).Settings;
            
            StreamIO.Erase(filePathSettings);
        }
    }
}