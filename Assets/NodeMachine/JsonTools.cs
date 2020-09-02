using SimpleJSON;
using UnityEngine;
using System;

namespace NodeMachine.Util {

    public class JsonGenericWrapper <T> {
        public T FromJson (string json) {
            return JsonUtility.FromJson<T>(json);
        }
    }

    public static class JsonHelper {
        public static T[] FromJson<T>(string json) {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array) {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson (Type arrayType, object[] array) {
            string json = "{\"Type\":\"" + arrayType.AssemblyQualifiedName + "\", \"Items\":[";
            for (int i = 0; i < array.Length; i++) {
                dynamic e = Convert.ChangeType(array[i], arrayType);
                string eJson = JsonUtility.ToJson(e, true);
                json += eJson + (i == array.Length - 1 ? "" : ",");
            }
            json += "]}";
            return json;
        }

        public static string ToJson<T>(T[] array, bool prettyPrint) {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T> {
            public T[] Items;
        }
    }

}