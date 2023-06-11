using HereticalSolutions.Persistence.Arguments;
using HereticalSolutions.Persistence.IO;

namespace HereticalSolutions.Persistence.Serializers
{
    public class UnitySerializeYamlIntoTextFileStrategy : IYamlSerializationStrategy
    {
        public bool Serialize(ISerializationArgument argument, string yaml)
        {
            UnityFileSystemSettings fileSystemSettings = ((UnityTextFileArgument)argument).Settings;
            
            return UnityTextFileIO.Write(fileSystemSettings, yaml);
        }

        public bool Deserialize(ISerializationArgument argument, out string yaml)
        {
            UnityFileSystemSettings fileSystemSettings = ((UnityTextFileArgument)argument).Settings;
            
            return UnityTextFileIO.Read(fileSystemSettings, out yaml);
        }
        
        public void Erase(ISerializationArgument argument)
        {
            UnityFileSystemSettings fileSystemSettings = ((UnityTextFileArgument)argument).Settings;
            
            UnityTextFileIO.Erase(fileSystemSettings);
        }
    }
}