using System;

namespace mmchecker.util
{
	/// <summary>
	/// Mimic the pair<> in stl
	/// Don't write as generic since C# doesn't support and 
	/// don't want to cast every time
	/// </summary>
	public class IntPair
	{
		int first, second;

		public IntPair(int first, int second)
		{
			this.first = first;
			this.second = second;
		}

		public int First 
		{
			get { return first; }
			set { first = value; }
		}

		public int Second 
		{
			get { return second; }
			set { second = value; }
		}

        public static void QSortIntPair(IntPair[] arr, int l, int r)
        {
            int i = l;
            int j = r;
            int x = arr[(i + j) / 2].Second;
            IntPair tmp;
            do
            {
                while (arr[i].Second > x)
                    i++;
                while (arr[j].Second < x)
                    j--;
                if (i <= j)
                {
                    tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
                    i++; j--;
                }
            } while (i < j);
            if (i < r)
                QSortIntPair(arr, i, r);
            if (l < j)
                QSortIntPair(arr, l, j);
        }
	}
}
