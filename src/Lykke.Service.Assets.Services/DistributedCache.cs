﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Assets.Core.Services;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Prometheus;
using StackExchange.Redis;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Lykke.Service.Assets.Services
{
    public class DistributedCache<I, T> : IDistributedCache<I, T> where T : class, I
    {
        private const int MaxConcurrentTasksCount = 50;

        private readonly ILog _log;
        //private readonly IDatabase _redisDatabase;
        private readonly string _partitionKey;
        private readonly TimeSpan _expiration;

        private readonly string _name;

        public static readonly Gauge CacheItemCount = Prometheus.Metrics
            .CreateGauge("lykke_distributed_cache_count",
                "Count object in cache.",
                new GaugeConfiguration { LabelNames = new[] { "name", "cache_type" } });

        public DistributedCache(
            ILogFactory logFactory,
            IDatabase redisDatabase,
            TimeSpan expiration,
            string partitionKey)
        {
            _log = logFactory.CreateLog(this);
            //_redisDatabase = redisDatabase;
            _partitionKey = partitionKey;
            _expiration = expiration;

            _name = $"{typeof(I).Name}-{typeof(T).Name}";

            CacheItemCount.WithLabels(_name, "single-item").Set(0);
            CacheItemCount.WithLabels(_name, "list-item").Set(0);
        }

        public string GetCacheKey(string id)
        {
            return $"{_partitionKey}:{id}";
        }

        private readonly Dictionary<string, T> _data = new Dictionary<string, T>();

        public async Task<T> GetAsync(string key, Func<Task<I>> factory)
        {
            try
            {
                lock (_data)
                {
                    if (_data.TryGetValue(GetCacheKey(key), out var cached))
                    {
                        return cached;
                    }
                }

                //var cached = await _redisDatabase.StringGetAsync(GetCacheKey(key));
                //if (cached.HasValue)
                //    return CacheSerializer.Deserialize<T>(cached);

                var result = await factory() as T;
                //if (result != null)
                //    await _redisDatabase.StringSetAsync(GetCacheKey(key), CacheSerializer.Serialize(result), _expiration);

                lock (_data)
                {
                    _data[GetCacheKey(key)] = result;
                    CacheItemCount.WithLabels(_name, "single-item").Set(_data.Count);
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error! key: {key}, I: {typeof(I).Name}; T: {typeof(T)}");
                _log.Warning(e.Message, e);
                return await factory() as T;
            }
        }

        private readonly Dictionary<string, List<T>> _dataList = new Dictionary<string, List<T>>();

        public async Task<IEnumerable<T>> GetListAsync(string key, Func<Task<IEnumerable<I>>> factory)
        {
            try
            {
                //var cached = await _redisDatabase.StringGetAsync(GetCacheKey(key));
                //if (cached.HasValue)
                //    return CacheSerializer.Deserialize<T[]>(cached);

                lock (_dataList)
                {
                    if (_dataList.TryGetValue(GetCacheKey(key), out var cached))
                    {
                        return cached;
                    }
                }

                var result = (await factory()).Cast<T>().ToArray();

                //await _redisDatabase.StringSetAsync(GetCacheKey(key), CacheSerializer.Serialize(result), _expiration);

                lock (_dataList)
                {
                    _dataList[GetCacheKey(key)] = result.ToList();
                    CacheItemCount.WithLabels(_name, "list-item").Set(_dataList.Count);
                }

                return result;
            }
            catch (Exception e)
            {
                _log.Warning(e.Message, e);
                return (await factory()).Cast<T>().ToArray();
            }
        }

        //public async Task<IEnumerable<T>> GetListAsync(
        //    string prefix,
        //    ICollection<string> keys,
        //    Func<T, string> keyExtractor,
        //    Func<IEnumerable<string>, Task<IEnumerable<I>>> factory)
        //{
        //    try
        //    {
        //        List<T> cachedItems;
        //        List<string> notFoundKeys;
        //        if (keys == null)
        //        {
        //            var cached = await _redisDatabase.StringGetAsync(GetCacheKey(prefix));
        //            if (cached.HasValue)
        //            {
        //                var resultItems = CacheSerializer.Deserialize<T[]>(cached);
        //                await CacheDataAsync(
        //                    resultItems,
        //                    keyExtractor,
        //                    prefix);
        //                return resultItems;
        //            }
        //            cachedItems = new List<T>();
        //            notFoundKeys = null;
        //        }
        //        else
        //        {
        //            var cached = await _redisDatabase.StringGetAsync(keys.Select(k => (RedisKey)GetCacheKey($"{prefix}:{k}")).ToArray());
        //            cachedItems = cached.Where(c => c.HasValue).Select(c => CacheSerializer.Deserialize<T>(c)).ToList();
        //            if (cachedItems.Count == keys.Count)
        //                return cachedItems;

        //            notFoundKeys = new List<string>();
        //            var cachedKeysHash = new HashSet<string>(cachedItems.Select(keyExtractor));
        //            foreach (var key in keys)
        //            {
        //                if (cachedKeysHash.Contains(key))
        //                    continue;
        //                notFoundKeys.Add(key);
        //            }
        //        }

        //        var foundResults = (await factory(notFoundKeys)).Cast<T>().ToList();
        //        await CacheDataAsync(
        //            foundResults,
        //            keyExtractor,
        //            prefix);

        //        return cachedItems.Concat(foundResults);
        //    }
        //    catch (Exception e)
        //    {
        //        _log.Warning(e.Message, e);
        //        return (await factory(keys)).Cast<T>();
        //    }
        //}

        public async Task RemoveAsync(string id)
        {
            try
            {
                lock (_data)
                {
                    _data.Clear();
                    CacheItemCount.WithLabels(_name, "single-item").Set(_data.Count);
                }
                lock (_dataList)
                {
                    _dataList.Clear();
                    CacheItemCount.WithLabels(_name, "list-item").Set(_dataList.Count);
                }

                //await _redisDatabase.KeyDeleteAsync(GetCacheKey(id));
            }
            catch (Exception e)
            {
                _log.Warning(e.Message, e);
            }
        }

        //public async Task CacheDataAsync<T>(
        //    IEnumerable<T> items,
        //    Func<T, string> keyExtractor,
        //    string prefix)
        //{
        //    var tasks = new List<Task>();
        //    foreach (var item in items)
        //    {
        //        var key = keyExtractor(item);
        //        tasks.Add(Task.Run(async () =>
        //        {
        //            var cacheKey = GetCacheKey($"{prefix}:{key}");
        //            if (!await _redisDatabase.KeyExistsAsync(cacheKey))
        //                await _redisDatabase.StringSetAsync(cacheKey, CacheSerializer.Serialize(item), _expiration);
        //        }));
        //        if (tasks.Count >= MaxConcurrentTasksCount)
        //        {
        //            await Task.WhenAll(tasks);
        //            tasks.Clear();
        //        }
        //    }
        //    if (tasks.Count > 0)
        //        await Task.WhenAll(tasks);
        //}

        public async Task CacheDataAsync<Y>(string key, IEnumerable<Y> items)
        {
            lock (_dataList)
            {
                var json = items.ToJson();
                var data = JsonConvert.DeserializeObject<List<T>>(json);
                _dataList[GetCacheKey(key)] = data;
                CacheItemCount.WithLabels(_name, "list-item").Set(_dataList.Count);
            }

            //return _redisDatabase.StringSetAsync(GetCacheKey(key), CacheSerializer.Serialize(items), _expiration);
        }
    }
}
