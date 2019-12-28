using Newtonsoft.Json;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class LinkEntity
    {
        public LinkEntity()
            : this(null, null, null, null, JoinOperator.Inner)
        {
        }

        public LinkEntity(string linkFromEntityName, string linkToEntityName, string linkFromAttributeName, string linkToAttributeName, JoinOperator joinOperator)
        {
            LinkFromEntityName = linkFromEntityName;
            LinkToEntityName = linkToEntityName;
            LinkFromAttributeName = linkFromAttributeName;
            LinkToAttributeName = linkToAttributeName;
            JoinOperator = joinOperator;

            _columns = new ColumnSet();
            LinkCriteria = new FilterExpression();
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LinkFromAttributeName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LinkFromEntityName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LinkToEntityName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LinkToAttributeName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JoinOperator JoinOperator { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public FilterExpression LinkCriteria { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<LinkEntity> LinkEntities
        {
            get
            {
                if (_linkEntities == null)
                {
                    _linkEntities = new DataCollection<LinkEntity>();
                }
                return _linkEntities;
            }

            private set
            {
                _linkEntities = value;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ColumnSet Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new ColumnSet();
                }
                return _columns;
            }
            set
            {
                _columns = value;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityAlias { get; set; }

        public LinkEntity AddLink(string linkToEntityName, string linkFromAttributeName, string linkToAttributeName)
        {
            return AddLink(linkToEntityName, linkFromAttributeName, linkToAttributeName, JoinOperator.Inner);
        }

        public LinkEntity AddLink(string linkToEntityName, string linkFromAttributeName, string linkToAttributeName, JoinOperator joinOperator)
        {
            LinkEntity link = new LinkEntity(
                LinkFromEntityName,
                linkToEntityName,
                linkFromAttributeName,
                linkToAttributeName,
                joinOperator);

            this.LinkEntities.Add(link);

            return link;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<OrderExpression> Orders
        {
            get
            {
                if (_orders == null)
                {
                    _orders = new DataCollection<OrderExpression>();
                }
                return _orders;
            }

            private set
            {
                _orders = value;
            }
        }

        private ColumnSet _columns;
        private DataCollection<LinkEntity> _linkEntities;
        private DataCollection<OrderExpression> _orders;
    }
}