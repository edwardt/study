namespace sorting
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

    static public class BubbleSort
    {
  
		static public IComparable[] SortIterative(IComparable[] srcList)
		{
			if(srcList!=null)
			{
				int count = srcList.Length;
				bool isIfEverSwaped = true;
				if (count > 1) 
				{
					while(isIfEverSwaped)  //while is somehow cleaner
					{
						isIfEverSwaped = false;
						for(int i =0; i < count; i++)
						{
							if(srcList[i].CompareTo(srcList [i+1]) > 0 )
							{
								swap(srcList, i, i+1);
								isIfEverSwaped = true;
							}
						}
					
					}
				}
				
			}
			return srcList;
		}
		
		static void swap(IComparable[] src, int i , int j)
		{
			IComparable temp = src[i];
			src[i] = src[j];
			src[j] = temp;
		}

	    static public IList<int> SortIterativeNotOptimized(IList<int> sourceList)
		{
			if(sourceList!=null)
			{
				int count = sourceList.Count;
				Console.WriteLine("NotOptimized length of list to be sorted => {0}", count);
				for(int numberofPass=1; numberofPass < count; numberofPass ++)
				{
					int i =0;
					for(; i < count; i++)
					{
						if(sourceList[i] < sourceList[i+1])
						{
							swap(sourceList, i ,i+1);
						}
						Console.WriteLine("NotOptimized Number of passes => {0}, " +
										  "Numberof swaps {1}", numberofPass, i);
					}
					
				 }
				
			}
			return sourceList;
		}
		
		static public IList<int> SortIterativeSortedInput(IList<int> sourceList)
		{
			if(sourceList != null)				
			{
				int count = sourceList.Count;
				bool isIfEverSwapped = false;
				
				Console.WriteLine("SortedInput: length of list to be sorted => {0}", count);
				Console.WriteLine("SortedInout: sorted list should only be looked at once");
				for(int numberOfPasses= 1; (numberOfPasses < count) && isIfEverSwapped; numberOfPasses++)
				{
					isIfEverSwapped = false; // reset
					int i =0;
					for(; i < count; i++)
					{
						if(sourceList[i]< sourceList[i+1])
						{
							swap(sourceList,i, i+1);
							isIfEverSwapped = true;
						}
					    Console.WriteLine("SortedInput Number of passes => {0}, " +
										  "Numberof swaps {1}", numberOfPasses, i);
					}
					
				}
				
			}
			return sourceList;
		}
		
        static public IList<int> SortIterative(IList<int> sourceList)
        {
			
			//BubbleSort also referred to sinking sort. I find it better to 
			//take it for each round, the largest element is sunk to the bottom
			//making the sorted sublist one element larger.
			
			//precondition: the list is not null
			//postcondition: total ordered list returned
            
			// special case: 
			// empty list is sorted
			// single item list is sorted
			// null list ignored
			
			//loop invariant, number of inversion pair in list
			Console.Write("SortIterative => " );
			DisplayHelper.printArray(sourceList);

			bool isIfEverSwapped = true;
			//bool isSwapped = false;
			if(sourceList != null)
			{
			   int listcount = sourceList.Count;
			   Console.WriteLine("listcount => {0}", listcount.ToString());
			   
			
          	   for (int numberofPass = 1; 
				     (numberofPass < listcount) & isIfEverSwapped;
				     numberofPass++)
			   {
					isIfEverSwapped = false;
			  		for(int i =0 ; i < (listcount - numberofPass )
					    ; i++)
					{
						if(sourceList[i] <= sourceList[i+1])
						{
							swap (sourceList, i, i+1);
							
							isIfEverSwapped = true;
						}
						//The worst case is when the list is in reverse order
						//then we need to go through all the passes  (the whole list)
						//and do all the swaps for each adjacent pairs. O(n^2) Big O and Omega
						
					}
					
					Console.Write(" number of pass => {0} ",numberofPass);
				}

			}
            return sourceList;
        }
		
		static void swap(IList<int> targetList, int srcIndex, int tgtIndex)
		{
			
			int tmp = targetList[srcIndex];			
			targetList[srcIndex] = targetList[tgtIndex];
			targetList[tgtIndex] = tmp;
		}
		
		
    }
}
