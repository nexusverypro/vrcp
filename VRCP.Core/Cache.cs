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

namespace VRCP.Core
{
    using System;
    using System.Collections.Generic;
    using VRCP.Log;

    public static class Cache
    {
        public static T Add<T>(int identifier, T value)
        {
            Cache.EnsureCapacity();
            if (!_cacheList.ContainsKey(identifier))
            {
                _cacheList.Add(identifier, new CacheItem(value));
                return value;
            }
            return default(T);
        }

        public static T Get<T>(int identifier)
        {
            Cache.EnsureCapacity();
            if (_cacheList.ContainsKey(identifier)) return (T)_cacheList[identifier].Item;
            return default(T);
        }

        public static T GetOrAdd<T>(int identifier, T defaultValue)
        {
            Cache.EnsureCapacity();
            if (!_cacheList.ContainsKey(identifier))
            {
                _cacheList.Add(identifier, new CacheItem(defaultValue));
                return defaultValue;
            }
            else return (T)_cacheList[identifier].Item;
        }

        public static void RescaleCacheCapacity(int to)
        {
            int prev = _capacity; 
            int now = _capacity = _cacheList.EnsureCapacity(to);

            Logger<ProductionLoggerConfig>.LogWarning($"Ensuring cache capacity change from {prev} to {to}");

            if (now == prev) // it didnt change the capacity
            {
                ErrorHelper.ReportError(ErrorHelper.CAPACITY_CHANGE);
            }
            else return;
        }

        private static void EnsureCapacity()
        {
            int cur = _capacity;
            int now = _cacheList.Count;

            for (int i = 0; i < rescaleFactors.Count; i++)
            {
                // if 'now' is greater than expected, and less than next expected by 10,
                // rescale
                if (i > 0 && now > rescaleFactors[i] && now < rescaleFactors[i + 1] - 10 && !(now > rescaleFactors[i + 1]))
                {
                    Cache.RescaleCacheCapacity(rescaleFactors[i + 1]);
                }
            }
        }

        private static List<int> rescaleFactors = new List<int>()
        {
            0,
            10,
            28,
            48,
            68,
            88,
            128,
            248,
            488,
            688,
            788,
            988,
            1028
        };

        private static int _capacity;
        private static Dictionary<int, CacheItem> _cacheList = new Dictionary<int, CacheItem>();
    }

    public struct CacheItem
    {
        public CacheItem(object item)
        {
            this.Lifetime = TimeSpan.FromHours(CacheItem.DefaultCacheItemLifetimeInHours);
            this.Item = item;
        }

        public static readonly int DefaultCacheItemLifetimeInHours = 2;

        public TimeSpan Lifetime;
        public object Item;
    }
}
