using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NodeMachine {

    public class ModelLoader
    {

        private IModelDataHolder _holder;

        public ModelLoader(IModelDataHolder holder)
        {
            this._holder = holder;
        }

        public void LoadModelProperties(NodeMachineModel model)
        {
            _holder.SetModel(model);
            if (model == null)
            {
                _holder.SetProperties(null);
                return;
            }

            if (!(_holder is IModelDataAndPropsHolder))
                _holder.SetProperties(CachedProperties.Deserialize(_holder.GetModel().properties));
            else
            {
                IModelDataAndPropsHolder propsHolder = _holder as IModelDataAndPropsHolder;

                // Check local copy against model properties, and remove/add any removed/added properties
                CachedProperties modelProps = CachedProperties.Deserialize(_holder.GetModel().properties);
                CachedProperties.PropsSerialized localPropsSerialized = propsHolder.GetPropsSerialized();

                if (localPropsSerialized == null)
                {
                    propsHolder.SetProperties(modelProps); // TODO: Needed?
                    propsHolder.SetPropsSerialized(propsHolder.GetModel().properties.DeepCopy());
                    propsHolder.SetProperties(CachedProperties.Deserialize(propsHolder.GetPropsSerialized()));
                }
                else
                {
                    CachedProperties localProps = CachedProperties.Deserialize(localPropsSerialized);
                    // Add new properties
                    foreach (string prop in modelProps._floats.Keys)
                    {
                        if (!localProps._floats.ContainsKey(prop))
                        {
                            localProps._floats.Add(prop, modelProps._floats[prop]);
                        }
                    }
                    foreach (string prop in modelProps._ints.Keys)
                    {
                        if (!localProps._ints.ContainsKey(prop))
                        {
                            localProps._ints.Add(prop, modelProps._ints[prop]);
                        }
                    }
                    foreach (string prop in modelProps._bools.Keys)
                    {
                        if (!localProps._bools.ContainsKey(prop))
                        {
                            localProps._bools.Add(prop, modelProps._bools[prop]);
                        }
                    }

                    // Remove old properties
                    List<string> propsToRemove = new List<string>();
                    foreach (string prop in localProps._floats.Keys)
                    {
                        if (!modelProps._floats.ContainsKey(prop))
                        {
                            propsToRemove.Add(prop);
                        }
                    }
                    foreach (string prop in propsToRemove)
                    {
                        localProps._floats.Remove(prop);
                    }
                    propsToRemove.Clear();
                    foreach (string prop in localProps._ints.Keys)
                    {
                        if (!modelProps._ints.ContainsKey(prop))
                        {
                            propsToRemove.Add(prop);
                        }
                    }
                    foreach (string prop in propsToRemove)
                    {
                        localProps._ints.Remove(prop);
                    }
                    propsToRemove.Clear();
                    foreach (string prop in localProps._bools.Keys)
                    {
                        if (!modelProps._bools.ContainsKey(prop))
                        {
                            propsToRemove.Add(prop);
                        }
                    }
                    foreach (string prop in propsToRemove)
                    {
                        localProps._bools.Remove(prop);
                    }
                    propsHolder.SetProperties(localProps);
                    propsHolder.GetPropsSerialized().Serialize(localProps);
                }
            }
        }

    }

}