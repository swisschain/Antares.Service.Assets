﻿using AzureStorage;
using Lykke.Service.Assets.Core.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.Assets.Core.Repositories;
using Lykke.Service.Assets.Repositories.Entities;

namespace Lykke.Service.Assets.Repositories
{
    public class AssetAttributeRepository : IAssetAttributeRepository
    {
        private readonly INoSQLTableStorage<AssetAttributeEntity> _assetAttributeTable;

        public AssetAttributeRepository(
            INoSQLTableStorage<AssetAttributeEntity> assetAttributeTable)
        {
            _assetAttributeTable = assetAttributeTable;
        }
        
        public async Task AddAsync(string assetId, IAssetAttribute attribute)
        {
            var entity = Mapper.Map<AssetAttributeEntity>(attribute);

            entity.PartitionKey = GetPartitionKey(assetId);
            entity.RowKey       = GetRowKey(attribute.Key);

            await _assetAttributeTable.InsertAsync(entity);
        }

        public async Task<IAssetAttribute> GetAsync(string assetId, string key)
        {
            return await _assetAttributeTable.GetDataAsync(GetPartitionKey(assetId), GetRowKey(key));
        }

        public async Task<IEnumerable<(string AssetId, IAssetAttribute Attribute)>> GetAllAsync()
        {
            return (await _assetAttributeTable.GetDataAsync())
                .Select(x => (x.AssetId, (IAssetAttribute) x));
        }

        public async Task<IEnumerable<IAssetAttribute>> GetAllAsync(string assetId)
        {
            return await _assetAttributeTable.GetDataAsync(GetPartitionKey(assetId));
        }

        public async Task RemoveAsync(string assetId, string key)
        {
            await _assetAttributeTable.DeleteIfExistAsync(GetPartitionKey(assetId), GetRowKey(key));
        }

        public async Task UpdateAsync(string assetId, IAssetAttribute attribute)
        {
            await _assetAttributeTable.MergeAsync(GetPartitionKey(assetId), GetRowKey(attribute.Key), x =>
            {
                x.Value = attribute.Value;

                return x;
            });
        }

        private static string GetPartitionKey(string assetId)
            => assetId;

        private static string GetRowKey(string key)
            => key;
    }
}
