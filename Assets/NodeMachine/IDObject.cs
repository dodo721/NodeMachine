using System;
using UnityEngine;

namespace NodeMachine {

    [Serializable]
    public abstract class IDObject
    {

        [SerializeField]
        private int _id;
        public int ID
        {
            get { return _id; }
        }

        public IDObject(int id)
        {
            this._id = id;
        }

    }

}