using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace NodeMachine {

    public class CachedProperties
    {
        public Dictionary<string, float> _floats = new Dictionary<string, float>();
        public Dictionary<string, int> _ints = new Dictionary<string, int>();
        public Dictionary<string, bool> _bools = new Dictionary<string, bool>();

        public string[] GetPropNamesForType(Condition.ConditionType type)
        {
            string[] propNames = null;
            if (type == Condition.ConditionType.FLOAT)
            {
                propNames = _floats.Keys.ToArray();
            }
            else if (type == Condition.ConditionType.INT)
            {
                propNames = _ints.Keys.ToArray();
            }
            else if (type == Condition.ConditionType.BOOL)
            {
                propNames = _bools.Keys.ToArray();
            }
            return propNames;
        }

        public CachedProperties DeepCopy () {
            CachedProperties properties = new CachedProperties();
            foreach (string fProp in _floats.Keys) {
                properties._floats.Add(fProp, _floats[fProp]);
            }
            foreach (string iProp in _ints.Keys) {
                properties._ints.Add(iProp, _ints[iProp]);
            }
            foreach (string bProp in _bools.Keys) {
                properties._bools.Add(bProp, _bools[bProp]);
            }
            return properties;
        }

        public bool CompareSchema (CachedProperties other) {
            if (other == null)
                return false;
            return
                other._floats.Count == _floats.Count &&
                other._floats.Keys.SequenceEqual(_floats.Keys) &&
                other._ints.Count == _ints.Count &&
                other._ints.Keys.SequenceEqual(_ints.Keys) &&
                other._bools.Count == _bools.Count &&
                other._bools.Keys.SequenceEqual(_bools.Keys);
        }

        public void AddFloat(string name, float val)
        {
            _floats.Add(name, val);
        }

        public void AddInt(string name, int val)
        {
            _ints.Add(name, val);
        }

        public void AddBool(string name, bool val)
        {
            _bools.Add(name, val);
        }

        public void RemoveFloat(string name)
        {
            _floats.Remove(name);
        }

        public void RemoveInt(string name)
        {
            _ints.Remove(name);
        }

        public void RemoveBool(string name)
        {
            _bools.Remove(name);
        }

        public void SetFloat(string name, float val)
        {
            _floats[name] = val;
        }

        public void SetInt(string name, int val)
        {
            _ints[name] = val;
        }

        public void SetBool(string name, bool val)
        {
            _bools[name] = val;
        }

        public bool ContainsProp (string name) {
            return _floats.ContainsKey(name) || _ints.ContainsKey(name) || _bools.ContainsKey(name);
        }

        public static PropsSerialized Serialize(CachedProperties p)
        {
            PropsSerialized props = new PropsSerialized();
            props.floats.LoadDictionary(p._floats);
            props.ints.LoadDictionary(p._ints);
            props.bools.LoadDictionary(p._bools);
            return props;
        }

        public static CachedProperties Deserialize(PropsSerialized props)
        {
            CachedProperties p = new CachedProperties();
            for (int i = 0; i < props.floats.keys.Count; i++)
            {
                p._floats.Add(props.floats.keys[i], props.floats.vals[i]);
            }
            for (int i = 0; i < props.ints.keys.Count; i++)
            {
                p._ints.Add(props.ints.keys[i], props.ints.vals[i]);
            }
            for (int i = 0; i < props.bools.keys.Count; i++)
            {
                p._bools.Add(props.bools.keys[i], props.bools.vals[i]);
            }
            return p;
        }

        [Serializable]
        public class PropsSerialized
        {
            public FloatSerialProp floats = new FloatSerialProp();
            public IntSerialProp ints = new IntSerialProp();
            public BoolSerialProp bools = new BoolSerialProp();

            public void SerializeFloats(Dictionary<string, float> props)
            {
                floats.keys.Clear();
                floats.vals.Clear();
                foreach (string key in props.Keys)
                {
                    floats.keys.Add(key);
                    floats.vals.Add(props[key]);
                }
            }

            public void SerializeInts(Dictionary<string, int> props)
            {
                ints.keys.Clear();
                ints.vals.Clear();
                foreach (string key in props.Keys)
                {
                    ints.keys.Add(key);
                    ints.vals.Add(props[key]);
                }
            }

            public void SerializeBools(Dictionary<string, bool> props)
            {
                bools.keys.Clear();
                bools.vals.Clear();
                foreach (string key in props.Keys)
                {
                    bools.keys.Add(key);
                    bools.vals.Add(props[key]);
                }
            }

            public void Serialize(CachedProperties props)
            {
                SerializeFloats(props._floats);
                SerializeInts(props._ints);
                SerializeBools(props._bools);
            }

            public PropsSerialized DeepCopy()
            {
                PropsSerialized copy = new PropsSerialized();
                copy.floats = floats.DeepCopy() as FloatSerialProp;
                copy.ints = ints.DeepCopy() as IntSerialProp;
                copy.bools = bools.DeepCopy() as BoolSerialProp;
                return copy;
            }
        }

        [Serializable]
        public class SerialProp<T>
        {
            public List<string> keys = new List<string>();
            public List<T> vals = new List<T>();

            public void LoadDictionary(Dictionary<string, T> props)
            {
                this.keys = new List<string>(props.Keys);
                this.vals = new List<T>(props.Values);
            }

            public SerialProp<T> DeepCopy()
            {
                List<string> copyKeys = new List<string>();
                foreach (string key in keys)
                {
                    copyKeys.Add(key);
                }
                List<T> copyVals = new List<T>();
                foreach (T val in vals)
                {
                    copyVals.Add(val);
                }
                SerialProp<T> copyProp = new SerialProp<T>();
                copyProp.keys = copyKeys;
                copyProp.vals = copyVals;
                return copyProp;
            }
        }

        [Serializable]
        public class FloatSerialProp : SerialProp<float> { }

        [Serializable]
        public class IntSerialProp : SerialProp<int> { }

        [Serializable]
        public class BoolSerialProp : SerialProp<bool> { }

    }

}