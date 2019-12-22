using Newtonsoft.Json;
using System.Collections;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class ConditionExpression
    {
        public ConditionExpression()
        {
        }

        public ConditionExpression(
            string attributeName,
            ConditionOperator conditionOperator,
            params object[] values)
            : this(null, attributeName, conditionOperator, values)
        {
        }

        public ConditionExpression(
            string entityName,
            string attributeName,
            ConditionOperator conditionOperator,
            params object[] values)
        {
            EntityName = entityName;
            AttributeName = attributeName;
            Operator = conditionOperator;
            if (values != null)
            {
                _values = new DataCollection<object>(values);
            }
        }

        public ConditionExpression(
            string attributeName,
            ConditionOperator conditionOperator,
            object value)
            : this(attributeName, conditionOperator, new object[] { value })
        {
        }

        public ConditionExpression(
            string entityName,
            string attributeName,
            ConditionOperator conditionOperator,
            object value
            )
            : this(entityName, attributeName, conditionOperator, new object[] { value })
        {
        }

        public ConditionExpression(
            string attributeName,
            ConditionOperator conditionOperator)
            : this(null, attributeName, conditionOperator, new object[] { })
        {
        }

        public ConditionExpression(
            string entityName,
            string attributeName,
            ConditionOperator conditionOperator)
            : this(entityName, attributeName, conditionOperator, new object[] { })
        {
        }

        /// <summary>
        /// Condition Expression constructor.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <param name="values"></param>
        /// <remarks>Need to handler collections differently. esp. Guid arrays.</remarks>
        public ConditionExpression(
            string attributeName,
            ConditionOperator conditionOperator,
            ICollection values)
        {
            AttributeName = attributeName;
            Operator = conditionOperator;
            if (values != null)
            {
                _values = new DataCollection<object>();
                foreach (object obj in values)
                {
                    _values.Add(obj);
                }
            }
        }

        /// <summary>
        /// Name or alias of LinkEntity to which this condition refers to
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityName { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AttributeName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ConditionOperator Operator { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DataCollection<object> Values
        {
            get
            {
                if (_values == null)
                {
                    _values = new DataCollection<object>();
                }
                return _values;
            }

            private set
            {
                _values = value;
            }
        }

        private DataCollection<object> _values;
    }
}