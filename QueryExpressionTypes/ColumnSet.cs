using Newtonsoft.Json;
using System;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public sealed class ColumnSet
    {
        public ColumnSet()
        {
            HasLazyFileAttribute = false;
        }

        public ColumnSet(bool allColumns)
        {
            AllColumns = allColumns;
            HasLazyFileAttribute = false;
        }

        public ColumnSet(params string[] columns)
        {
            _columns = new DataCollection<string>(columns);
            HasLazyFileAttribute = false;
        }

        public void AddColumns(params string[] columns)
        {
            foreach (string column in columns)
            {
                Columns.Add(column);
            }
        }

        public void AddColumn(string column)
        {
            Columns.Add(column);
        }

        
        public bool AllColumns { get; set; }

        
        public DataCollection<string> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new DataCollection<string>();
                }
                return _columns;
            }
        }

        
        public DataCollection<XrmAttributeExpression> AttributeExpressions
        {
            get
            {
                if (_attributeExpressions == null)
                {
                    _attributeExpressions = new DataCollection<XrmAttributeExpression>();
                }
                return _attributeExpressions;
            }
        }

        /// <summary>
        /// Gets and sets a flag for a lazy file attribute value. This flag is used
        /// when large files are not routed to plugins.
        /// </summary>
        /// <returns>True if the entity has a lazy file attribute, otherwise false.</returns>
        public bool HasLazyFileAttribute { get; set; }

        /// <summary>
        /// Gets and sets the lazy file attribute's entity name..
        /// </summary>
        /// <returns>True if the entity has a lazy file attribute, otherwise false.</returns>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LazyFileAttributeEntityName { get; set; }

        /// <summary>
        /// Gets and sets the lazy file attribute name;
        /// </summary>
        /// <returns>The lazy file attribute key.</returns>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LazyFileAttributeKey { get; set; }

        /// <summary>
        /// Gets and sets the lazy file attribute value;
        /// </summary>
        /// <returns>A lazy file attribute value.</returns>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public Lazy<object> LazyFileAttributeValue { get; set; }

        /// <summary>
        /// Gets and sets the lazy file attribute value;
        /// </summary>
        /// <returns>A lazy file attribute value.</returns>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        public int LazyFileAttributeSizeLimit { get; set; }

        private DataCollection<string> _columns;
        private DataCollection<XrmAttributeExpression> _attributeExpressions;
    }
}