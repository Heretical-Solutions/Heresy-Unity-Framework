using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using HereticalSolutions.Persistence.Arguments;
using HereticalSolutions.Persistence.IO;

namespace HereticalSolutions.Persistence.Serializers
{
    public class UnitySerializeBinaryIntoStreamStrategy : IBinarySerializationStrategy
    {
        public bool Serialize(ISerializationArgument argument, BinaryFormatter formatter, object value)
        {
            UnityFileSystemSettings fileSystemSettings = ((UnityStreamArgument)argument).Settings;
            
            if (!UnityStreamIO.OpenWriteStream(fileSystemSettings, out FileStream fileStream))
                return false;
            
            formatter.Serialize(fileStream, value);
            
            UnityStreamIO.CloseStream(fileStream);

            return true;
        }

        public bool Deserialize(ISerializationArgument argument, BinaryFormatter formatter, out object value)
        {
            UnityFileSystemSettings fileSystemSettings = ((UnityStreamArgument)argument).Settings;

            if (!UnityStreamIO.OpenReadStream(fileSystemSettings, out FileStream fileStream))
            {
                value = default(object);
                
                return false;
            }

            value = formatter.Deserialize(fileStream);
            
            UnityStreamIO.CloseStream(fileStream);

            return true;
        }

        public void Erase(ISerializationArgument argument)
        {
            UnityFileSystemSettings fileSystemSettings = ((UnityStreamArgument)argument).Settings;
            
            UnityStreamIO.Erase(fileSystemSettings);
        }
    }
}