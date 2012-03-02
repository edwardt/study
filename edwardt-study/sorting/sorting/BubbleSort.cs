using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace sorting
{
    static public class BubbleSort
    {
  
        static public List<int> Sort(List<int> unsortedList)
        {
            List<int> sortedList = new List<int>();
		
           // write code stuff here.
            return sortedList;
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
			
			
			if(sourceList != null)
			{
			   int listcount = sourceList.Count;
			   Console.WriteLine("listcount => {0}", listcount.ToString());
			   bool isAllSwapped = false;
			
          	   for (int numberofPass = 1; 
				     (numberofPass <= listcount) & !isAllSwapped;
				     numberofPass++)
			   {
					//isAllSwapped = false;
			  		for(int i =0 ; (i < listcount - numberofPass )
					    ; i++)
					{
						if(sourceList[i] < sourceList[i+1])
						{
							swap (sourceList, i, i+1);
							//isAllSwapped = true;
							
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
