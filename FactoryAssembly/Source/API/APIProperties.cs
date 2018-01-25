using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FactoryAssembly
{
    /// <remarks>
    /// Adapted from Lupo511's MultipleBombs' PropertiesBehaviour class: https://github.com/Lupo511/ktanemodkit/blob/MultipleBombs/ManagedAssembly/MultipleBombsAssembly/MultipleBombsAssembly/PropertiesBehaviour.cs
    /// </remarks>
    public class APIProperties : MonoBehaviour, IDictionary<string, object>
    {
        private class Property
        {
            public bool CanGet
            {
                get
                {
                    return _get != null;
                }
            }

            public bool CanSet
            {
                get
                {
                    return _set != null;
                }
            }

            public object Value
            {
                get
                {
                    if (CanGet)
                    {
                        return _get();
                    }
                    else
                    {
                        throw new Exception(string.Format("Cannot get value '{0}' - no get access.", _name));
                    }
                }
                set
                {
                    if (CanSet)
                    {
                        _set(value);
                    }
                    else
                    {
                        throw new Exception(string.Format("Cannot set value '{0}' - no set access.", _name));
                    }
                }
            }

            private readonly string _name = null;
            private readonly Func<object> _get = null;
            private readonly Action<object> _set = null;

            public Property(string name, Func<object> get, Action<object> set)
            {
                _name = name;
                _get = get;
                _set = set;
            }
        }

        public object this[string key]
        {
            get
            {
                return _properties[key].Value;
            }
            set
            {
                _properties[key].Value = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return _properties.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                throw new NotImplementedException("The Values property is not supported in this Dictionary.");
            }
        }

        public int Count
        {
            get
            {
                return _properties.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        private Dictionary<string, Property> _properties = new Dictionary<string, Property>();

        public void Add(string key, Func<object> get, Action<object> set)
        {
            _properties[key] = new Property(key, get, set);
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException("You can't add items to this Dictionary.");
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException("You can't add items to this Dictionary.");
        }

        public void Clear()
        {
            throw new NotImplementedException("You can't clear this Dictionary.");
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException("The Contains method is not supported in this Dictionary.");
        }

        public bool ContainsKey(string key)
        {
            return _properties.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException("The CopyTo method is not supported in this Dictionary.");
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException("The GetEnumerator method is not supported in this Dictionary.");
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException("The Remove method is not supported in this Dictionary.");
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException("The Remove method is not supported in this Dictionary.");
        }

        public bool TryGetValue(string key, out object value)
        {
            try
            {
                value = _properties[key].Value;
                return true;
            }
            catch(Exception)
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException("The GetEnumerator method is not supported in this Dictionary.");
        }
    }
}
