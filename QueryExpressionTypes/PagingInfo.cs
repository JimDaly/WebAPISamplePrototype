using Newtonsoft.Json;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class PagingInfo
    {
        /// <summary>
        /// The page to be returned.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PageNumber { get; set; }

        /// <summary>
        /// number of rows per page.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Count { get; set; }

        /// <summary>
        /// If total record count across all pages is required
        /// </summary>
        public bool ReturnTotalRecordCount { get; set; }

        /// <summary>
        /// Page Cookie information, null indicates old paging to be used.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PagingCookie { get; set; }
    }
}