﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Core.Domain;

namespace Lykke.Service.Assets.Core.Repositories
{
    public interface IErc20TokenRepository
    {
        Task<IErc20Token> GetByTokenAddressAsync(string tokenAddress);

        Task<IErc20Token> GetByAssetIdAsync(string assetId);

        Task AddAsync(IErc20Token erc20Token);

        Task UpdateAsync(IErc20Token erc20Token);

        Task<IEnumerable<IErc20Token>> GetAllWithAssetsAsync(IEnumerable<string> assetIds);
    }
}
