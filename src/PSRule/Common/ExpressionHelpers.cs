// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Pipeline;
using PSRule.Runtime;

namespace PSRule
{
    internal static class ExpressionHelpers
    {
        private const string CACHE_MATCH = "MatchRegex";
        private const string CACHE_MATCH_C = "MatchRegexCaseSensitive";

        private const char Backslash = '\\';
        private const char Slash = '/';

        internal static bool NullOrEmpty(object o)
        {
            if (o == null)
                return true;

            o = GetBaseObject(o);
            return (o is ICollection c && c.Count == 0) ||
                (TryString(o, out var s) && string.IsNullOrEmpty(s));
        }

        internal static bool Exists(IBindingContext bindingContext, object inputObject, string field, bool caseSensitive)
        {
            return ObjectHelper.GetPath(bindingContext, inputObject, field, caseSensitive, out object _);
        }

        internal static bool Equal(object expectedValue, object actualValue, bool caseSensitive, bool convertExpected = false, bool convertActual = false)
        {
            if (TryString(expectedValue, out var s1) && TryString(actualValue, convertActual, out var s2))
                return StringEqual(s1, s2, caseSensitive);

            if (TryBool(expectedValue, convertExpected, out var b1) && TryBool(actualValue, convertActual, out var b2))
                return b1 == b2;

            if (TryLong(expectedValue, convertExpected, out var l1) && TryLong(actualValue, convertActual, out var l2))
                return l1 == l2;

            if (TryInt(expectedValue, convertExpected, out var i1) && TryInt(actualValue, convertActual, out var i2))
                return i1 == i2;

            var expectedBase = GetBaseObject(expectedValue);
            var actualBase = GetBaseObject(actualValue);
            return expectedBase.Equals(actualBase) || expectedValue.Equals(actualValue);
        }

        internal static bool CompareNumeric(object actual, object expected, bool convert, out int compare, out object value)
        {
            if (TryInt(actual, convert, out var actualInt) && TryInt(expected, convert: true, value: out var expectedInt))
            {
                compare = Comparer<int>.Default.Compare(actualInt, expectedInt);
                value = actualInt;
                return true;
            }
            else if (TryLong(actual, convert, out var actualLong) && TryLong(expected, convert: true, value: out var expectedLong))
            {
                compare = Comparer<long>.Default.Compare(actualLong, expectedLong);
                value = actualLong;
                return true;
            }
            else if (TryFloat(actual, convert, out var actualFloat) && TryFloat(expected, convert: true, value: out var expectedFloat))
            {
                compare = Comparer<float>.Default.Compare(actualFloat, expectedFloat);
                value = actualFloat;
                return true;
            }
            else if (TryDateTime(actual, convert, out var actualDateTime) && TryDateTime(expected, convert: true, value: out var expectedDateTime))
            {
                compare = Comparer<DateTime>.Default.Compare(actualDateTime, expectedDateTime);
                value = actualDateTime;
                return true;
            }
            else if ((TryStringLength(actual, out actualInt) ||
                TryEnumerableLength(actual, out actualInt)) &&
                TryInt(expected, convert: true, value: out expectedInt))
            {
                compare = Comparer<int>.Default.Compare(actualInt, expectedInt);
                value = actualInt;
                return true;
            }
            compare = 0;
            value = 0;
            return false;
        }

        internal static bool TryString(object o, out string value)
        {
            o = GetBaseObject(o);
            if (o is string s)
            {
                value = s;
                return true;
            }
            else if (o is JToken token && token.Type == JTokenType.String)
            {
                value = token.Value<string>();
                return true;
            }
            value = null;
            return false;
        }

        internal static bool TryString(object o, bool convert, out string value)
        {
            if (TryString(o, out value))
                return true;

            if (convert && o is Enum evalue)
            {
                value = evalue.ToString();
                return true;
            }
            else if (convert && TryInt(o, false, out var ivalue))
            {
                value = ivalue.ToString(Thread.CurrentThread.CurrentCulture);
                return true;
            }
            return false;
        }

        internal static bool TryArray(object o, out Array value)
        {
            o = GetBaseObject(o);
            if (o is Array a)
            {
                value = a;
                return true;
            }
            value = null;
            return false;
        }

        internal static bool TryConvertStringArray(object[] o, out string[] value)
        {
            value = Array.Empty<string>();
            if (o == null || o.Length == 0 || !TryString(o[0], convert: true, value: out var s))
                return false;

            value = new string[o.Length];
            value[0] = s;
            for (var i = 1; i < o.Length; i++)
            {
                if (TryString(o[i], convert: true, value: out s))
                    value[i] = s;
            }
            return true;
        }

        /// <summary>
        /// Try to get an int from the existing object.
        /// </summary>
        internal static bool TryInt(object o, bool convert, out int value)
        {
            o = GetBaseObject(o);
            if (o is int ivalue)
            {
                value = ivalue;
                return true;
            }
            if (o is long lvalue && lvalue <= int.MaxValue && lvalue >= int.MinValue)
            {
                value = (int)lvalue;
                return true;
            }
            else if (o is JToken token && token.Type == JTokenType.Integer)
            {
                value = token.Value<int>();
                return true;
            }
            else if (convert && TryString(o, out var s) && int.TryParse(s, out ivalue))
            {
                value = ivalue;
                return true;
            }
            value = default;
            return false;
        }

        internal static bool TryBool(object o, bool convert, out bool value)
        {
            o = GetBaseObject(o);
            if (o is bool bvalue)
            {
                value = bvalue;
                return true;
            }
            else if (o is JToken token && token.Type == JTokenType.Boolean)
            {
                value = token.Value<bool>();
                return true;
            }
            else if (convert && TryString(o, out var s) && bool.TryParse(s, out bvalue))
            {
                value = bvalue;
                return true;
            }
            value = default;
            return false;
        }

        internal static bool TryByte(object o, bool convert, out byte value)
        {
            o = GetBaseObject(o);
            if (o is byte bvalue)
            {
                value = bvalue;
                return true;
            }
            else if (o is JToken token && token.Type == JTokenType.Integer)
            {
                value = token.Value<byte>();
                return true;
            }
            else if (convert && TryString(o, out var s) && byte.TryParse(s, out bvalue))
            {
                value = bvalue;
                return true;
            }
            value = default;
            return false;
        }

        internal static bool TryLong(object o, bool convert, out long value)
        {
            o = GetBaseObject(o);
            if (o is byte b)
            {
                value = b;
                return true;
            }
            else if (o is int i)
            {
                value = i;
                return true;
            }
            else if (o is long l)
            {
                value = l;
                return true;
            }
            else if (o is JToken token && token.Type == JTokenType.Integer)
            {
                value = token.Value<long>();
                return true;
            }
            else if (convert && TryString(o, out var s) && long.TryParse(s, out l))
            {
                value = l;
                return true;
            }
            value = default;
            return false;
        }

        internal static bool TryFloat(object o, bool convert, out float value)
        {
            o = GetBaseObject(o);
            if (o is float fvalue || (convert && o is string s && float.TryParse(s, out fvalue)))
            {
                value = fvalue;
                return true;
            }
            else if (convert && o is int ivalue)
            {
                value = ivalue;
                return true;
            }
            value = default;
            return false;
        }

        internal static bool TryDouble(object o, bool convert, out double value)
        {
            o = GetBaseObject(o);
            if (o is double dvalue || (convert && o is string s && double.TryParse(s, out dvalue)))
            {
                value = dvalue;
                return true;
            }
            value = default;
            return false;
        }

        internal static bool TryStringLength(object o, out int value)
        {
            o = GetBaseObject(o);
            if (o is string s)
            {
                value = s.Length;
                return true;
            }
            value = 0;
            return false;
        }

        internal static bool TryEnumerableLength(object o, out int value)
        {
            o = GetBaseObject(o);
            if (o is Array array)
            {
                value = array.Length;
                return true;
            }
            if (o is ICollection collection)
            {
                value = collection.Count;
                return true;
            }
            if (o is JArray jArray)
            {
                value = jArray.Count;
                return true;
            }
            if (o is IEnumerable enumerable)
            {
                value = enumerable.OfType<object>().Count();
                return true;
            }
            value = 0;
            return false;
        }

        internal static bool TryDateTime(object o, bool convert, out DateTime value)
        {
            o = GetBaseObject(o);
            if (o is DateTime dvalue)
            {
                value = dvalue;
                return true;
            }
            else if (o is JToken token && token.Type == JTokenType.Date)
            {
                value = token.Value<DateTime>();
                return true;
            }
            else if (convert && TryString(o, out var s) && DateTime.TryParse(s, out dvalue))
            {
                value = dvalue;
                return true;
            }
            else if (convert && TryInt(o, convert: false, out var daysOffset))
            {
                value = DateTime.Now.AddDays(daysOffset);
                return true;
            }
            value = default;
            return false;
        }

        internal static bool Match(string pattern, string value, bool caseSensitive)
        {
            var expression = GetRegularExpression(pattern, caseSensitive);
            return expression.IsMatch(value);
        }

        internal static bool Match(object pattern, object value, bool caseSensitive)
        {
            return TryString(pattern, out var patternString) && TryString(value, out var s) && Match(patternString, s, caseSensitive);
        }

        internal static bool AnyValue(object actualValue, object expectedValue, bool caseSensitive, out object foundValue)
        {
            foundValue = actualValue;
            var expectedBase = GetBaseObject(expectedValue);
            if (actualValue is IEnumerable items && !(actualValue is string))
            {
                foreach (var item in items)
                {
                    foundValue = item;
                    if (Equal(expectedBase, item, caseSensitive))
                        return true;
                }
            }
            if (Equal(expectedBase, actualValue, caseSensitive))
            {
                foundValue = actualValue;
                return true;
            }
            return false;
        }

        internal static bool CountValue(object actualValue, object expectedValue, bool caseSensitive, out int count)
        {
            count = 0;
            var expectedBase = GetBaseObject(expectedValue);
            var actualBase = GetBaseObject(actualValue);
            if (actualBase is IEnumerable items)
            {
                foreach (var item in items)
                {
                    if (Equal(expectedBase, item, caseSensitive))
                        count++;
                }
                return count > 0;
            }
            else if (Equal(expectedBase, actualValue, caseSensitive))
            {
                count = 1;
                return true;
            }
            return false;
        }

        internal static bool WithinPath(string actualPath, string expectedPath, bool caseSensitive)
        {
            var expected = PSRuleOption.GetRootedBasePath(expectedPath, normalize: true);
            var actual = PSRuleOption.GetRootedPath(actualPath, normalize: true);
            return actual.StartsWith(expected, ignoreCase: !caseSensitive, Thread.CurrentThread.CurrentCulture);
        }

        internal static string NormalizePath(string basePath, string path)
        {
            path = Path.IsPathRooted(path) ? Path.GetFullPath(path) : Path.GetFullPath(Path.Combine(basePath, path));
            basePath = PSRuleOption.GetRootedBasePath(basePath);
            return path.Substring(basePath.Length).Replace(Backslash, Slash);
        }

        internal static string GetObjectOriginPath(object o)
        {
            var baseObject = GetBaseObject(o);
            var targetInfo = GetTargetInfo(o);
            if (baseObject is InputFileInfo inputFileInfo)
            {
                return PSRuleOption.GetRootedPath(inputFileInfo.FullName, normalize: true);
            }
            else if (baseObject is FileInfo fileInfo)
            {
                return PSRuleOption.GetRootedPath(fileInfo.FullName, normalize: true);
            }
            else if (baseObject is TargetSourceInfo sourceInfo && !string.IsNullOrEmpty(sourceInfo.File))
            {
                return PSRuleOption.GetRootedPath(sourceInfo.File, normalize: true);
            }
            else if (targetInfo != null)
            {
                return PSRuleOption.GetRootedPath(targetInfo.Path, normalize: true);
            }
            else if (baseObject is string s)
            {
                return PSRuleOption.GetRootedPath(s, normalize: true);
            }
            return null;
        }

        private static string NormalizeSchemaUri(string value, bool ignoreScheme)
        {
            if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
                return value;

            var result = uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.ToString();
            if (ignoreScheme && result.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                result = result.Remove(0, 8);
            else if (ignoreScheme && result.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                result = result.Remove(0, 7);

            return uri.IsAbsoluteUri && uri.Fragment == "#" ? result.TrimEnd('#') : result;
        }

        internal static bool AnySchema(string actualValue, Array expectedValue, bool ignoreScheme, bool caseSensitive)
        {
            var actualNormal = NormalizeSchemaUri(actualValue, ignoreScheme);
            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            for (var i = 0; expectedValue != null && i < expectedValue.Length; i++)
            {
                if (expectedValue.GetValue(i) is string uri && comparer.Equals(actualNormal, NormalizeSchemaUri(uri, ignoreScheme)))
                    return true;
            }
            return false;
        }

        private static Regex GetRegularExpression(string pattern, bool caseSensitive)
        {
            if (!TryPipelineCache(caseSensitive ? CACHE_MATCH_C : CACHE_MATCH, pattern, out Regex expression))
            {
                var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                expression = new Regex(pattern, options);
                SetPipelineCache(CACHE_MATCH, pattern, expression);
            }
            return expression;
        }

        /// <summary>
        /// Try to retrieve the cached key from the pipeline cache.
        /// </summary>
        private static bool TryPipelineCache<T>(string prefix, string key, out T value)
        {
            value = default;
            if (PipelineContext.CurrentThread.ExpressionCache.TryGetValue(string.Concat(prefix, key), out var ovalue))
            {
                value = (T)ovalue;
                return true;
            }
            return false;
        }

        private static void SetPipelineCache<T>(string prefix, string key, T value)
        {
            PipelineContext.CurrentThread.ExpressionCache[string.Concat(prefix, key)] = value;
        }

        /// <summary>
        /// Get the base object.
        /// </summary>
        internal static object GetBaseObject(object o)
        {
            return o is PSObject pso && pso.BaseObject != null && !(pso.BaseObject is PSCustomObject) ? pso.BaseObject : o;
        }

        private static PSRuleTargetInfo GetTargetInfo(object o)
        {
            return o is PSObject pso && pso.TryTargetInfo(out var targetInfo) ? targetInfo : null;
        }

        private static bool StringEqual(string expectedValue, string actualValue, bool caseSensitive)
        {
            return caseSensitive
                ? StringComparer.Ordinal.Equals(expectedValue, actualValue)
                : StringComparer.OrdinalIgnoreCase.Equals(expectedValue, actualValue);
        }

        internal static bool StartsWith(string actualValue, object expectedValue, bool caseSensitive)
        {
            return TryString(expectedValue, out var expected) &&
                actualValue.StartsWith(expected, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EndsWith(string actualValue, object expectedValue, bool caseSensitive)
        {
            return TryString(expectedValue, out var expected)
                && actualValue.EndsWith(expected, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        internal static bool Contains(string actualValue, object expectedValue, bool caseSensitive)
        {
            return TryString(expectedValue, out var expected)
                && actualValue.IndexOf(expected, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static bool IsLower(string actualValue, bool requireLetters, out bool notLetter)
        {
            notLetter = false;
            for (var i = 0; i < actualValue.Length; i++)
            {
                if (!char.IsLetter(actualValue, i) && requireLetters)
                {
                    notLetter = true;
                    return false;
                }
                if (char.IsLetter(actualValue, i) && !char.IsLower(actualValue, i))
                    return false;
            }
            return true;
        }

        internal static bool IsUpper(string actualValue, bool requireLetters, out bool notLetter)
        {
            notLetter = false;
            for (var i = 0; i < actualValue.Length; i++)
            {
                if (!char.IsLetter(actualValue, i) && requireLetters)
                {
                    notLetter = true;
                    return false;
                }
                if (char.IsLetter(actualValue, i) && !char.IsUpper(actualValue, i))
                    return false;
            }
            return true;
        }

        internal static bool Like(string actualValue, string pattern, bool caseSensitive)
        {
            var options = caseSensitive ? WildcardOptions.CultureInvariant : WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase;
            var p = WildcardPattern.Get(pattern, options);
            return p.IsMatch(actualValue);
        }
    }
}
