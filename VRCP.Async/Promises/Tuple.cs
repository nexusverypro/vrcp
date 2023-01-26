// Notices will be in their respective files; "NOTICE.txt".

// --------------------- NOTICE --------------------- //
//                                                    //
// This library was made by RSG (Real Serious Games)  //
// and modified to fit the VRCP needs.                //
//                                                    //
// Link to RSG Website: https://realseriousgames.com/ //
// Link to RSG: https://github.com/Real-Serious-Games //
//                                                    //
// -------------------------------------------------- //

/*

The MIT License (MIT)

Copyright (c) 2014 Real Serious Games

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

namespace VRCP
{
    /// <summary>
    /// Provides static methods for creating tuple objects.
    /// 
    /// Tuple implementation for .NET 3.5
    /// </summary>
    public class Tuple
    {
        /// <summary>
        /// Create a new 2-tuple, or pair.
        /// </summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <returns>A 2-tuple whose value is (item1, item2)</returns>
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        /// <summary>
        /// Create a new 3-tuple, or triple.
        /// </summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <returns>A 3-tuple whose value is (item1, item2, item3)</returns>
        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        /// <summary>
        /// Create a new 4-tuple, or quadruple.
        /// </summary>
        /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
        /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
        /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
        /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
        /// <param name="item1">The value of the first component of the tuple.</param>
        /// <param name="item2">The value of the second component of the tuple.</param>
        /// <param name="item3">The value of the third component of the tuple.</param>
        /// <param name="item4">The value of the fourth component of the tuple.</param>
        /// <returns>A 3-tuple whose value is (item1, item2, item3, item4)</returns>
        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }
    }

    /// <summary>
    /// Represents a 2-tuple, or pair.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    public class Tuple<T1, T2>
    {
        internal Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// Gets the value of the current tuple's first component.
        /// </summary>
        public T1 Item1 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's second component.
        /// </summary>
        public T2 Item2 { get; private set; }
    }

    /// <summary>
    /// Represents a 3-tuple, or triple.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    public class Tuple<T1, T2, T3>
    {
        internal Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        /// <summary>
        /// Gets the value of the current tuple's first component.
        /// </summary>
        public T1 Item1 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's second component.
        /// </summary>
        public T2 Item2 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's third component.
        /// </summary>
        public T3 Item3 { get; private set; }
    }

    /// <summary>
    /// Represents a 4-tuple, or quadruple.
    /// </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam>
    /// <typeparam name="T2">The type of the tuple's second component.</typeparam>
    /// <typeparam name="T3">The type of the tuple's third component.</typeparam>
    /// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
    public class Tuple<T1, T2, T3, T4>
    {
        internal Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        /// <summary>
        /// Gets the value of the current tuple's first component.
        /// </summary>
        public T1 Item1 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's second component.
        /// </summary>
        public T2 Item2 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's third component.
        /// </summary>
        public T3 Item3 { get; private set; }

        /// <summary>
        /// Gets the value of the current tuple's fourth component.
        /// </summary>
        public T4 Item4 { get; private set; }
    }
}
