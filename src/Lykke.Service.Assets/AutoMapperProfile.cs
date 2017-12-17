﻿using AutoMapper;
using Lykke.Service.Assets.Core.Domain;
using Lykke.Service.Assets.Responses.V2;

namespace Lykke.Service.Assets
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IAsset,             Asset>();
            CreateMap<IAssetAttributes,   AssetAttributes>();
            CreateMap<IAssetAttribute,    AssetAttribute>();
            CreateMap<IAssetCategory,     AssetCategory>();
            CreateMap<IAssetDescription,  AssetDescription>();
            CreateMap<IAssetExtendedInfo, AssetExtendedInfo>();
            CreateMap<IAssetGroup,        AssetGroup>();
            CreateMap<IAssetPair,         AssetPair>();
            CreateMap<IAssetSettings,     AssetSettings>();
            CreateMap<IErc20Token,        Erc20Token>();
            CreateMap<IIssuer,            Issuer>();
            CreateMap<IMarginAsset,       MarginAsset>();
            CreateMap<IMarginAssetPair,   MarginAssetPair>();
            CreateMap<IMarginIssuer,      MarginIssuer>();
            CreateMap<IWatchList,         WatchList>();
        }
    }
}