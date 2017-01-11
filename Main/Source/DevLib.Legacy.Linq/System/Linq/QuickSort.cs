using System.Collections.Generic;

namespace System.Linq
{
    internal class QuickSort<TElement>
    {
        private SortContext<TElement> context;
        private TElement[] elements;
        private int[] indexes;

        private QuickSort(IEnumerable<TElement> source, SortContext<TElement> context)
        {
            this.elements = source.ToArray();
            this.indexes = CreateIndexes(elements.Length);
            this.context = context;
        }

        public static IEnumerable<TElement> Sort(IEnumerable<TElement> source, SortContext<TElement> context)
        {
            var sorter = new QuickSort<TElement>(source, context);

            sorter.PerformSort();

            for (int i = 0; i < sorter.indexes.Length; i++)
                yield return sorter.elements[sorter.indexes[i]];
        }

        private static int[] CreateIndexes(int length)
        {
            var indexes = new int[length];
            for (int i = 0; i < length; i++)
                indexes[i] = i;

            return indexes;
        }

        private int CompareItems(int first_index, int second_index)
        {
            return context.Compare(first_index, second_index);
        }

        private void InsertionSort(int left, int right)
        {
            for (int i = left + 1; i <= right; i++)
            {
                int j, tmp = indexes[i];

                for (j = i; j > left && CompareItems(tmp, indexes[j - 1]) < 0; j--)
                    indexes[j] = indexes[j - 1];

                indexes[j] = tmp;
            }
        }

        // We look at the first, middle, and last items in the subarray.
        // Then we put the largest on the right side, the smallest on
        // the left side, and the median becomes our pivot.
        private int MedianOfThree(int left, int right)
        {
            int center = (left + right) / 2;
            if (CompareItems(indexes[center], indexes[left]) < 0)
                Swap(left, center);
            if (CompareItems(indexes[right], indexes[left]) < 0)
                Swap(left, right);
            if (CompareItems(indexes[right], indexes[center]) < 0)
                Swap(center, right);
            Swap(center, right - 1);
            return indexes[right - 1];
        }

        private void PerformSort()
        {
            // If the source contains just zero or one element, there's no need to sort
            if (elements.Length <= 1)
                return;

            context.Initialize(elements);

            // Then sorts the elements according to the collected
            // key values and the selected ordering
            Sort(0, indexes.Length - 1);
        }

        private void Sort(int left, int right)
        {
            if (left + 3 <= right)
            {
                int l = left, r = right - 1, pivot = MedianOfThree(left, right);
                while (true)
                {
                    while (CompareItems(indexes[++l], pivot) < 0) { }
                    while (CompareItems(indexes[--r], pivot) > 0) { }
                    if (l < r)
                        Swap(l, r);
                    else
                        break;
                }

                // Restore pivot
                Swap(l, right - 1);
                // Partition and sort
                Sort(left, l - 1);
                Sort(l + 1, right);
            }
            else
                // If there are three items in the subarray, insertion sort is better
                InsertionSort(left, right);
        }

        private void Swap(int left, int right)
        {
            int temp = indexes[right];
            indexes[right] = indexes[left];
            indexes[left] = temp;
        }
    }
}
