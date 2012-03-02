using System;
using System.Collections.Generic;
using System.Linq;

namespace sorting
{
	public sealed class RandomStringUtil
	{
		string seedString;
		Random random; 
		public RandomStringUtil(string sourceString,int seed)
		{
			seedString = sourceString;
			random = new Random(seed);
		}
		
		public string GetRandomString()
		{
			return String.Empty;
		//	return new string(seedString.ToCharArray().OrderBy(s => ));
			//return new string(seedString.ToCharArray().
		}
		
		
		
	}
	
	static public class DisplayHelper {
		
		static public void printArray(IList<int> listToPrint)
		{
			if(listToPrint != null)
				foreach(int i in listToPrint)
				{
					Console.Write("{0},",i);	
				}
			Console.WriteLine();
		}
	}
}

