using System;
using UnityEngine;

namespace NodeMachine {

    [Serializable]
    public class Condition : ISerializationCallbackReceiver {
        
        public enum Comparison {
            EQUAL, NOT_EQUAL, GREATER_THAN, GREATER_OR_EQUAL, LESS_THAN, LESS_OR_EQUAL
        }
        
        public enum ConditionType {
            FLOAT, INT, BOOL, STRING
        }

        public enum CompareTo {
            CONSTANT, PROP
        }

        public ConditionType _valueType;
        public Comparison _comparison;
        public CompareTo _compareMode;
        public string _propName;
        public string _compPropName = "";
        protected dynamic _compare;

        [SerializeField]
        private string _compareValueString;

        public Condition (string propName, ConditionType type, Comparison comparison, dynamic compare) {
            this._propName = propName;
            this._comparison = comparison;
            this._compareMode = CompareTo.CONSTANT;
            this._compPropName = "-constant-";
            this._valueType = type;
            if (compare.GetType() != FromConditionType(_valueType))
                throw new Exception("Wrong compare type given to Condition.Compare");
            SetComparisonValue(compare);
        }

        public Condition (string propName, string compPropName, ConditionType type, Comparison comparison) {
            this._propName = propName;
            this._comparison = comparison;
            this._compPropName = compPropName;
            this._compareMode = CompareTo.PROP;
            this._valueType = type;
        }

        public bool Compare (object compareTo) {
            dynamic typed_compareTo = GetTypedValue(compareTo);
            dynamic typed_compare = GetTypedValue(_compare);

            if (_valueType == ConditionType.FLOAT || _valueType == ConditionType.INT) {
                switch (_comparison) {
                    case Comparison.EQUAL:
                        return typed_compare == typed_compareTo;
                    case Comparison.NOT_EQUAL:
                        return typed_compare != typed_compareTo;
                    case Comparison.GREATER_THAN:
                        return typed_compareTo > typed_compare;
                    case Comparison.GREATER_OR_EQUAL:
                        return typed_compareTo >= typed_compare;
                    case Comparison.LESS_THAN:
                        return typed_compareTo < typed_compare;
                    case Comparison.LESS_OR_EQUAL:
                        return typed_compareTo <= typed_compare;
                }
                return false;
            } else if (_valueType == ConditionType.BOOL || _valueType == ConditionType.STRING) {
                if (_comparison == Comparison.EQUAL) {
                    return typed_compare == typed_compareTo;
                } else if (_comparison == Comparison.NOT_EQUAL) {
                    return typed_compare != typed_compareTo;
                } else {
                    throw new Exception ("Comparisons of type Bool or String can only use EQUAL or NOT_EQUAL for Conditions!");
                }
            } else {
                throw new Exception("Unrecognized Condition Comparison type!");
            }
        }

        public static Type FromConditionType (Condition.ConditionType valueType) {
            Type compareType = null;
            switch (valueType) {
                case ConditionType.FLOAT:
                    compareType = typeof(float);
                    break;
                case ConditionType.INT:
                    compareType = typeof(int);
                    break;
                case ConditionType.BOOL:
                    compareType = typeof(bool);
                    break;
                case ConditionType.STRING:
                    compareType = typeof(string);
                    break;
            }
            return compareType;
        }

        public static ConditionType? ParseConditionType (Type type) {
            if (type == typeof(float)) {
                return ConditionType.FLOAT;
            } else if (type == typeof(int)) {
                return ConditionType.INT;
            } else if (type == typeof(bool)) {
                return ConditionType.BOOL;
            } else if (type == typeof(string)) {
                return ConditionType.STRING;
            }
            return null;
        }

        public void SetConditionType (ConditionType type) {
            this._valueType = type;
            switch (_valueType) {
                case ConditionType.FLOAT:
                    SetComparisonValue(0f);
                    break;
                case ConditionType.INT:
                    SetComparisonValue(0);
                    break;
                case ConditionType.BOOL:
                    SetComparisonValue(false);
                    break;
                case ConditionType.STRING:
                    SetComparisonValue("");
                    break;
            }
        }

        dynamic GetTypedValue (object value) {
            Type compareType = FromConditionType(_valueType);
            dynamic typed_value = Convert.ChangeType(value, compareType);
            return typed_value;
        }

        public dynamic GetComparisonValue () {
            return GetTypedValue(_compare);
        }

        public void SetComparisonValue (dynamic value) {
            if (value.GetType() != FromConditionType(_valueType)) {
                throw new Exception("Wrong value type given to LinkCondition: " + value.GetType() + "; expected " + FromConditionType(_valueType));
            } else {
                this._compare = value;
            }
        }

        public void OnAfterDeserialize () {
            switch (_valueType) {
                case ConditionType.FLOAT:
                    _compare = float.Parse(_compareValueString);
                    break;
                case ConditionType.INT:
                    _compare = int.Parse(_compareValueString);
                    break;
                case ConditionType.BOOL:
                    _compare = bool.Parse(_compareValueString);
                    break;
                case ConditionType.STRING:
                    _compare = _compareValueString;
                    break;
            }
            if (_compare == null)
                throw new Exception ("Unable to deserialize Condition!");
        }

        public void OnBeforeSerialize () {
            this._compareValueString = _compare.ToString();
        }

        public static Condition.ConditionType ConditionTypeFromString (string s) {
            Condition.ConditionType parsed_enum = (Condition.ConditionType)System.Enum.Parse( typeof(Condition.ConditionType), s );
            return parsed_enum;
        }

        public static Condition.Comparison ComparisonFromString (string s) {
            Condition.Comparison parsed_enum = (Condition.Comparison)System.Enum.Parse( typeof(Condition.Comparison), s );
            return parsed_enum;
        }

        public static dynamic GetDefaultValue (ConditionType type) {
            dynamic defVal = null;
            if (type == Condition.ConditionType.FLOAT) {
                defVal = 0f;
            } else if (type == Condition.ConditionType.INT) {
                defVal = 0;
            } else if (type == Condition.ConditionType.BOOL) {
                defVal = false;
            } else if (type == Condition.ConditionType.STRING) {
                defVal = "";
            }
            return defVal;
        }

        public override string ToString() {
            string quote = _valueType == ConditionType.STRING ? "\"" : "";
            string compareToString = _compareMode == CompareTo.PROP ? _compPropName : (_compareMode == CompareTo.CONSTANT ? GetComparisonValue().ToString() : " previous output");
            return _valueType + " " + _propName + " " + _comparison + " " + quote + compareToString + quote;
        }

        public string ToPrettyString () {
            string quote = _valueType == ConditionType.STRING ? "\"" : "";
            string compareToString = _compareMode == CompareTo.PROP ? _compPropName : (_compareMode == CompareTo.CONSTANT ? GetComparisonValue().ToString() : " previous output");
            string comparitor = "";
            switch (_comparison) {
                case (Comparison.EQUAL):
                    comparitor = "==";
                    break;
                case (Comparison.NOT_EQUAL):
                    comparitor = "!=";
                    break;
                case (Comparison.GREATER_THAN):
                    comparitor = ">";
                    break;
                case (Comparison.GREATER_OR_EQUAL):
                    comparitor = ">=";
                    break;
                case (Comparison.LESS_THAN):
                    comparitor = "<";
                    break;
                case (Comparison.LESS_OR_EQUAL):
                    comparitor = "<=";
                    break;
            }
            return _propName + " " + comparitor + " " + quote + compareToString + quote;
        }
        
    }

    [Serializable]
    public class LinkConditionSerialized {
        public Condition.Comparison comparison;
        public string propName;
        public Condition.ConditionType compareType;
        public string compareValue;
        public LinkConditionSerialized(Condition.Comparison comparison, string propName, Condition.ConditionType compareType, string compareValue) {
            this.comparison = comparison;
            this.propName = propName;
            this.compareType = compareType;
            this.compareValue = compareValue;
        }
    }

}