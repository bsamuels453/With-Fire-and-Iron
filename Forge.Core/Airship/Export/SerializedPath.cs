namespace Forge.Core.Airship.Export{
    public struct SerializedPath {
        const string _serializedRelPath = "Data/Serialized/";
        const string _serializedExtension = ".protocol";
        public readonly string Path;

        public SerializedPath(string airshipName){
            Path = _serializedRelPath + airshipName + _serializedExtension;
        }
    }
}