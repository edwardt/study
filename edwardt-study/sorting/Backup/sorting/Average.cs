namespace sorting
{
	using System;
	using System.Collections.Generic;
	
    public class OrderStatistics
    {
        // Get the average # in an array. warmup

        static public float AverageArray(IList<int> array)
        {
            float answer = 0;
			if (array != null) 
			{
				int count = array.Count;
				for (int i = 0; i < count; i++)
            	{
                	answer = answer + array[i];
            	}
            	answer = answer / count;
			}
            return answer;
        }
		
		static public int FindTheMiddleItem(IList<int> sourceList)
		{
			int slowIndex = 0;
			int fasterIndex = 0;
			
			return -1;
			
		}
		
		static public int FindMedianSlowWay(IList<int> sourceList)
		{
			//precondition a non empty sourceList
			//postcondition: median index of source list is returned
			//special case: 
			// null list -> return null. no work
			// list with one item -> return item index, no work
			// correctness: list with even number of items -> return floor (n/2) index
			// list with odd number of items -> the middle (n+1)/2.
			
			int index = -1;
			if(sourceList != null)
			{
				//
				;
			}
			
			return index;
		}
		
		static public int FindMedian(IList<int> sourceList)
		{
			int index = -1;
			
			
			
			return index;
			
		}
    
	}
}
