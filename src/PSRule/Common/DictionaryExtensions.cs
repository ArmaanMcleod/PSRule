﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PSRule
{
    internal static class DictionaryExtensions
    {
        [DebuggerStepThrough]
        public static bool TryPopValue(this IDictionary<string, object> dictionary, string key, out object value)
        {
            return dictionary.TryGetValue(key, out value) && dictionary.Remove(key);
        }

        [DebuggerStepThrough]
        public static bool TryPopValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
        {
            value = default;
            if (dictionary.TryGetValue(key, out var v) && dictionary.Remove(key) && v is T result)
            {
                value = result;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryPopBool(this IDictionary<string, object> dictionary, string key, out bool value)
        {
            value = default;
            return TryPopValue(dictionary, key, out var v) && bool.TryParse(v.ToString(), out value);
        }

        [DebuggerStepThrough]
        public static bool TryPopEnum<TEnum>(this IDictionary<string, object> dictionary, string key, out TEnum value) where TEnum : struct
        {
            value = default;
            return TryPopValue(dictionary, key, out var v) && Enum.TryParse(v.ToString(), ignoreCase: true, result: out value);
        }

        [DebuggerStepThrough]
        public static bool TryPopString(this IDictionary<string, object> dictionary, string key, out string value)
        {
            value = default;
            if (TryPopValue(dictionary, key, out var v) && v is string svalue)
            {
                value = svalue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryPopStringArray(this IDictionary<string, object> dictionary, string key, out string[] value)
        {
            value = default;
            return TryPopValue(dictionary, key, out var v) && TryStringArray(v, out value);
        }

        [DebuggerStepThrough]
        public static bool TryGetBool(this IDictionary<string, object> dictionary, string key, out bool? value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out var o))
                return false;

            if (o is bool bvalue || (o is string svalue && bool.TryParse(svalue, out bvalue)))
            {
                value = bvalue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryGetLong(this IDictionary<string, object> dictionary, string key, out long? value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out var o))
                return false;

            if (o is long lvalue || (o is string svalue && long.TryParse(svalue, out lvalue)))
            {
                value = lvalue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryGetString(this IDictionary<string, object> dictionary, string key, out string value)
        {
            value = null;
            if (!dictionary.TryGetValue(key, out var o))
                return false;

            if (o is string svalue)
            {
                value = svalue;
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool TryGetStringArray(this IDictionary<string, object> dictionary, string key, out string[] value)
        {
            value = null;
            return dictionary.TryGetValue(key, out var o) && TryStringArray(o, out value);
        }

        [DebuggerStepThrough]
        public static void AddUnique(this IDictionary<string, object> dictionary, IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var kv in values)
                if (!dictionary.ContainsKey(kv.Key))
                    dictionary.Add(kv.Key, kv.Value);
        }

        [DebuggerStepThrough]
        private static bool TryStringArray(object o, out string[] value)
        {
            value = default;
            if (o == null)
                return false;

            value = o.GetType().IsArray ? ((object[])o).OfType<string>().ToArray() : new string[] { o.ToString() };
            return true;
        }

        internal static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return new SortedDictionary<TKey, TValue>(dictionary);
        }

        internal static bool NullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary == null || dictionary.Count == 0;
        }
    }
}
