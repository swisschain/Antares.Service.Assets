﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.Assets.Cache;
using Lykke.Service.Assets.Responses;
using Lykke.Service.Assets.Responses.v1;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.Assets.Controllers.V1
{
    /// <inheritdoc />
    /// <summary>
    ///    Controller for assets
    /// </summary>
    [Route("api/[controller]")]
    public class AssetsController : Controller
    {
        private readonly ICachedAssetService _assetService;

        public AssetsController(
            ICachedAssetService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        ///    Forcibly updates assets cache
        /// </summary>
        /// <returns></returns>
        [HttpPost("updateCache"), Obsolete]
        [SwaggerOperation("UpdateAssetsCache")]
        public Task UpdateCache()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///    Returns asset by ID
        /// </summary>
        /// <param name="assetId">Asset ID</param>
        [HttpGet("{assetId}"), Obsolete]
        [SwaggerOperation("GetAsset")]
        [ProducesResponseType(typeof(AssetResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(string assetId)
        {
            var asset = await _assetService.GetAsync(assetId);

            if (asset == null)
            {
                return NotFound(ErrorResponse.Create(nameof(assetId), "Asset not found"));
            }

            return Ok(AssetResponseModel.Create(asset));
        }

        /// <summary>
        ///    Returns all assets
        /// </summary>
        [HttpGet, Obsolete]
        [ProducesResponseType(typeof(AssetResponseModel[]), (int)HttpStatusCode.OK)]
        [SwaggerOperation("GetAssets")]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _assetService.GetAllAsync(false);

            return Ok(assets.Select(AssetResponseModel.Create));
        }
    }
}
