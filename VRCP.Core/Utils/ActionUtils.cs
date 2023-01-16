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
using Tokens.Logging;
using VRCP.Log;

namespace VRCP.Core.Utils
{
    public static class ActionUtils
    {
        public static void SafeInvoke(this Action act)
        {
            try
            {
                if (act != null) act.Invoke();
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Failed to SafeInvoke action: " + ex.Message);
            }
        }

        public static void SafeInvoke<T>(this Action<T> act, T arg1)
        {
            try
            {
                if (act != null) act.Invoke(arg1);
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Failed to SafeInvoke action: " + ex.Message);
            }
        }

        public static void SafeInvoke<T, T1>(this Action<T, T1> act, T arg1, T1 arg2)
        {
            try
            {
                if (act != null) act.Invoke(arg1, arg2);
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Failed to SafeInvoke action: " + ex.Message);
            }
        }

        public static void SafeInvoke<T, T1, T3>(this Action<T, T1, T3> act, T arg1, T1 arg2, T3 arg3)
        {
            try
            {
                if (act != null) act.Invoke(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Failed to SafeInvoke action: " + ex.Message);
            }
        }

        public static void SafeInvoke<T, T1, T3, T4>(this Action<T, T1, T3, T4> act, T arg1, T1 arg2, T3 arg3, T4 arg4)
        {
            try
            {
                if (act != null) act.Invoke(arg1, arg2, arg3, arg4);
            }
            catch (Exception ex)
            {
                Logger<ProductionLoggerConfig>.LogCritical("Failed to SafeInvoke action: " + ex.Message);
            }
        }
    }
}
