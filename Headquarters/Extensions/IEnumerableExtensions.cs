using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Extensions
{
    /// <summary>
    /// Extension class providing extensions for IEnumerables
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Reads a number of objects from an IEnumerable into an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static T[] ReadToArray<T>(this IEnumerable<T> enumerable, int startIndex = 0, int count = 1)
        {
            if (startIndex == 0 && count == enumerable.Count())
            {
                return enumerable.ToArray();
            }

            IEnumerator<T> enumerator = enumerable.GetEnumerator();

            int index = -1;
            while (index < startIndex)
            {
                if (!enumerator.MoveNext())
                {
                    throw new IndexOutOfRangeException("startIndex is outside the bounds of the IEnumerable.");
                }
                index++;
            }

            bool endOfSequence = false;
            T[] array = new T[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = enumerator.Current;
                if (!enumerator.MoveNext())
                {
                    if (endOfSequence)
                    {
                        throw new IndexOutOfRangeException("Index is outside the bounds of the IEnumerable.");
                    }
                    else
                    {
                        endOfSequence = true;
                    }
                }
            }

            return array;
        }
    }
}
