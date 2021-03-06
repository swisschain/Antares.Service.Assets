﻿using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.Assets.Repositories.Entities
{
    public class PredefinedWatchListEntity : TableEntity
    {
        public string AssetIds { get; set; }

        public string Id => RowKey;

        public string Name { get; set; }

        public int Order { get; set; }

        public bool ReadOnly => true;
    }
}
