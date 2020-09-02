using System;
using UnityEngine;

namespace NodeMachine {

    [Serializable]
    public class Condition : ISerializationCallbackReceiver {
        
        public enum Comparison {
            EQUAL, GREATER_THAN, GREATER_OR_EQUAL, LESS_THAN, LESS_OR_EQUAL
        }
        
        public enum ConditionType {
            FLOAT, INT, BOOL
        }

        public ConditionType _type;
        public Comparison _comparison;
        public string _propName;
        protected dynamic _compare;

        [SerializeField]
        private string _compareValueString;

        public Condition (string propName, ConditionType type, Comparison comparison, dynamic compare) {
            this._propName = propName;
            this._comparison = comparison;
            this._type = type;
            if (compare.GetType() != GetConditionType())
                throw new Exception("Wrong compare type given to Condition.Compare");
            SetComparisonValue(compare);
        }

        public bool Compare (object compareTo) {
            dynamic typed_compareTo = GetTypedValue(compareTo);
            dynamic typed_compare = GetTypedValue(_compare);

            if (_type == ConditionType.FLOAT || _type == ConditionType.INT) {
                switch (_comparison) {
                    case Comparison.EQUAL:
                        return typed_compare == compareTo;
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
            } else if (_type == ConditionType.BOOL) {
                if (_comparison == Comparison.EQUAL) {
                    return typed_compare == typed_compareTo;
                } else {
                    throw new Exception ("Comparisons of type Bool can only use EQUALS for LinkConditions!");
                }
            } else {
                throw new Exception("Unrecognized LinkCondition Comparison type!");
            }
        }

        public Type GetConditionType () {
            Type compareType = null;
            switch (_type) {
                case ConditionType.FLOAT:
                    compareType = typeof(float);
                    break;
                case ConditionType.INT:
                    compareType = typeof(int);
                    break;
                case ConditionType.BOOL:
                    compareType = typeof(bool);
                    break;
            }
            if (compareType == null) {
                throw new Exception ("Unrecognized LinkCondition object type!");
            }
            return compareType;
        }

        public void SetConditionType (ConditionType type) {
            this._type = type;
            switch (_type) {
                case ConditionType.FLOAT:
                    SetComparisonValue(0f);
                    break;
                case ConditionType.INT:
                    SetComparisonValue(0);
                    break;
                case ConditionType.BOOL:
                    SetComparisonValue(false);
                    break;
            }
        }

        dynamic GetTypedValue (object value) {
            Type compareType = GetConditionType();
            dynamic typed_value = Convert.ChangeType(value, compareType);
            return typed_value;
        }

        public dynamic GetComparisonValue () {
            return GetTypedValue(_compare);
        }

        public void SetComparisonValue (dynamic value) {
            if (value.GetType() != GetConditionType()) {
                throw new Exception("Wrong value type given to LinkCondition: " + value.GetType() + "; expected " + GetConditionType());
            } else {
                this._compare = value;
            }
        }

        public void OnAfterDeserialize () {
            switch (_type) {
                case ConditionType.FLOAT:
                    _compare = float.Parse(_compareValueString);
                    break;
                case ConditionType.INT:
                    _compare = int.Parse(_compareValueString);
                    break;
                case ConditionType.BOOL:
                    _compare = bool.Parse(_compareValueString);
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
            }
            return defVal;
        }

        public override string ToString() {
            return _type + " " + _propName + " " + _comparison + " " + GetComparisonValue();
        }

        public string ToPrettyString () {
            string comparitor = "";
            switch (_comparison) {
                case (Comparison.EQUAL):
                    comparitor = "==";
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
            return _propName + " " + comparitor + " " + GetComparisonValue();
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