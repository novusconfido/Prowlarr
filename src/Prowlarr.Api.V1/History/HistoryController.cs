using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.History;
using Prowlarr.Http;
using Prowlarr.Http.Extensions;

namespace Prowlarr.Api.V1.History
{
    [V1ApiController]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        [Produces("application/json")]
        public PagingResource<HistoryResource> GetHistory([FromQuery] PagingRequestResource paging, int? eventType, bool? successful, string downloadId)
        {
            var pagingResource = new PagingResource<HistoryResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<HistoryResource, NzbDrone.Core.History.History>("date", SortDirection.Descending);

            if (eventType.HasValue)
            {
                var filterValue = (HistoryEventType)eventType.Value;
                pagingSpec.FilterExpressions.Add(v => v.EventType == filterValue);
            }

            if (successful.HasValue)
            {
                var filterValue = successful.Value;
                pagingSpec.FilterExpressions.Add(v => v.Successful == filterValue);
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                pagingSpec.FilterExpressions.Add(h => h.DownloadId == downloadId);
            }

            return pagingSpec.ApplyToPage(_historyService.Paged, MapToResource);
        }

        [HttpGet("since")]
        [Produces("application/json")]
        public List<HistoryResource> GetHistorySince(DateTime date, HistoryEventType? eventType = null)
        {
            return _historyService.Since(date, eventType).Select(MapToResource).ToList();
        }

        [HttpGet("indexer")]
        [Produces("application/json")]
        public List<HistoryResource> GetIndexerHistory(int indexerId, HistoryEventType? eventType = null, int? limit = null)
        {
            if (limit.HasValue)
            {
                return _historyService.GetByIndexerId(indexerId, eventType).Select(MapToResource).Take(limit.Value).ToList();
            }

            return _historyService.GetByIndexerId(indexerId, eventType).Select(MapToResource).ToList();
        }

        protected HistoryResource MapToResource(NzbDrone.Core.History.History model)
        {
            return model.ToResource();
        }
    }
}
