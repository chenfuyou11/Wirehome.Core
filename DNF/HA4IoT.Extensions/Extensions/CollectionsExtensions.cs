using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Extensions.Extensions
{
    public static class CollectionsExtensions
    {
        /// <summary>
        /// Gets a HashCode of a collection by combining the hash codes of its elements. Takes order into account
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Collection to generate the hash code for</param>
        /// <returns>Generated hash code</returns>
        public static int GetHashCodeOfElements<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            unchecked
            {
                int hash = 17;
                foreach (var element in source)
                {
                    hash = hash * 23 + element.GetHashCode();
                }
                return hash;
            }
        }

        /// <summary>
        /// Tests whether two collections have the same elements, in any order. 
        /// </summary>
        /// <remarks>
        /// Slightly cheaper than 'new HashSet{T}(source).SetEquals(other)' in some common, simple cases, but delegates to it
        /// in the worst case.
        /// </remarks>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="other">Other collection</param>
        /// <param name="comparer">Optional comparer to use to compare elements, defaults to the default EqualityComparer{T}</param>
        /// <returns>True if both elements contain only the same elements, false otherwise</returns>
        public static bool EqualsAnyOrder<T>(this IEnumerable<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (other == null)
                throw new ArgumentNullException(nameof(source));

            if (ReferenceEquals(source, other))
                return true;

            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            var sourceAsCollection = source as ICollection<T>;
            var otherAsCollection = other as ICollection<T>;

            if (sourceAsCollection != null && otherAsCollection != null)
            {
                if (sourceAsCollection.Count == 0 && otherAsCollection.Count == 0)
                    return true;
                if (sourceAsCollection.Count != otherAsCollection.Count)
                    return false;
                if (sourceAsCollection.Count == 1 && otherAsCollection.Count == 1)
                    return comparer.Equals(sourceAsCollection.First(), otherAsCollection.First());
            }

            return new HashSet<T>(source, comparer).SetEquals(other);
        }

        /// <summary>
        /// Gets a HashCode of a collection by combining the hash codes of its elements. Returns the same hashcode for any order of elements
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Collection to generate the hash code for</param>
        /// <returns>Generated hash code</returns>
        public static int GetHashCodeOfElementsAnyOrder<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // See http://stackoverflow.com/a/670068/1086121

            int hash = 0;
            int curHash;
            int bitOffset = 0;
            // Stores number of occurences so far of each value.
            var valueCounts = new Dictionary<T, int>();

            foreach (var element in source)
            {
                curHash = EqualityComparer<T>.Default.GetHashCode(element);
                if (valueCounts.TryGetValue(element, out bitOffset))
                    valueCounts[element] = bitOffset + 1;
                else
                    valueCounts.Add(element, bitOffset);

                // The current hash code is shifted (with wrapping) one bit
                // further left on each successive recurrence of a certain
                // value to widen the distribution.
                // 37 is an arbitrary low prime number that helps the
                // algorithm to smooth out the distribution.
                hash = unchecked(hash + ((curHash << bitOffset) |
                    (curHash >> (32 - bitOffset))) * 37);
            }

            return hash;
        }
    }
}
