
namespace NodeMachine {

    public interface IModelDataAndPropsHolder : IModelDataHolder
    {

        CachedProperties.PropsSerialized GetPropsSerialized();
        void SetPropsSerialized(CachedProperties.PropsSerialized propsSerialized);

    }

}