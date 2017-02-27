//-----------------------------------------------------------------------
// <copyright file="JsonSerializer.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Serializes objects to the JavaScript Object Notation (JSON) and deserializes JSON data to objects.
    /// </summary>
    internal static class JsonSerializer
    {
        private const int BUILDER_CAPACITY = 2000;
        private const int TOKEN_COLON = 5;
        private const int TOKEN_COMMA = 6;
        private const int TOKEN_CURLY_CLOSE = 2;
        private const int TOKEN_CURLY_OPEN = 1;
        private const int TOKEN_FALSE = 10;
        private const int TOKEN_NONE = 0;
        private const int TOKEN_NULL = 11;
        private const int TOKEN_NUMBER = 8;
        private const int TOKEN_SQUARED_CLOSE = 4;
        private const int TOKEN_SQUARED_OPEN = 3;
        private const int TOKEN_STRING = 7;
        private const int TOKEN_TRUE = 9;
        private static readonly char[] EscapeCharacters = new char[] { '"', '\\', '\b', '\f', '\n', '\r', '\t' };
        private static readonly string EscapeCharactersString = new string(EscapeCharacters);
        private static readonly char[] EscapeTable;
        private static PocoJsonSerializerStrategy CurrentJsonSerializerStrategy = new PocoJsonSerializerStrategy();

        /// <summary>
        /// Initializes static members of the <see cref="JsonSerializer"/> class.
        /// </summary>
        static JsonSerializer()
        {
            EscapeTable = new char[93];
            EscapeTable['"'] = '"';
            EscapeTable['\\'] = '\\';
            EscapeTable['\b'] = 'b';
            EscapeTable['\f'] = 'f';
            EscapeTable['\n'] = 'n';
            EscapeTable['\r'] = 'r';
            EscapeTable['\t'] = 't';
        }

        /// <summary>
        /// Deserializes the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>The System.Object.</returns>
        public static object Deserialize(string json)
        {
            object obj;

            if (TryDeserialize(json, out obj))
            {
                return obj;
            }

            throw new SerializationException("Invalid JSON string");
        }

        /// <summary>
        /// Deserializes the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="type">The type.</param>
        /// <returns>The System.Object.</returns>
        public static object Deserialize(string json, Type type)
        {
            object jsonObject = Deserialize(json);
            return type == null || (jsonObject != null && ReflectionUtilities.IsAssignableFrom(jsonObject.GetType(), type))
                       ? jsonObject
                       : CurrentJsonSerializerStrategy.DeserializeObject(jsonObject, type);
        }

        /// <summary>
        /// Deserializes the specified json.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns>The System.Object.</returns>
        public static T Deserialize<T>(string json)
        {
            return (T)Deserialize(json, typeof(T));
        }

        /// <summary>
        /// Escapes to javascript string.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>The System.String.</returns>
        public static string EscapeToJavascriptString(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return jsonString;
            }

            StringBuilder stringBuilder = new StringBuilder();
            char c;

            for (int i = 0; i < jsonString.Length; )
            {
                c = jsonString[i++];

                if (c == '\\')
                {
                    int remainingLength = jsonString.Length - i;

                    if (remainingLength >= 2)
                    {
                        char lookahead = jsonString[i];

                        if (lookahead == '\\')
                        {
                            stringBuilder.Append('\\');
                            ++i;
                        }
                        else if (lookahead == '"')
                        {
                            stringBuilder.Append("\"");
                            ++i;
                        }
                        else if (lookahead == 't')
                        {
                            stringBuilder.Append('\t');
                            ++i;
                        }
                        else if (lookahead == 'b')
                        {
                            stringBuilder.Append('\b');
                            ++i;
                        }
                        else if (lookahead == 'n')
                        {
                            stringBuilder.Append('\n');
                            ++i;
                        }
                        else if (lookahead == 'r')
                        {
                            stringBuilder.Append('\r');
                            ++i;
                        }
                    }
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable.</returns>
        public static string Serialize(object json)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            bool success = SerializeValue(CurrentJsonSerializerStrategy, json, builder);
            return success ? builder.ToString() : null;
        }

        /// <summary>
        /// Try to deserialize the specified json.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="obj">The object.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public static bool TryDeserialize(string json, out object obj)
        {
            bool success = true;

            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                obj = ParseValue(charArray, ref index, ref success);
            }
            else
            {
                obj = null;
            }

            return success;
        }

        /// <summary>
        /// Converts from utf32.
        /// </summary>
        /// <param name="utf32">The utf32.</param>
        /// <returns>The System.String.</returns>
        private static string ConvertFromUtf32(int utf32)
        {
            if (utf32 < 0 || utf32 > 0x10FFFF)
            {
                throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
            }

            if (utf32 >= 0xD800 && utf32 <= 0xDFFF)
            {
                throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
            }

            if (utf32 < 0x10000)
            {
                return new string((char)utf32, 1);
            }

            utf32 -= 0x10000;
            return new string(new char[] { (char)((utf32 >> 10) + 0xD800), (char)((utf32 % 0x0400) + 0xDC00) });
        }

        /// <summary>
        /// Gets the last index of number.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <returns>The System.Int32.</returns>
        private static int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }

            return lastIndex - 1;
        }

        /// <summary>
        /// Determines if a given object is numeric in any way (can be integer, double, null, etc).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private static bool IsNumeric(object value)
        {
            if (value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is float
                || value is double
                || value is decimal)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Looks the ahead.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <returns>The System.Int32.</returns>
        private static int LookAhead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        /// <summary>
        /// Next token.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <returns>The System.Int32.</returns>
        private static int NextToken(char[] json, ref int index)
        {
            RemoveWhitespace(json, ref index);

            if (index == json.Length)
            {
                return TOKEN_NONE;
            }

            char c = json[index];
            index++;

            switch (c)
            {
                case '{':
                    return TOKEN_CURLY_OPEN;

                case '}':
                    return TOKEN_CURLY_CLOSE;

                case '[':
                    return TOKEN_SQUARED_OPEN;

                case ']':
                    return TOKEN_SQUARED_CLOSE;

                case ',':
                    return TOKEN_COMMA;

                case '"':
                    return TOKEN_STRING;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return TOKEN_NUMBER;

                case ':':
                    return TOKEN_COLON;
            }

            index--;
            int remainingLength = json.Length - index;

            if (remainingLength >= 5)
            {
                if (json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
                {
                    index += 5;
                    return TOKEN_FALSE;
                }
            }

            if (remainingLength >= 4)
            {
                if (json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
                {
                    index += 4;
                    return TOKEN_TRUE;
                }
            }

            if (remainingLength >= 4)
            {
                if (json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
                {
                    index += 4;
                    return TOKEN_NULL;
                }
            }

            return TOKEN_NONE;
        }

        /// <summary>
        /// Parses the array.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <param name="success">true if succeeded; otherwise, false.</param>
        /// <returns>The JsonArray.</returns>
        private static JsonArray ParseArray(char[] json, ref int index, ref bool success)
        {
            JsonArray array = new JsonArray();

            NextToken(json, ref index);

            bool done = false;

            while (!done)
            {
                int token = LookAhead(json, index);

                if (token == TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    object value = ParseValue(json, ref index, ref success);

                    if (!success)
                    {
                        return null;
                    }

                    array.Add(value);
                }
            }

            return array;
        }

        /// <summary>
        /// Parses the number.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <param name="success">true if succeeded; otherwise, false.</param>
        /// <returns>The System.Object.</returns>
        private static object ParseNumber(char[] json, ref int index, ref bool success)
        {
            RemoveWhitespace(json, ref index);
            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;
            object returnNumber;
            string newString = new string(json, index, charLength);

            if (newString.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || newString.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
            {
                double number;
                success = double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);
                returnNumber = number;
            }
            else
            {
                long number;
                success = long.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);
                returnNumber = number;
            }

            index = lastIndex + 1;
            return returnNumber;
        }

        /// <summary>
        /// Parses the object.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <param name="success">true if succeeded; otherwise, false.</param>
        /// <returns>The IDictionary.</returns>
        private static IDictionary<string, object> ParseObject(char[] json, ref int index, ref bool success)
        {
            IDictionary<string, object> table = new JsonObject(StringComparer.OrdinalIgnoreCase);

            int token;

            NextToken(json, ref index);

            bool done = false;

            while (!done)
            {
                token = LookAhead(json, index);

                if (token == TOKEN_NONE)
                {
                    success = false;
                    return null;
                }
                else if (token == TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {
                    string name = ParseString(json, ref index, ref success);

                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    token = NextToken(json, ref index);

                    if (token != TOKEN_COLON)
                    {
                        success = false;
                        return null;
                    }

                    object value = ParseValue(json, ref index, ref success);

                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        /// <summary>
        /// Parses the string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <param name="success">true if succeeded; otherwise, false.</param>
        /// <returns>The System.String.</returns>
        private static string ParseString(char[] json, ref int index, ref bool success)
        {
            StringBuilder stringBuilder = new StringBuilder(BUILDER_CAPACITY);
            char c;

            RemoveWhitespace(json, ref index);

            c = json[index++];
            bool complete = false;

            while (!complete)
            {
                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];

                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {
                    if (index == json.Length)
                    {
                        break;
                    }

                    c = json[index++];

                    if (c == '"')
                    {
                        stringBuilder.Append('"');
                    }
                    else if (c == '\\')
                    {
                        stringBuilder.Append('\\');
                    }
                    else if (c == '/')
                    {
                        stringBuilder.Append('/');
                    }
                    else if (c == 'b')
                    {
                        stringBuilder.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        stringBuilder.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        stringBuilder.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        stringBuilder.Append('\r');
                    }
                    else if (c == 't')
                    {
                        stringBuilder.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;

                        if (remainingLength >= 4)
                        {
                            uint codePoint;

                            if (!(success = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                            {
                                return string.Empty;
                            }

                            if (codePoint >= 0xD800 && codePoint <= 0xDBFF)
                            {
                                index += 4;
                                remainingLength = json.Length - index;

                                if (remainingLength >= 6)
                                {
                                    uint lowCodePoint;

                                    if (new string(json, index, 2) == "\\u" && uint.TryParse(new string(json, index + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out lowCodePoint))
                                    {
                                        if (lowCodePoint >= 0xDC00 && lowCodePoint <= 0xDFFF)
                                        {
                                            stringBuilder.Append((char)codePoint);
                                            stringBuilder.Append((char)lowCodePoint);
                                            index += 6;
                                            continue;
                                        }
                                    }
                                }

                                success = false;
                                return string.Empty;
                            }

                            stringBuilder.Append(ConvertFromUtf32((int)codePoint));
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Parses the value.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        /// <param name="success">true if succeeded; otherwise, false.</param>
        /// <returns>The System.Object.</returns>
        private static object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case TOKEN_STRING:
                    return ParseString(json, ref index, ref success);

                case TOKEN_NUMBER:
                    return ParseNumber(json, ref index, ref success);

                case TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index, ref success);

                case TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index, ref success);

                case TOKEN_TRUE:
                    NextToken(json, ref index);
                    return true;

                case TOKEN_FALSE:
                    NextToken(json, ref index);
                    return false;

                case TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;

                case TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        /// <summary>
        /// Removes the whitespace.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="index">The index.</param>
        private static void RemoveWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r\b\f".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Serializes the array.
        /// </summary>
        /// <param name="jsonSerializerStrategy">The json serializer strategy.</param>
        /// <param name="sourceArray">An array.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private static bool SerializeArray(PocoJsonSerializerStrategy jsonSerializerStrategy, IEnumerable sourceArray, StringBuilder builder)
        {
            builder.Append("[");
            bool first = true;

            foreach (object value in sourceArray)
            {
                if (!first)
                {
                    builder.Append(",");
                }

                if (!SerializeValue(jsonSerializerStrategy, value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("]");
            return true;
        }

        /// <summary>
        /// Serializes the number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private static bool SerializeNumber(object number, StringBuilder builder)
        {
            if (number is long)
            {
                builder.Append(((long)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is ulong)
            {
                builder.Append(((ulong)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is int)
            {
                builder.Append(((int)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is uint)
            {
                builder.Append(((uint)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is decimal)
            {
                builder.Append(((decimal)number).ToString(CultureInfo.InvariantCulture));
            }
            else if (number is float)
            {
                builder.Append(((float)number).ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
            }

            return true;
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="jsonSerializerStrategy">The json serializer strategy.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="values">The values.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private static bool SerializeObject(PocoJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder)
        {
            builder.Append("{");
            IEnumerator ke = keys.GetEnumerator();
            IEnumerator ve = values.GetEnumerator();
            bool first = true;

            while (ke.MoveNext() && ve.MoveNext())
            {
                object key = ke.Current;
                object value = ve.Current;

                if (!first)
                {
                    builder.Append(",");
                }

                string stringKey = key as string;

                if (stringKey != null)
                {
                    SerializeString(stringKey, builder);
                }
                else if (!SerializeValue(jsonSerializerStrategy, value, builder))
                {
                    return false;
                }

                builder.Append(":");

                if (!SerializeValue(jsonSerializerStrategy, value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("}");
            return true;
        }

        /// <summary>
        /// Serializes the string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private static bool SerializeString(string source, StringBuilder builder)
        {
            if (source.IndexOfAny(EscapeCharacters) == -1)
            {
                builder.Append('"');
                builder.Append(source);
                builder.Append('"');

                return true;
            }

            builder.Append('"');
            int safeCharacterCount = 0;
            char[] charArray = source.ToCharArray();

            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];

                if (c >= EscapeTable.Length || EscapeTable[c] == default(char))
                {
                    safeCharacterCount++;
                }
                else
                {
                    if (safeCharacterCount > 0)
                    {
                        builder.Append(charArray, i - safeCharacterCount, safeCharacterCount);
                        safeCharacterCount = 0;
                    }

                    builder.Append('\\');
                    builder.Append(EscapeTable[c]);
                }
            }

            if (safeCharacterCount > 0)
            {
                builder.Append(charArray, charArray.Length - safeCharacterCount, safeCharacterCount);
            }

            builder.Append('"');
            return true;
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="jsonSerializerStrategy">The json serializer strategy.</param>
        /// <param name="value">The value.</param>
        /// <param name="builder">The builder.</param>
        /// <returns>true if succeeded; otherwise, false.</returns>
        private static bool SerializeValue(PocoJsonSerializerStrategy jsonSerializerStrategy, object value, StringBuilder builder)
        {
            bool success = true;
            string stringValue = value as string;

            if (stringValue != null)
            {
                success = SerializeString(stringValue, builder);
            }
            else
            {
                IDictionary<string, object> dictionary = value as IDictionary<string, object>;

                if (dictionary != null)
                {
                    success = SerializeObject(jsonSerializerStrategy, dictionary.Keys, dictionary.Values, builder);
                }
                else
                {
                    IDictionary<string, string> stringDictionary = value as IDictionary<string, string>;

                    if (stringDictionary != null)
                    {
                        success = SerializeObject(jsonSerializerStrategy, stringDictionary.Keys, stringDictionary.Values, builder);
                    }
                    else
                    {
                        IEnumerable enumerableValue = value as IEnumerable;

                        if (enumerableValue != null)
                        {
                            success = SerializeArray(jsonSerializerStrategy, enumerableValue, builder);
                        }
                        else if (IsNumeric(value))
                        {
                            success = SerializeNumber(value, builder);
                        }
                        else if (value is bool)
                        {
                            builder.Append((bool)value ? "true" : "false");
                        }
                        else if (value == null)
                        {
                            builder.Append("null");
                        }
                        else
                        {
                            object serializedObject;
                            success = jsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out serializedObject);

                            if (success)
                            {
                                SerializeValue(jsonSerializerStrategy, serializedObject, builder);
                            }
                        }
                    }
                }
            }

            return success;
        }
    }

    /// <summary>
    /// Represents the JSON array.
    /// </summary>
    internal class JsonArray : List<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonArray"/> class.
        /// </summary>
        public JsonArray()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonArray"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the json array.</param>
        public JsonArray(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this) ?? string.Empty;
        }
    }

    /// <summary>
    /// Represents the JSON object.
    /// </summary>
    internal class JsonObject : IDictionary<string, object>
    {
        /// <summary>
        /// The internal member dictionary.
        /// </summary>
        private readonly Dictionary<string, object> _members;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonObject"/> class.
        /// </summary>
        public JsonObject()
        {
            this._members = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonObject"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public JsonObject(IEqualityComparer<string> comparer)
        {
            this._members = new Dictionary<string, object>(comparer);
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return this._members.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<string> Keys
        {
            get
            {
                return this._members.Keys;
            }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<object> Values
        {
            get
            {
                return this._members.Values;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The object.</returns>
        public object this[int index]
        {
            get
            {
                return GetAtIndex(this._members, index);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object" /> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The object.</returns>
        public object this[string key]
        {
            get
            {
                return this._members[key];
            }

            set
            {
                this._members[key] = value;
            }
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object value)
        {
            this._members.Add(key, value);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<string, object> item)
        {
            this._members.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this._members.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if contains the specified item; otherwise, false.</returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return this._members.ContainsKey(item.Key) && this._members[item.Key] == item.Value;
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string key)
        {
            return this._members.ContainsKey(key);
        }

        /// <summary>
        /// Copies KeyValuePair to array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            int num = this.Count;

            foreach (KeyValuePair<string, object> item in this)
            {
                array[arrayIndex++] = item;

                if (--num <= 0)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this._members.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._members.GetEnumerator();
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public bool Remove(string key)
        {
            return this._members.Remove(key);
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return this._members.Remove(item.Key);
        }

        /// <summary>
        /// Returns a json <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>A json <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(string key, out object value)
        {
            return this._members.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets at index.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="index">The index.</param>
        /// <returns>The object.</returns>
        internal static object GetAtIndex(IDictionary<string, object> dictionary, int index)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (index >= dictionary.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            int i = 0;

            foreach (KeyValuePair<string, object> item in dictionary)
            {
                if (i++ == index)
                {
                    return item.Value;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Class PocoJsonSerializerStrategy.
    /// </summary>
    internal class PocoJsonSerializerStrategy
    {
        /// <summary>
        /// Field ArrayConstructorParameterTypes.
        /// </summary>
        internal static readonly Type[] ArrayConstructorParameterTypes = new Type[] { typeof(int) };

        /// <summary>
        /// Field EmptyTypes.
        /// </summary>
        internal static readonly Type[] EmptyTypes = new Type[0];

        /// <summary>
        /// Field Iso8601Format.
        /// </summary>
        private static readonly string[] Iso8601Format = new string[]
        {
            @"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z",
            @"yyyy-MM-dd\THH:mm:ss\Z",
            @"yyyy-MM-dd\THH:mm:ssK"
        };

        /// <summary>
        /// Field ConstructorCache.
        /// </summary>
        private IDictionary<Type, ReflectionUtilities.ConstructorDelegate> ConstructorCache;

        /// <summary>
        /// Field GetCache.
        /// </summary>
        private IDictionary<Type, IDictionary<string, ReflectionUtilities.GetDelegate>> GetCache;

        /// <summary>
        /// Field SetCache.
        /// </summary>
        private IDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtilities.SetDelegate>>> SetCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocoJsonSerializerStrategy" /> class.
        /// </summary>
        public PocoJsonSerializerStrategy()
        {
            this.ConstructorCache = new ReflectionUtilities.ThreadSafeDictionary<Type, ReflectionUtilities.ConstructorDelegate>(this.ConstructorDelegateFactory);
            this.GetCache = new ReflectionUtilities.ThreadSafeDictionary<Type, IDictionary<string, ReflectionUtilities.GetDelegate>>(this.GetterValueFactory);
            this.SetCache = new ReflectionUtilities.ThreadSafeDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtilities.SetDelegate>>>(this.SetterValueFactory);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns>The System.Object.</returns>
        public virtual object DeserializeObject(object value, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            string valueAsString = value as string;

            if (type == typeof(Guid) && string.IsNullOrEmpty(valueAsString))
            {
                return default(Guid);
            }

            if (value == null)
            {
                return null;
            }

            object obj = null;

            if (valueAsString != null)
            {
                if (valueAsString.Length != 0)
                {
                    if (type == typeof(DateTime) || (ReflectionUtilities.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(DateTime)))
                    {
                        return DateTime.ParseExact(valueAsString, Iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                    }

                    if (type == typeof(DateTimeOffset) || (ReflectionUtilities.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(DateTimeOffset)))
                    {
                        return DateTimeOffset.ParseExact(valueAsString, Iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                    }

                    if (type == typeof(Guid) || (ReflectionUtilities.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid)))
                    {
                        return new Guid(valueAsString);
                    }

                    if (type == typeof(Uri))
                    {
                        bool isValid = Uri.IsWellFormedUriString(valueAsString, UriKind.RelativeOrAbsolute);

                        Uri result;

                        if (isValid && Uri.TryCreate(valueAsString, UriKind.RelativeOrAbsolute, out result))
                        {
                            return result;
                        }

                        return null;
                    }

                    if (type == typeof(string))
                    {
                        return valueAsString;
                    }

                    return Convert.ChangeType(valueAsString, type, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (type == typeof(Guid))
                    {
                        obj = default(Guid);
                    }
                    else if (ReflectionUtilities.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid))
                    {
                        obj = null;
                    }
                    else
                    {
                        obj = valueAsString;
                    }
                }

                if (!ReflectionUtilities.IsNullableType(type) && Nullable.GetUnderlyingType(type) == typeof(Guid))
                {
                    return valueAsString;
                }
            }
            else if (value is bool)
            {
                return value;
            }

            bool isValueLong = value is long;
            bool isValueDouble = value is double;

            if ((isValueLong && type == typeof(long)) || (isValueDouble && type == typeof(double)))
            {
                return value;
            }

            if ((isValueDouble && type != typeof(double)) || (isValueLong && type != typeof(long)))
            {
                obj = type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(bool) || type == typeof(decimal) || type == typeof(byte) || type == typeof(short)
                            ? Convert.ChangeType(value, type, CultureInfo.InvariantCulture)
                            : value;
            }
            else
            {
                IDictionary<string, object> objects = value as IDictionary<string, object>;

                if (objects != null)
                {
                    IDictionary<string, object> jsonObject = objects;

                    if (ReflectionUtilities.IsTypeDictionary(type))
                    {
                        Type[] types = ReflectionUtilities.GetGenericTypeArguments(type);
                        Type keyType = types[0];
                        Type valueType = types[1];
                        Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                        IDictionary dictionary = (IDictionary)this.ConstructorCache[genericType]();

                        foreach (KeyValuePair<string, object> item in jsonObject)
                        {
                            dictionary.Add(item.Key, this.DeserializeObject(item.Value, valueType));
                        }

                        obj = dictionary;
                    }
                    else
                    {
                        if (type == typeof(object))
                        {
                            obj = value;
                        }
                        else
                        {
                            obj = this.ConstructorCache[type]();

                            foreach (KeyValuePair<string, KeyValuePair<Type, ReflectionUtilities.SetDelegate>> setter in this.SetCache[type])
                            {
                                object jsonValue;

                                if (jsonObject.TryGetValue(setter.Key, out jsonValue))
                                {
                                    jsonValue = this.DeserializeObject(jsonValue, setter.Value.Key);
                                    setter.Value.Value(obj, jsonValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    IList<object> valueAsList = value as IList<object>;

                    if (valueAsList != null)
                    {
                        IList<object> jsonObject = valueAsList;
                        IList list = null;

                        if (type.IsArray)
                        {
                            list = (IList)this.ConstructorCache[type](jsonObject.Count);
                            int i = 0;

                            foreach (object o in jsonObject)
                            {
                                list[i++] = this.DeserializeObject(o, type.GetElementType());
                            }
                        }
                        else if (ReflectionUtilities.IsTypeGenericCollectionInterface(type) || ReflectionUtilities.IsAssignableFrom(typeof(IList), type))
                        {
                            Type innerType = ReflectionUtilities.GetGenericListElementType(type);
                            list = (IList)(this.ConstructorCache[type] ?? this.ConstructorCache[typeof(List<>).MakeGenericType(innerType)])(jsonObject.Count);

                            foreach (object o in jsonObject)
                            {
                                list.Add(this.DeserializeObject(o, innerType));
                            }
                        }

                        obj = list;
                    }
                }

                return obj;
            }

            if (ReflectionUtilities.IsNullableType(type))
            {
                return ReflectionUtilities.ToNullableType(obj, type);
            }

            return obj;
        }

        /// <summary>
        /// Try to serialize non primitive object.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns>true if serialize succeeded; otherwise, false.</returns>
        public virtual bool TrySerializeNonPrimitiveObject(object input, out object output)
        {
            return this.TrySerializeKnownTypes(input, out output) || this.TrySerializeUnknownTypes(input, out output);
        }

        /// <summary>
        /// Constructor delegate.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The delegate.</returns>
        internal virtual ReflectionUtilities.ConstructorDelegate ConstructorDelegateFactory(Type key)
        {
            return ReflectionUtilities.GetConstructor(key, key.IsArray ? ArrayConstructorParameterTypes : EmptyTypes);
        }

        /// <summary>
        /// Getters the value factory.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The delegate.</returns>
        internal virtual IDictionary<string, ReflectionUtilities.GetDelegate> GetterValueFactory(Type type)
        {
            IDictionary<string, ReflectionUtilities.GetDelegate> result = new Dictionary<string, ReflectionUtilities.GetDelegate>();

            foreach (PropertyInfo propertyInfo in ReflectionUtilities.GetProperties(type))
            {
                if (propertyInfo.CanRead)
                {
                    MethodInfo getMethod = ReflectionUtilities.GetGetterMethodInfo(propertyInfo);

                    if (getMethod.IsStatic || !getMethod.IsPublic)
                    {
                        continue;
                    }

                    result[this.MapClrMemberNameToJsonFieldName(propertyInfo.Name)] = ReflectionUtilities.GetGetMethod(propertyInfo);
                }
            }

            foreach (FieldInfo fieldInfo in ReflectionUtilities.GetFields(type))
            {
                if (fieldInfo.IsStatic || !fieldInfo.IsPublic)
                {
                    continue;
                }

                result[this.MapClrMemberNameToJsonFieldName(fieldInfo.Name)] = ReflectionUtilities.GetGetMethod(fieldInfo);
            }

            return result;
        }

        /// <summary>
        /// Setters the value factory.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The delegate.</returns>
        internal virtual IDictionary<string, KeyValuePair<Type, ReflectionUtilities.SetDelegate>> SetterValueFactory(Type type)
        {
            IDictionary<string, KeyValuePair<Type, ReflectionUtilities.SetDelegate>> result = new Dictionary<string, KeyValuePair<Type, ReflectionUtilities.SetDelegate>>();

            foreach (PropertyInfo propertyInfo in ReflectionUtilities.GetProperties(type))
            {
                if (propertyInfo.CanWrite)
                {
                    MethodInfo setMethod = ReflectionUtilities.GetSetterMethodInfo(propertyInfo);

                    if (setMethod.IsStatic || !setMethod.IsPublic)
                    {
                        continue;
                    }

                    result[this.MapClrMemberNameToJsonFieldName(propertyInfo.Name)] = new KeyValuePair<Type, ReflectionUtilities.SetDelegate>(propertyInfo.PropertyType, ReflectionUtilities.GetSetMethod(propertyInfo));
                }
            }

            foreach (FieldInfo fieldInfo in ReflectionUtilities.GetFields(type))
            {
                if (fieldInfo.IsInitOnly || fieldInfo.IsStatic || !fieldInfo.IsPublic)
                {
                    continue;
                }

                result[this.MapClrMemberNameToJsonFieldName(fieldInfo.Name)] = new KeyValuePair<Type, ReflectionUtilities.SetDelegate>(fieldInfo.FieldType, ReflectionUtilities.GetSetMethod(fieldInfo));
            }

            return result;
        }

        /// <summary>
        /// Maps the name of the color member name to json field.
        /// </summary>
        /// <param name="clrPropertyName">The name of the property.</param>
        /// <returns>The System.String.</returns>
        protected virtual string MapClrMemberNameToJsonFieldName(string clrPropertyName)
        {
            return clrPropertyName;
        }

        /// <summary>
        /// Serializes the enum.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The System.Object.</returns>
        protected virtual object SerializeEnum(Enum enumValue)
        {
            return Convert.ToDouble(enumValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tries the serialize known types.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns>true if serialize succeeded; otherwise, false.</returns>
        protected virtual bool TrySerializeKnownTypes(object input, out object output)
        {
            bool returnValue = true;

            if (input is DateTime)
            {
                output = ((DateTime)input).ToUniversalTime().ToString(Iso8601Format[0], CultureInfo.InvariantCulture);
            }
            else if (input is DateTimeOffset)
            {
                output = ((DateTimeOffset)input).ToUniversalTime().ToString(Iso8601Format[0], CultureInfo.InvariantCulture);
            }
            else if (input is Guid)
            {
                output = ((Guid)input).ToString("D");
            }
            else if (input is Uri)
            {
                output = input.ToString();
            }
            else
            {
                Enum inputEnum = input as Enum;

                if (inputEnum != null)
                {
                    output = this.SerializeEnum(inputEnum);
                }
                else
                {
                    returnValue = false;
                    output = null;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Tries the serialize unknown types.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns>true if serialize succeeded; otherwise, false.</returns>
        protected virtual bool TrySerializeUnknownTypes(object input, out object output)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            output = null;

            Type type = input.GetType();

            if (type.FullName == null)
            {
                return false;
            }

            IDictionary<string, object> obj = new JsonObject(StringComparer.OrdinalIgnoreCase);
            IDictionary<string, ReflectionUtilities.GetDelegate> getters = this.GetCache[type];

            foreach (KeyValuePair<string, ReflectionUtilities.GetDelegate> getter in getters)
            {
                if (getter.Value != null)
                {
                    obj.Add(this.MapClrMemberNameToJsonFieldName(getter.Key), getter.Value(input));
                }
            }

            output = obj;

            return true;
        }
    }

    /// <summary>
    /// Class ReflectionUtilities.
    /// </summary>
    internal class ReflectionUtilities
    {
        /// <summary>
        /// The empty objects
        /// </summary>
        private static readonly object[] EmptyObjects = new object[] { };

        /// <summary>
        /// Delegate ConstructorDelegate
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The System.Object.</returns>
        public delegate object ConstructorDelegate(params object[] args);

        /// <summary>
        /// Delegate GetDelegate
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The System.Object.</returns>
        public delegate object GetDelegate(object source);

        /// <summary>
        /// Delegate SetDelegate
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        public delegate void SetDelegate(object source, object value);

        /// <summary>
        /// Delegate ThreadSafeDictionaryValueFactory
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public delegate TValue ThreadSafeDictionaryValueFactory<TKey, TValue>(TKey key);

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="type">The type.</param>
        /// <returns>The Attribute.</returns>
        public static Attribute GetAttribute(MemberInfo info, Type type)
        {
            if (info == null || type == null || !Attribute.IsDefined(info, type))
            {
                return null;
            }

            return Attribute.GetCustomAttribute(info, type);
        }

        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <returns>The Attribute.</returns>
        public static Attribute GetAttribute(Type objectType, Type attributeType)
        {
            if (objectType == null || attributeType == null || !Attribute.IsDefined(objectType, attributeType))
            {
                return null;
            }

            return Attribute.GetCustomAttribute(objectType, attributeType);
        }

        /// <summary>
        /// Gets the constructor.
        /// </summary>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <returns>The ConstructorDelegate.</returns>
        public static ConstructorDelegate GetConstructor(ConstructorInfo constructorInfo)
        {
            return GetConstructorByReflection(constructorInfo);
        }

        /// <summary>
        /// Gets the constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argsType">Type of the arguments.</param>
        /// <returns>The ConstructorDelegate.</returns>
        public static ConstructorDelegate GetConstructor(Type type, params Type[] argsType)
        {
            return GetConstructorByReflection(type, argsType);
        }

        /// <summary>
        /// Gets the constructor by reflection.
        /// </summary>
        /// <param name="constructorInfo">The constructor information.</param>
        /// <returns>The ConstructorDelegate.</returns>
        public static ConstructorDelegate GetConstructorByReflection(ConstructorInfo constructorInfo)
        {
            if (IsEnumerable(constructorInfo.DeclaringType))
            {
                return delegate(object[] args) { return CreateIList(constructorInfo.DeclaringType, args); };
            }
            else
            {
                return delegate(object[] args) { return constructorInfo.Invoke(args); };
            }
        }

        /// <summary>
        /// Gets the constructor by reflection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argsType">Type of the arguments.</param>
        /// <returns>The ConstructorDelegate.</returns>
        public static ConstructorDelegate GetConstructorByReflection(Type type, params Type[] argsType)
        {
            ConstructorInfo constructorInfo = GetConstructorInfo(type, argsType);
            return constructorInfo == null ? null : GetConstructorByReflection(constructorInfo);
        }

        /// <summary>
        /// Gets the constructor information.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="argsType">Type of the arguments.</param>
        /// <returns>The ConstructorInfo.</returns>
        public static ConstructorInfo GetConstructorInfo(Type type, params Type[] argsType)
        {
            IEnumerable<ConstructorInfo> constructorInfos = GetConstructors(type);

            int i;
            bool matches;

            foreach (ConstructorInfo constructorInfo in constructorInfos)
            {
                ParameterInfo[] parameters = constructorInfo.GetParameters();

                if (argsType.Length != parameters.Length)
                {
                    continue;
                }

                i = 0;
                matches = true;

                foreach (ParameterInfo parameterInfo in constructorInfo.GetParameters())
                {
                    if (parameterInfo.ParameterType != argsType[i])
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    return constructorInfo;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the constructors.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The IEnumerable.</returns>
        public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
        {
            return type.GetConstructors();
        }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The IEnumerable.</returns>
        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>
        /// Gets the type of the generic list element.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The Type.</returns>
        public static Type GetGenericListElementType(Type type)
        {
            IEnumerable<Type> interfaces = type.GetInterfaces();

            foreach (Type implementedInterface in interfaces)
            {
                if (IsTypeGeneric(implementedInterface) && implementedInterface.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return GetGenericTypeArguments(implementedInterface)[0];
                }
            }

            return GetGenericTypeArguments(type)[0];
        }

        /// <summary>
        /// Gets the generic type arguments.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The Type[].</returns>
        public static Type[] GetGenericTypeArguments(Type type)
        {
            return type.GetGenericArguments();
        }

        /// <summary>
        /// Gets the get method.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The GetDelegate.</returns>
        public static GetDelegate GetGetMethod(PropertyInfo propertyInfo)
        {
            return GetGetMethodByReflection(propertyInfo);
        }

        /// <summary>
        /// Gets the get method.
        /// </summary>
        /// <param name="fieldInfo">The field information.</param>
        /// <returns>The GetDelegate.</returns>
        public static GetDelegate GetGetMethod(FieldInfo fieldInfo)
        {
            return GetGetMethodByReflection(fieldInfo);
        }

        /// <summary>
        /// Gets the get method by reflection.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The GetDelegate.</returns>
        public static GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
        {
            MethodInfo methodInfo = GetGetterMethodInfo(propertyInfo);
            return delegate(object source) { return methodInfo.Invoke(source, EmptyObjects); };
        }

        /// <summary>
        /// Gets the get method by reflection.
        /// </summary>
        /// <param name="fieldInfo">The field information.</param>
        /// <returns>The GetDelegate.</returns>
        public static GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo)
        {
            return delegate(object source) { return fieldInfo.GetValue(source); };
        }

        /// <summary>
        /// Gets the getter method information.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The MethodInfo.</returns>
        public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetGetMethod(true);
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The IEnumerable.</returns>
        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }

        /// <summary>
        /// Gets the set method.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The SetDelegate.</returns>
        public static SetDelegate GetSetMethod(PropertyInfo propertyInfo)
        {
            return GetSetMethodByReflection(propertyInfo);
        }

        /// <summary>
        /// Gets the set method.
        /// </summary>
        /// <param name="fieldInfo">The field information.</param>
        /// <returns>The SetDelegate.</returns>
        public static SetDelegate GetSetMethod(FieldInfo fieldInfo)
        {
            return GetSetMethodByReflection(fieldInfo);
        }

        /// <summary>
        /// Gets the set method by reflection.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The SetDelegate.</returns>
        public static SetDelegate GetSetMethodByReflection(PropertyInfo propertyInfo)
        {
            MethodInfo methodInfo = GetSetterMethodInfo(propertyInfo);
            return delegate(object source, object value) { methodInfo.Invoke(source, new object[] { value }); };
        }

        /// <summary>
        /// Gets the set method by reflection.
        /// </summary>
        /// <param name="fieldInfo">The field information.</param>
        /// <returns>The SetDelegate.</returns>
        public static SetDelegate GetSetMethodByReflection(FieldInfo fieldInfo)
        {
            return delegate(object source, object value) { fieldInfo.SetValue(source, value); };
        }

        /// <summary>
        /// Gets the setter method information.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>The MethodInfo.</returns>
        public static MethodInfo GetSetterMethodInfo(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetSetMethod(true);
        }

        /// <summary>
        /// Determines whether the type1 is assignable from the type2.
        /// </summary>
        /// <param name="type1">The type1.</param>
        /// <param name="type2">The type2.</param>
        /// <returns>true if the type1 is assignable from type2; otherwise, false.</returns>
        public static bool IsAssignableFrom(Type type1, Type type2)
        {
            return type1.IsAssignableFrom(type2);
        }

        /// <summary>
        /// Determines whether the specified type is nullable type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if the specified type is nullable type; otherwise, false.</returns>
        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// Determines whether the specified type is dictionary.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if the specified type is dictionary]; otherwise, false.</returns>
        public static bool IsTypeDictionary(Type type)
        {
            if (typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                return true;
            }

            if (!type.IsGenericType)
            {
                return false;
            }

            Type genericDefinition = type.GetGenericTypeDefinition();

            return genericDefinition == typeof(IDictionary<,>);
        }

        /// <summary>
        /// Determines whether the specified type is generic.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if the specified type is generic type; otherwise, false.</returns>
        public static bool IsTypeGeneric(Type type)
        {
            return type.IsGenericType;
        }

        /// <summary>
        /// Determines whether the specified type is generic collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if the specified type is generic collection; otherwise, false.</returns>
        public static bool IsTypeGenericCollectionInterface(Type type)
        {
            if (!IsTypeGeneric(type))
            {
                return false;
            }

            Type genericDefinition = type.GetGenericTypeDefinition();

            return genericDefinition == typeof(IList<>)
                || genericDefinition == typeof(ICollection<>)
                || genericDefinition == typeof(IEnumerable<>);
        }

        /// <summary>
        /// Determines whether the specified type is value type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if the specified type is value type; otherwise, false.</returns>
        public static bool IsValueType(Type type)
        {
            return type.IsValueType;
        }

        /// <summary>
        /// Converts to nullable.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="nullableType">Type of the nullable.</param>
        /// <returns>The System.Object.</returns>
        public static object ToNullableType(object obj, Type nullableType)
        {
            return obj == null ? null : Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableType), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates an instance of IList by the specified type which inherit IEnumerable interface.
        /// </summary>
        /// <param name="source">Source Type which inherit IEnumerable interface.</param>
        /// <param name="lengths">An array of 32-bit integers that represent the size of each dimension of the list to create.</param>
        /// <returns>A reference to the newly created IList object.</returns>
        private static IList CreateIList(Type source, params object[] lengths)
        {
            if (IsEnumerable(source) && !IsDictionary(source))
            {
                if (source.IsArray)
                {
                    if (IsNullOrEmpty(lengths))
                    {
                        return Array.CreateInstance(source.GetElementType(), 0);
                    }
                    else
                    {
                        long[] parameters = new long[lengths.Length];

                        for (int i = 0; i < lengths.Length; i++)
                        {
                            var length = lengths[i];

                            try
                            {
                                parameters[i] = (long)length;
                            }
                            catch
                            {
                                parameters[i] = (long)(int)length;
                            }
                        }

                        return Array.CreateInstance(source.GetElementType(), parameters);
                    }
                }
                else
                {
                    return IsNullOrEmpty(lengths)
                        ? (IList)Activator.CreateInstance(source)
                        : (IList)Activator.CreateInstance(source, lengths[0]);
                }
            }

            return null;
        }

        /// <summary>
        /// Check Type inherit IDictionary interface or not.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IDictionary interface; otherwise, false.</returns>
        private static bool IsDictionary(Type source)
        {
            return source.GetInterface("IDictionary") != null;
        }

        /// <summary>
        /// Check Type inherit IEnumerable interface or not.
        /// </summary>
        /// <param name="source">Source Type.</param>
        /// <returns>true if the source Type inherit IEnumerable interface; otherwise, false.</returns>
        private static bool IsEnumerable(Type source)
        {
            return source != typeof(string)
                && source.GetInterface("IEnumerable") != null;
        }

        /// <summary>
        /// Determines whether a sequence is null or empty.
        /// </summary>
        /// <param name="source">Source IEnumerable.</param>
        /// <returns>true if the source sequence is empty; otherwise, false.</returns>
        private static bool IsNullOrEmpty(Array source)
        {
            return source == null || source.Length == 0;
        }

        /// <summary>
        /// Class ThreadSafeDictionary. This class cannot be inherited.
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            /// <summary>
            /// Field _syncRoot.
            /// </summary>
            private readonly object _syncRoot = new object();

            /// <summary>
            /// Field _valueFactory.
            /// </summary>
            private readonly ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;

            /// <summary>
            /// Field _dictionary.
            /// </summary>
            private Dictionary<TKey, TValue> _dictionary;

            /// <summary>
            /// Initializes a new instance of the <see cref="ThreadSafeDictionary{TKey, TValue}"/> class.
            /// </summary>
            /// <param name="valueFactory">The value factory.</param>
            public ThreadSafeDictionary(ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory)
            {
                this._valueFactory = valueFactory;
            }

            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <value>The count.</value>
            public int Count
            {
                get
                {
                    return this._dictionary.Count;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
            /// </summary>
            /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
            public bool IsReadOnly
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            /// <summary>
            /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
            /// </summary>
            /// <value>The keys.</value>
            public ICollection<TKey> Keys
            {
                get
                {
                    return this._dictionary.Keys;
                }
            }

            /// <summary>
            /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
            /// </summary>
            /// <value>The values.</value>
            public ICollection<TValue> Values
            {
                get
                {
                    return this._dictionary.Values;
                }
            }

            /// <summary>
            /// Gets or sets the element with the specified key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns>The Value.</returns>
            public TValue this[TKey key]
            {
                get
                {
                    return this.Get(key);
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            /// <summary>
            /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
            /// </summary>
            /// <param name="key">The object to use as the key of the element to add.</param>
            /// <param name="value">The object to use as the value of the element to add.</param>
            public void Add(TKey key, TValue value)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            public void Clear()
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
            /// </summary>
            /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
            /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
            public bool ContainsKey(TKey key)
            {
                return this._dictionary.ContainsKey(key);
            }

            /// <summary>
            /// Copies to.
            /// </summary>
            /// <param name="array">The array.</param>
            /// <param name="arrayIndex">Index of the array.</param>
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the collection.</returns>
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            /// <summary>
            /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
            /// </summary>
            /// <param name="key">The key of the element to remove.</param>
            /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
            public bool Remove(TKey key)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Gets the value associated with the specified key.
            /// </summary>
            /// <param name="key">The key whose value to get.</param>
            /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
            /// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
            public bool TryGetValue(TKey key, out TValue value)
            {
                value = this[key];
                return true;
            }

            /// <summary>
            /// Adds the value.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns>The value.</returns>
            private TValue AddValue(TKey key)
            {
                TValue result = this._valueFactory(key);

                lock (this._syncRoot)
                {
                    if (this._dictionary == null)
                    {
                        this._dictionary = new Dictionary<TKey, TValue>();
                        this._dictionary[key] = result;
                    }
                    else
                    {
                        TValue value;

                        if (this._dictionary.TryGetValue(key, out value))
                        {
                            return value;
                        }

                        Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this._dictionary);
                        dictionary[key] = result;
                        this._dictionary = dictionary;
                    }
                }

                return result;
            }

            /// <summary>
            /// Gets the specified key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <returns>The Value.</returns>
            private TValue Get(TKey key)
            {
                if (this._dictionary == null)
                {
                    return this.AddValue(key);
                }

                TValue value;

                if (!this._dictionary.TryGetValue(key, out value))
                {
                    return this.AddValue(key);
                }

                return value;
            }
        }
    }
}
