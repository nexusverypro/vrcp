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
    public static class PromiseHelpers
    {
        /// <summary>
        /// Returns a promise that resolves with all of the specified promises have resolved.
        /// Returns a promise of a tuple of the resolved results.
        /// </summary>
        public static IPromise<Tuple<T1, T2>> All<T1, T2>(IPromise<T1> p1, IPromise<T2> p2)
        {
            var val1 = default(T1);
            var val2 = default(T2);
            var numUnresolved = 2;
            var alreadyRejected = false;
            var promise = new Promise<Tuple<T1, T2>>();

            p1
                .Then(val => 
                {
                    val1 = val;
                    numUnresolved--;
                    if (numUnresolved <= 0)
                    {
                        promise.Resolve(Tuple.Create(val1, val2));
                    }
                })
                .Catch(e =>
                {
                    if (!alreadyRejected)
                    {
                        promise.Reject(e);
                    }

                    alreadyRejected = true;
                })
                .Done();

            p2
                .Then(val => 
                {
                    val2 = val;
                    numUnresolved--;
                    if (numUnresolved <= 0)
                    {
                        promise.Resolve(Tuple.Create(val1, val2));
                    }
                })
                .Catch(e =>
                {
                    if (!alreadyRejected)
                    {
                        promise.Reject(e);
                    }

                    alreadyRejected = true;
                })
                .Done();

            return promise;
        }

        /// <summary>
        /// Returns a promise that resolves with all of the specified promises have resolved.
        /// Returns a promise of a tuple of the resolved results.
        /// </summary>
        public static IPromise<Tuple<T1, T2, T3>> All<T1, T2, T3>(IPromise<T1> p1, IPromise<T2> p2, IPromise<T3> p3)
        {
            return All(All(p1, p2), p3)
                .Then(vals => Tuple.Create(vals.Item1.Item1, vals.Item1.Item2, vals.Item2));
        }

        /// <summary>
        /// Returns a promise that resolves with all of the specified promises have resolved.
        /// Returns a promise of a tuple of the resolved results.
        /// </summary>
        public static IPromise<Tuple<T1, T2, T3, T4>> All<T1, T2, T3, T4>(IPromise<T1> p1, IPromise<T2> p2, IPromise<T3> p3, IPromise<T4> p4)
        {
            return All(All(p1, p2), All(p3, p4))
                .Then(vals => Tuple.Create(vals.Item1.Item1, vals.Item1.Item2, vals.Item2.Item1, vals.Item2.Item2));
        }
    }
}
