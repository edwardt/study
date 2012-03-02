using System;
using System.Timers;
using System.Collections.Generic;


namespace sorting
{
	public static class RandomStringUtil
	{
		static char[] seedString = {'a','b','c','d','e',
                             'f','g','h','i','j',
                             'k','l','m','n','o',
                             'p','q','r','s','t',
                             'u','v','w','x','y',
                             'z'};

        static public string GetRandomString(int size)
		{
            char[] charArray = null;
            string result = null;
            if (size > 0)
            {
                charArray = new char[size];
                int seedSize = seedString.Length - 1; 
                Random random = new Random((int)TimeSpan.TicksPerSecond);

                for (int i = 0; i < size; i++)
                { 
                    charArray[i] = seedString[random.Next(0,seedSize)];
                }    
                result = charArray.ToString();   
            }
            return result;
		}

	
	}

    static public class RandomIntegerList {

        static public IList<int> GetRandomIntList(int size, TimeSpan time, int minVal, int maxVal)
        {
            IList<int> intList = null;
            if(size>0)
            {
                 Random randGen = new Random((int)TimeSpan.TicksPerSecond);
                 
                 intList = new List<int>(size);
                 for(int i =0; i < size;  i++) 
                 {
                     intList[i] = randGen.Next(minVal,maxVal); 
                 }
            }
            return intList; 
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

