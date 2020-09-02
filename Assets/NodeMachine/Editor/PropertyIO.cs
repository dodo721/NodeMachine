using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NodeMachine {

    public static class PropertyIO {
        
        /// <summary>
        ///  Generates a C# script for <c>props</c> and prompts Unity to compile it.
        ///  Returns false if the generated script has not changed.
        /// </summary>
        public static bool CompileProperties (NodeMachineModel model, CachedProperties props) {
            string propsName = GetFormattedName(model.name);
            string codeBase = File.ReadAllText(Application.dataPath + "/NodeMachine/Editor/NodeMachinePropertyBase.txt");
            codeBase = codeBase.Replace("<name>", propsName).Replace("<model_name>", model.name);

            HashSet<string> fpropStrings = new HashSet<string>();
            HashSet<string> ipropStrings = new HashSet<string>();
            HashSet<string> bpropStrings = new HashSet<string>();

            HashSet<string> getSwitchCases = new HashSet<string>();
            HashSet<string> setSwitchCases = new HashSet<string>();

            foreach (string floatProp in props._floats.Keys) {
                fpropStrings.Add("public float " + floatProp + " = " + props._floats[floatProp] + "f;");
                getSwitchCases.Add("case \"" + floatProp + "\": return " + floatProp + ";");
                setSwitchCases.Add("case \"" + floatProp + "\": " + floatProp + " = (float)val; break;");
            }
            foreach (string intProp in props._ints.Keys) {
                ipropStrings.Add("public int " + intProp + " = " + props._ints[intProp] + ";");
                getSwitchCases.Add("case \"" + intProp + "\": return " + intProp + ";");
                setSwitchCases.Add("case \"" + intProp + "\": " + intProp + " = (int)val; break;");
            }
            foreach (string boolProp in props._bools.Keys) {
                bpropStrings.Add("public bool " + boolProp + " = " + props._bools[boolProp].ToString().ToLowerInvariant() + ";");
                getSwitchCases.Add("case \"" + boolProp + "\": return " + boolProp + ";");
                setSwitchCases.Add("case \"" + boolProp + "\": " + boolProp + " = (bool)val; break;");
            }

            string fpropsCode = "";
            foreach (string propString in fpropStrings) {
                fpropsCode += "\t" + propString + "\n";
            }
            string ipropsCode = "";
            foreach (string propString in ipropStrings) {
                ipropsCode += "\t" + propString + "\n";
            }
            string bpropsCode = "";
            foreach (string propString in bpropStrings) {
                bpropsCode += "\t" + propString + "\n";
            }

            codeBase = codeBase.Replace("<float_props>", fpropsCode);
            codeBase = codeBase.Replace("<int_props>", ipropsCode);
            codeBase = codeBase.Replace("<bool_props>", bpropsCode);
            
            string getPropsCode = "";
            foreach (string getSwitchCase in getSwitchCases) {
                getPropsCode += "\t\t\t" + getSwitchCase + "\n";
            }
            string setPropsCode = "";
            foreach (string setSwitchCase in setSwitchCases) {
                setPropsCode += "\t\t\t" + setSwitchCase + "\n";
            }

            codeBase = codeBase.Replace("<GetProps>", getPropsCode);
            codeBase = codeBase.Replace("<SetProps>", setPropsCode);

            if (!Directory.Exists(Application.dataPath + "/NodeMachine/PropertyBuilds")) {
                Directory.CreateDirectory(Application.dataPath + "/NodeMachine/PropertyBuilds");
            }
            string savePath = Application.dataPath + "/NodeMachine/PropertyBuilds/" + propsName + ".cs";

            if (File.Exists(savePath)) {
                string existingCode = File.ReadAllText(savePath);
                if (existingCode == codeBase)
                    return false;
            }

            File.WriteAllText(savePath, codeBase);
            AssetDatabase.Refresh();

            return true;
        }

        public static string GetFormattedName (string name) {
            return Regex.Replace(name, @"[^a-zA-Z0-9]", "") + "Properties";
        }

    }

}