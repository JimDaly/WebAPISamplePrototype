using System.Collections.Generic;

namespace WebAPISamplePrototype.QueryExpressionTypes
{
    public class DataCollection<T> : System.Collections.ObjectModel.Collection<T>
    {
        internal DataCollection()
            : base()
        {
        }

        internal DataCollection(IList<T> list)
            : base()
        {
            AddRange(list);
        }

        /// <summary>
        /// Add items to the collection
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(params T[] items)
        {
            if (items != null)
            {
                AddRange((IEnumerable<T>)items);
            }
        }

        /// <summary>
        /// Add items from another collection
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            if (null != items)
            {
                foreach (T item in items)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Converts the collection into an array
        /// </summary>
        /// <returns>Array for the collection</returns>
        public T[] ToArray()
        {
            T[] array = new T[this.Count];
            CopyTo(array, 0);

            return array;
        }
    }
}