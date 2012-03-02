using System;

namespace mmchecker.vm.observer
{
	/// <summary>
	/// Summary description for SimpleCumulativeObserver.
	/// </summary>
	public class SimpleCumulativeObserver : Observer
	{
		const int MAX = 100;
		int[] counter;
		int maxseen = 0;
		int totalseen = 0;

		public SimpleCumulativeObserver()
		{
			counter = new int[MAX];
		}

		public override void Report(int number)
		{
			if(number < MAX)
			{
				totalseen++;
				if(number > maxseen)
					maxseen = number;
				if(number >= 0)
					counter[number]++;
				if(totalseen % 1000 == 0)
					PrintReport();
			}else
				Console.WriteLine("SimpleCumulativeObserver: seeing too big number in report");
		}

		public override void PrintReport()
		{
			Console.Write("REPORT:{0}:", totalseen);
			for(int i = 0; i <= maxseen; i++)
				Console.Write(" {0}", counter[i]);
			Console.WriteLine();
		}
	}
}
