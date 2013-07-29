namespace Forge.Core.Airship.Export{
    public struct AirshipStatePath{
        const string _stateRelPath = "Data/";
        const string _stateExtension = ".json";
        public readonly string Path;

        public AirshipStatePath(string stateIdentifier){
            //todo: add assert to make sure this file exists on that path
            Path = _stateRelPath + stateIdentifier + _stateExtension;
        }
    }
}