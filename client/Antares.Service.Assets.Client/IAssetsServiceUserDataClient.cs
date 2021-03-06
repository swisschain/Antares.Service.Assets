﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Antares.Service.Assets.Client.Models;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Core.Domain;

namespace Antares.Service.Assets.Client
{
    public interface IAssetsServiceUserDataClient
    {
        IWatchListsClient WatchLists { get; }

        IAvailableAssetClient AvailableAssets { get; }

        IAssetsServiceHttp HttpClient { get; }
    }

    public interface IWatchListsClient
    {
        Task<IWatchList> AddCustomAsync(WatchListDto watchList, string clientId);
        Task UpdateCustomWatchListAsync(string clientId, WatchListDto watchList);
        Task RemoveCustomAsync(string watchListId, string clientId);
        Task<IWatchList> AddPredefinedAsync(WatchListDto watchList);
        Task UpdatePredefinedAsync(WatchListDto watchList);

        Task<List<IWatchList>> GetAllCustom(string clientId);
        Task<IWatchList> GetCustomWatchListAsync(string clientId, string watchListId);
        Task<IWatchList> GetPredefinedWatchListAsync(string watchListId);
    }

    public interface IAvailableAssetClient
    {
        Task<List<string>> GetAssetIds(string clientId, bool isIosDevice);
    }
}
