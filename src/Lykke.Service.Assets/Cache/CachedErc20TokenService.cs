﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Assets.Core.Domain;
using Lykke.Service.Assets.Core.Services;
using Lykke.Service.Assets.Responses.V2;

namespace Lykke.Service.Assets.Cache
{
    public class CachedErc20TokenService : ICachedErc20TokenService
    {
        private readonly IErc20TokenService _tokenService;
        private readonly IDistributedCache<IErc20Token, Erc20Token> _cache;
        private const string WithAssetsKey = "WithAssets";

        public CachedErc20TokenService(
            IErc20TokenService tokenService,
            IDistributedCache<IErc20Token, Erc20Token> cache)
        {
            _cache = cache;
            _tokenService = tokenService;
        }

        public async Task<Erc20Token> AddAsync(IErc20Token token)
        {
            await InvalidateCache();

            await _tokenService.AddAsync(token);

            return AutoMapper.Mapper.Map<Erc20Token>(token);
        }

        public async Task<IEnumerable<Erc20Token>> GetByAssetIdsAsync(string[] assetIds)
        {
            if (assetIds == null || assetIds.Length == 0)
                return new Erc20Token[0];

            if (assetIds.Length == 1)
            {
                var token = await GetByAssetIdAsync(assetIds[0]);
                if (token == null)
                    return new Erc20Token[0];
                return new[] { token };
            }

            return await GetAllWithAssetsAsync(assetIds);
        }

        public Task<Erc20Token> GetByAssetIdAsync(string assetId)
        {
            return _cache.GetAsync(assetId,
                async () => AutoMapper.Mapper.Map<Erc20Token>(await _tokenService.GetByAssetIdAsync(assetId)));
        }

        public async Task UpdateAsync(IErc20Token token)
        {
            await InvalidateCache(token.AssetId);
            await _tokenService.UpdateAsync(token);
        }

        public Task<Erc20Token> GetByTokenAddressAsync(string tokenAddress)
        {
            return _cache.GetAsync(tokenAddress,
                async () => AutoMapper.Mapper.Map<Erc20Token>(await _tokenService.GetByTokenAddressAsync(tokenAddress)));
        }

        private static Dictionary<string, List<Erc20Token>> _erc20 = new Dictionary<string, List<Erc20Token>>();

        public async Task<IEnumerable<Erc20Token>> GetAllWithAssetsAsync(string[] assetIds = null)
        {
            var key = "key";

            if (assetIds != null && assetIds.Any())
            {
                foreach (var id in assetIds)
                {
                    key += id;
                }
            }

            lock (_erc20)
            {
                if (_erc20.TryGetValue(key, out var data))
                {
                    return data;
                }
            }

            var result = AutoMapper.Mapper.Map<List<Erc20Token>>(await _tokenService.GetAllWithAssetsAsync(assetIds ?? new string[]{}));

            lock (_erc20)
            {
                _erc20[key] = result.ToList();
            }

            //return await _cache.GetListAsync(
            //    WithAssetsKey,
            //    assetIds,
            //    a => a.AssetId,
            //    async ids => );

            return result;
        }

        private async Task InvalidateCache(string id = null)
        {
            if (id != null)
            {
                await _cache.RemoveAsync(id);
                await _cache.RemoveAsync($"{WithAssetsKey}:{id}");
            }
            await _cache.RemoveAsync(WithAssetsKey);
        }
    }
}
