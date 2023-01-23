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

namespace VRCP.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Specifies a Network Adapter Id. Works in GUIDS.
    /// </summary>
    public struct NetworkAdapterId
    {
        /// <summary>
        /// Creates a <see cref="NetworkAdapterId"/>
        /// </summary>
        /// <param name="id">The specified Network Adapter Id.</param>
        public NetworkAdapterId(string id)
        {
            string reparsed = id.Replace("{", "")
                                .Replace("}", "");
            _id = Guid.Parse(reparsed);
        }

        /// <summary>
        /// Creates a <see cref="NetworkAdapterId"/>
        /// </summary>
        /// <param name="id">The specified Network Adapter Id.</param>
        public NetworkAdapterId(Guid id) => _id = id;

        public static implicit operator NetworkAdapterId(string id) => new NetworkAdapterId(id);
        public static implicit operator NetworkAdapterId(Guid id) => new NetworkAdapterId(id);

        public static implicit operator string(NetworkAdapterId id) => id._id.ToString().ToUpper();
        public static implicit operator Guid(NetworkAdapterId id) => id._id;

        private Guid _id;
    }
}
