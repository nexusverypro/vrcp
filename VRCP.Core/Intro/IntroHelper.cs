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

namespace VRCP.Core.Intro
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Helper class that gets called on startup.
    /// </summary>
    public static class IntroHelper
    {
        /// <summary>
        /// Plays an intro.
        /// </summary>
        public static IPromise PlayIntro()
        {
            Promise promise = new Promise();
            IntroHelper.Internal_PlayIntro(promise);
            return promise;
        }

        private static void Internal_PlayIntro(Promise p)
        {
            if (p != null)
            {
                if (_intro == null)
                {
                    _intro = new PromiseTask_Intro();
                    _intro.promise = p;
                    _intro.StartTask();
                }
            }
        }
        private static PromiseTask_Intro _intro;
    }

    public class PromiseTask_Intro
    {
        public Promise promise;
        public bool isPlayingIntro;

        public void StartTask()
        {
            Task.Factory.StartNew(async () =>
            {
                var lines = textToEnter;
                var longestLength = lines.Max(line => line.Length);
                var leadingSpaces = new string(' ', (Console.WindowWidth - longestLength) / 2);
                var centeredText = string.Join(Environment.NewLine,
                    lines.Select(line => leadingSpaces + line));

                for (int i = 0; i < 20; i++)
                {
                    Console.WriteLine();
                }
                Console.WriteLine(centeredText);
                await Task.Delay(2000);
                this.promise.Resolve();
            });
        }

        private static List<string> textToEnter = new List<string>()
        {
            "`7MMF'   `7MF'`7MM\"\"\"Mq.   .g8\"\"\"bgd `7MM\"\"\"Mq. ",
            "  `MA     ,V    MM   `MM..dP'     `M   MM   `MM.",
            "   VM:   ,V     MM   ,M9 dM'       `   MM   ,M9 ",
            "    MM.  M'     MMmmdM9  MM            MMmmdM9  ",
            "    `MM A'      MM  YM.  MM.           MM       ",
            "     :MM;       MM   `Mb.`Mb.     ,'   MM       ",
            "      VF      .JMML. .JMM. `\"bmmmd'  .JMML.     ",
        };
    }
}