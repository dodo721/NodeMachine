
namespace NodeMachine {

    public interface IModelDataHolder
    {

        NodeMachineModel GetModel();
        void SetModel(NodeMachineModel model);

        CachedProperties GetProperties();
        void SetProperties(CachedProperties properties);

    }

}