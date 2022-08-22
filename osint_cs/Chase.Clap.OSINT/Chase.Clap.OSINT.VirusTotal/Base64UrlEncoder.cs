//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Text;

namespace Chase.Clap.OSINT.VirusTotal
{
    /// <summary>
    /// Encodes and Decodes strings as Base64Url encoding.
    /// </summary>
    public static class Base64UrlEncoder
    {
        private static char base64PadCharacter = '=';
        private static char base64Character62 = '+';
        private static char base64Character63 = '/';
        private static char base64UrlCharacter62 = '-';
        private static char base64UrlCharacter63 = '_';

        /// <summary>
        /// The following functions perform base64url encoding which differs from regular base64 encoding as follows
        /// * padding is skipped so the pad character '=' doesn't have to be percent encoded
        /// * the 62nd and 63rd regular base64 encoding characters ('+' and '/') are replace with ('-' and '_')
        /// The changes make the encoding alphabet file and URL safe.
        /// </summary>
        /// <param name="arg">string to encode.</param>
        /// <returns>Base64Url encoding of the UTF8 bytes.</returns>
        public static string Encode(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            return Encode(Encoding.UTF8.GetBytes(arg));
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64-url digits. Parameters specify
        /// the subset as an offset in the input array, and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="length">An offset in inArray.</param>
        /// <param name="offset">The number of elements of inArray to convert.</param>
        /// <returns>The string representation in base 64 url encoding of length elements of inArray, starting at position offset.</returns>
        /// <exception cref="ArgumentNullException">'inArray' is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or length is negative OR offset plus length is greater than the length of inArray.</exception>
        public static string Encode(byte[] inArray, int offset, int length)
        {
            if (inArray == null)
                throw new ArgumentNullException("inArray");

            return EncodeString(Convert.ToBase64String(inArray, offset, length));
        }

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with base-64-url digits. Parameters specify
        /// the subset as an offset in the input array, and the number of elements in the array to convert.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <returns>The string representation in base 64 url encoding of length elements of inArray, starting at position offset.</returns>
        /// <exception cref="ArgumentNullException">'inArray' is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">offset or length is negative OR offset plus length is greater than the length of inArray.</exception>
        public static string Encode(byte[] inArray)
        {
            if (inArray == null)
                throw new ArgumentNullException("inArray");

            return EncodeString(Convert.ToBase64String(inArray, 0, inArray.Length));
        }

        internal static string EncodeString(string str)
        {
            return UnsafeEncode(str);
        }

        private unsafe static string UnsafeEncode(string str)
        {
            bool needReplace = false;
            int reductionSize = 0;
            if (str[str.Length - 1] == base64PadCharacter)
                reductionSize = 1;

            if (str[str.Length - 2] == base64PadCharacter)
                reductionSize = 2;

            int encodedLength = str.Length - reductionSize;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == base64Character62 || str[i] == base64Character63)
                {
                    needReplace = true;
                    break;
                }
            }

            if (needReplace)
            {
                string encodedString = new string(char.MinValue, encodedLength);
                fixed (char* dest = encodedString)
                {
                    for (int i = 0; i < encodedLength; i++)
                    {
                        if (str[i] == base64Character62)
                            dest[i] = base64UrlCharacter62;
                        else if (str[i] == base64Character63)
                            dest[i] = base64UrlCharacter63;
                        else
                            dest[i] = str[i];
                    }
                }

                return encodedString;
            }
            else
            {
                if (encodedLength == str.Length)
                {
                    return str;
                }
                else
                {
                    string encodedString = new string(char.MinValue, encodedLength);
                    fixed (char* src = str)
                    fixed (char* dest = encodedString)
                    {
                        Buffer.MemoryCopy(src, dest, encodedLength * 2, encodedLength * 2);
                    }

                    return encodedString;
                }
            }
        }

        /// <summary>
        ///  Converts the specified string, which encodes binary data as base-64-url digits, to an equivalent 8-bit unsigned integer array.</summary>
        /// <param name="str">base64Url encoded string.</param>
        /// <returns>UTF8 bytes.</returns>
        public static byte[] DecodeBytes(string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            return UnsafeDecode(str);
        }

        private unsafe static byte[] UnsafeDecode(string str)
        {
            int mod = str.Length % 4;
            if (mod == 1)
                throw new FormatException(str);

            bool needReplace = false;
            int decodedLength = str.Length + (4 - mod) % 4;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == base64UrlCharacter62 || str[i] == base64UrlCharacter63)
                {
                    needReplace = true;
                    break;
                }
            }

            if (needReplace)
            {
                string decodedString = new string(char.MinValue, decodedLength);
                fixed (char* dest = decodedString)
                {
                    int i = 0;
                    for (; i < str.Length; i++)
                    {
                        if (str[i] == base64UrlCharacter62)
                            dest[i] = base64Character62;
                        else if (str[i] == base64UrlCharacter63)
                            dest[i] = base64Character63;
                        else
                            dest[i] = str[i];
                    }

                    for (; i < decodedLength; i++)
                        dest[i] = base64PadCharacter;
                }

                return Convert.FromBase64String(decodedString);
            }
            else
            {
                if (decodedLength == str.Length)
                {
                    return Convert.FromBase64String(str);
                }
                else
                {
                    string decodedString = new string(char.MinValue, decodedLength);
                    fixed (char* src = str)
                    fixed (char* dest = decodedString)
                    {
                        Buffer.MemoryCopy(src, dest, str.Length * 2, str.Length * 2);
                        dest[str.Length] = base64PadCharacter;
                        if (str.Length + 2 == decodedLength)
                            dest[str.Length + 1] = base64PadCharacter;
                    }

                    return Convert.FromBase64String(decodedString);
                }
            }
        }

        /// <summary>
        /// Decodes the string from Base64UrlEncoded to UTF8.
        /// </summary>
        /// <param name="arg">string to decode.</param>
        /// <returns>UTF8 string.</returns>
        public static string Decode(string arg)
        {
            return Encoding.UTF8.GetString(DecodeBytes(arg));
        }
    }
}
