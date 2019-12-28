using Newtonsoft.Json;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class FilterExpression
    {
        public FilterExpression()
        {
        }

        public FilterExpression(LogicalOperator filterOperator)
        {
            FilterOperator = filterOperator;
        }

        public LogicalOperator FilterOperator { get; set; }

        /// <summary>
        /// Filter hint
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FilterHint { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<ConditionExpression> Conditions
        {
            get
            {
                if (_conditions == null)
                {
                    _conditions = new DataCollection<ConditionExpression>();
                }
                return _conditions;
            }

            private set
            {
                _conditions = value;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<FilterExpression> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = new DataCollection<FilterExpression>();
                }
                return _filters;
            }

            private set
            {
                _filters = value;
            }
        }

        public bool IsQuickFindFilter { get; set; } = false;

        /// <summary>
        /// overloaded add multiple value condition to the filter
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="conditionOperator"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public void AddCondition(
            string attributeName,
            ConditionOperator conditionOperator,
            params object[] values)
        {
            Conditions.Add(new ConditionExpression(
                attributeName,
                conditionOperator,
                values));
        }

        public void AddCondition(
            string entityName,
            string attributeName,
            ConditionOperator conditionOperator,
            params object[] values)
        {
            Conditions.Add(new ConditionExpression(
                entityName,
                attributeName,
                conditionOperator,
                values));
        }

        public void AddCondition(ConditionExpression condition)
        {
            Conditions.Add(condition);
        }

        /// <summary>
        /// add filter to current filter
        /// </summary>
        /// <param name="logicalOperator"></param>
        /// <returns></returns>
        public FilterExpression AddFilter(LogicalOperator logicalOperator)
        {
            FilterExpression filter = new FilterExpression
            {
                FilterOperator = logicalOperator
            };
            Filters.Add(filter);
            return filter;
        }

        /// <summary>
        /// add filter to current filter
        /// </summary>
        /// <param name="childFilter">Filter to be added.</param>
        /// <returns></returns>
        public void AddFilter(FilterExpression childFilter)
        {
            if (null != childFilter)
            {
                Filters.Add(childFilter);
            }
        }

        private DataCollection<ConditionExpression> _conditions;
        private DataCollection<FilterExpression> _filters;
    }
}