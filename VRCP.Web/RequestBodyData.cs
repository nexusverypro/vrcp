// ---------------------------------- NOTICE ---------------------------------- //
// VRCP is made with the MIT License. Notices will be in their respective file. //
// ---------------------------------------------------------------------------- //

/*
MIT License

Copyright (c) 2023 Nexus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCP.Web
{
    public class RequestBodyData
    {
// CS8603:Possible null reference return.
#pragma warning disable CS8603
        public static implicit operator RequestBodyDataResult(RequestBodyData data)
        {
            return data.DataType switch
            {
                BodyDataType.Literal => new RequestBodyDataResult() { Data = data.LiteralData },
                BodyDataType.Form => new RequestBodyDataResult() { Data = data.FormData },
                BodyDataType.Json => new RequestBodyDataResult() { Data = data.JsonData },
                _ => null
            };
        }
#pragma warning restore CS8603

        public string LiteralData { get; set; }
        public object JsonData { get; set; }
        public Dictionary<string, string> FormData { get; set; }
        public BodyDataType DataType
        {
            get
            {
                if (!string.IsNullOrEmpty(LiteralData)) return BodyDataType.Literal;
                else if (JsonData != null) return BodyDataType.Json;
                else return BodyDataType.Form;
            }
        }

        public enum BodyDataType
        {
            Json, Form, Literal
        }
    }

    public class RequestBodyDataResult
    {
        public object Data;
    }
}
