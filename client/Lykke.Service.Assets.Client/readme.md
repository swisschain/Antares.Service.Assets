# Lykke.Service.Balances 

# Purpose

  - access to the Lykke.Service.Assets service.

# Usages

1) Autofac module
builder.RegisterAssetsClient(_settings.CurrentValue.AssetsServiceClient.ServiceUrl);
2) CqrsModule
Register.BoundedContext(<context-name>)
  .WithAssetsReadModel()
3) Your code:
IAssetPairsReadModelRepository TryGet, GetAll, TryGetIfEnabled, GetAllEnabled
IAssetsReadModelRepository TryGet, GetAll, TryGetIfEnabled, GetAllEnabled
