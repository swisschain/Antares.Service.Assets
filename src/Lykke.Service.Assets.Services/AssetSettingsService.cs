﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Assets.Core.Domain;
using Lykke.Service.Assets.Core.Repositories;
using Lykke.Service.Assets.Core.Services;
using Lykke.Service.Assets.Services.Domain;

namespace Lykke.Service.Assets.Services
{
    public class AssetSettingsService : IAssetSettingsService
    {
        private readonly IAssetSettingsRepository _assetSettingsRepository;


        public AssetSettingsService(
            IAssetSettingsRepository assetSettingsRepository)
        {
            _assetSettingsRepository = assetSettingsRepository;
        }

        public async Task<IAssetSettings> AddAsync(IAssetSettings settings)
        {
            await _assetSettingsRepository.UpsertAsync(settings);

            return settings;
        }

        public IAssetSettings CreateDefault()
        {
            return new AssetSettings();
        }

        public async Task<IEnumerable<IAssetSettings>> GetAllAsync()
        {
            return await _assetSettingsRepository.GetAllAsync();
        }

        public async Task<IAssetSettings> GetAsync(string asset)
        {
            return await _assetSettingsRepository.GetAsync(asset);
        }

        public async Task RemoveAsync(string asset)
        {
            await _assetSettingsRepository.RemoveAsync(asset);
        }

        public async Task UpdateAsync(IAssetSettings settings)
        {
            await _assetSettingsRepository.UpsertAsync(settings);
        }
    }
}