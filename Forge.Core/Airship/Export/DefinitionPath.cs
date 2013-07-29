namespace Forge.Core.Airship.Export{
    public struct DefinitionPath {
        const string _definitionRelPath = "Data/Definitions/";
        const string _definitionExtension = ".def";
        public readonly string Path;

        public DefinitionPath(string airshipName){
            //todo: add assert to make sure this file exists on that path
            Path = _definitionRelPath + airshipName + _definitionExtension;
        }
    }
}