﻿// ---------------------------------- NOTICE ---------------------------------- //
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

namespace VRCP.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using VRCP.Log;

    public static class ErrorHelper
    {
        public static void ReportError(int error)
        {
            Logger<ProductionLoggerConfig>.LogError($"Error at 0x{error.ToString("x")}! {ErrorHelper.DEFAULT}");
        }
        public static readonly int CAPACITY_CHANGE      = 917836812;
        public static readonly int PCAP_ERROR           = 816231278;
        public static readonly int PCAP_CAPTURE_ERROR   = 148920091;

        public static readonly string DEFAULT       = "Is something configured incorrectly?";
    }
}
