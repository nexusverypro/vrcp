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

namespace VRCP.Log
{
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using VRCP.Core;

    public static class Logger
    {
        public static IPromise Trace(string message, params object[] parameters)
            => Logger<ProductionLoggerConfig>.LogTrace(message, parameters);

        public static IPromise Debug(string message, params object[] parameters)
            => Logger<ProductionLoggerConfig>.LogDebug(message, parameters);

        public static IPromise Information(string message, params object[] parameters)
            => Logger<ProductionLoggerConfig>.LogInformation(message, parameters);

        public static IPromise Warning(string message, params object[] parameters)
            => Logger<ProductionLoggerConfig>.LogWarning(message, parameters);

        public static IPromise Error(string message, params object[] parameters)
            => Logger<ProductionLoggerConfig>.LogError(message, parameters);

        public static IPromise Critical(string message, params object[] parameters)
            => Logger<ProductionLoggerConfig>.LogCritical(message, parameters);
    }

    public static class Logger<TConfig> where TConfig : ILoggerConfig, new()
    {
        // i am too good at cs dev
        static Logger()
        {
            _logItems = new Queue<LogQueueItem>();
            (new Thread(() => ProcessLogQueue())).Start();
        }

        private static void ProcessLogQueue()
        {
            LogQueueItem previous = null;
            for (; ; )
            {
                if (((previous != null && previous.promise.CurState == PromiseState.Resolved) || previous == null) && _logItems.Count > 0)
                {
                    var item = previous = _logItems.Dequeue();

                    (new Thread(() =>
                    {
                        bool isFormat = item.param.Any();
                        string finalMessage = isFormat ? string.Format(item.message, item.param) : item.message;

                        var logTypeConfig = LoggerConfiguration.LogTypes[item.type];
                        var prevColor = Console.ForegroundColor;
                        Console.ForegroundColor = logTypeConfig.FriendlyColor;
                        Console.Write(logTypeConfig.FriendlyName);
                        Console.ForegroundColor = prevColor;
                        Console.Write($"[{DateTime.Now.ToString("d")}|{DateTime.Now.ToString("t")}] ");
                        Console.Write(finalMessage);
                        Console.Write("\n");
                        item.promise.Resolve();
                    })).Start();
                }
                Thread.Sleep(1);
            }
        }

        // what a good programmer i am
        public static IPromise LogTrace(string message, params object[] parameters)
            => Logger<TConfig>.Log(new EventId(0x0FF, ""), 0x029, message, parameters);
        public static IPromise LogDebug(string message, params object[] parameters)
            => Logger<TConfig>.Log(new EventId(0x0FF, ""), 0x030, message, parameters);
        public static IPromise LogInformation(string message, params object[] parameters)
            => Logger<TConfig>.Log(new EventId(0x0FF, ""), 0x031, message, parameters);
        public static IPromise LogWarning(string message, params object[] parameters)
            => Logger<TConfig>.Log(new EventId(0x0FF, ""), 0x032, message, parameters);
        public static IPromise LogError(string message, params object[] parameters)
            => Logger<TConfig>.Log(new EventId(0x0FF, ""), 0x033, message, parameters);
        public static IPromise LogCritical(string message, params object[] parameters)
            => Logger<TConfig>.Log(new EventId(0x0FF, ""), 0x034, message, parameters);

        // keep this private for reasons
        private static IPromise Log(EventId eventId, int type, string message, params object[] parameters)
        {
            var item = new LogQueueItem()
            {
                eventId = eventId,
                type = type,
                message = message,
                param = parameters,
                promise = new Promise()
            };

            _logItems.Enqueue(item);
            return item.promise;
        }

        private static bool IsFormatStr(string str) => Regex.IsMatch(str, "/{[0-9]}/gm");

        public static TConfig LoggerConfiguration => Cache.GetOrAdd<TConfig>(LoggerConfigId, Activator.CreateInstance<TConfig>());

        public const int LoggerConfigId = 0x0DE9F18;
        public const string LogFormat = "{0}[{1}|{2}] {3}";

        private static Queue<LogQueueItem> _logItems;
    }

    public class LogQueueItem
    {
        public LogQueueItem() { }

        public EventId eventId;
        public int type;
        public string message;
        public object[] param;

        // promises can slow down the clps (console logs per second) but
        // who cares! not me
        public Promise promise;
    }

    public struct EventId
    {
        public EventId(int id, string name)
        {
            this.identifer = id;
            this.name = name;
        }

        public int identifer;
        public string name;
    }

    public class ProductionLoggerConfig : ILoggerConfig
    {
        public bool IsDebug => false;

        public List<int> AvailableLogTypes
        {
            get;
        } = new List<int>()
        {
            0x029, // trace
            0x030, // debug
            0x031, // info
            0x032, // warn
            0x033, // error
            0x034, // critical
        };

        public Dictionary<int, LogTypeConfig> LogTypes
        {
            get;
        } = new Dictionary<int, LogTypeConfig>()
        {
            { 0x029, new LogTypeConfig("trace", ConsoleColor.Cyan) },
            { 0x030, new LogTypeConfig("debug", ConsoleColor.Magenta) },
            { 0x031, new LogTypeConfig("info", ConsoleColor.Green) },
            { 0x032, new LogTypeConfig("warn", ConsoleColor.Yellow) },
            { 0x033, new LogTypeConfig("error", ConsoleColor.Red) },
            { 0x034, new LogTypeConfig("critical", ConsoleColor.DarkRed) }
        };
    }

    public class DebugLoggerConfig : ILoggerConfig
    {
        public bool IsDebug => true;

        public List<int> AvailableLogTypes
        {
            get;
        } = new List<int>()
        {
            0x029, // trace
            0x030, // debug
            0x031, // info
            0x032, // warn
            0x033, // error
            0x034, // critical
        };

        public Dictionary<int, LogTypeConfig> LogTypes
        {
            get;
        } = new Dictionary<int, LogTypeConfig>()
        {
            { 0x029, new LogTypeConfig("trace", ConsoleColor.Cyan) },
            { 0x030, new LogTypeConfig("debug", ConsoleColor.Magenta) },
            { 0x031, new LogTypeConfig("info", ConsoleColor.Green) },
            { 0x032, new LogTypeConfig("warn", ConsoleColor.Yellow) },
            { 0x033, new LogTypeConfig("error", ConsoleColor.Red) },
            { 0x034, new LogTypeConfig("critical", ConsoleColor.DarkRed) }
        };
    }

    public interface ILoggerConfig
    {
        bool IsDebug { get; }
        List<int> AvailableLogTypes { get; }
        Dictionary<int, LogTypeConfig> LogTypes { get; }
    }

    public class LogTypeConfig
    {
        public LogTypeConfig(string friendlyName, ConsoleColor friendlyColor)
        {
            FriendlyName = friendlyName;
            FriendlyColor = friendlyColor;
        }

        public string FriendlyName { get; private set; }
        public ConsoleColor FriendlyColor { get; private set; }
    }
}
