using System;

namespace mmchecker.vm.observer
{
	/// <summary>
	/// Summary description for SimpleObserver.
	/// </summary>
	public class SimpleObserver : Observer
	{
		public SimpleObserver() 
		{
		}

		public override void Report(int number)
		{
			Console.WriteLine("REPORT:{0}", number);
		}
	}
}
