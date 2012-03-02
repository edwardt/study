using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sorting
{
    class Program
    {
        static void Main(string[] args)
        {
            // Bubble Sort
            // Quick Sort
            // Insertion Sort
            // Selection Sort
            // Merge Sort
            // Heap Sort
            // Shell Sort
            //http://www.squidoo.com/sorting-algorithms

            // Test out average
            int[] testAvgArray = new int[] { 1, 2, 3, 4, 7};

            
            float answer = OrderStatistics.AverageArray(testAvgArray);
            Console.WriteLine("answer Average is 20 =>{0}", answer);
			
			int[] emptyList = new int[0];
			Console.WriteLine("median index => {0}", OrderStatistics.FindMedianSlowWay(emptyList));
			
			Console.WriteLine("median index => {0}", OrderStatistics.FindMedianSlowWay(null));
			int[] singleItemList = new int[] {10};
			Console.WriteLine("median index => {0}", OrderStatistics.FindMedianSlowWay(singleItemList));
			
			int[] evenNumberList = new int[] {9,8};
			Console.WriteLine("median index => {0}", OrderStatistics.FindMedianSlowWay(evenNumberList));
			
			int[] oddNumberList = new int[] {10,6, 3};
			Console.WriteLine("median index => {0}", OrderStatistics.FindMedianSlowWay(oddNumberList));
			
			
			//Sorting
			Console.WriteLine("**************************");
			IList<int> bubblesortArray = null;
			IList<int> result = BubbleSort.SortIterative(bubblesortArray);
		
			Console.WriteLine("Ans: bubblesort null list => ");
			DisplayHelper.printArray(result);
			Console.WriteLine("**************************");
			bubblesortArray = new int[0];
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans: bubblesort 0 element list => ");
			DisplayHelper.printArray(result);
			
			Console.WriteLine("**************************");
			bubblesortArray = new int[] {1};
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans: bubblesort 1 element list => ");
			DisplayHelper.printArray(result);
			
			Console.WriteLine("**************************");
			bubblesortArray = new int[] {5,2,9};
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans: bubblesort 3 element list => ");
			DisplayHelper.printArray(result);
			
			Console.WriteLine("**************************");
			bubblesortArray = new int[] {8,2};
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans: bubblesort 2 element list => ");
			DisplayHelper.printArray(result);
			
			Console.WriteLine("**************************");
			bubblesortArray = new int[] {0,8,2,0,3,6,1,2,0,12,9,3};
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans: bubblesort 11 element list => ");
			DisplayHelper.printArray(result);
			
			Console.WriteLine("**************************");
			bubblesortArray = new int[] {9,8,7,6,5,4,3,2,1,0,0};
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans: bubblesort already sorted element list => ");
			DisplayHelper.printArray(result);
		
			Console.WriteLine("**************************");
			bubblesortArray = new int[] {0,0,1,2,3,4,5,6,7,8,9,10};
			result = BubbleSort.SortIterative(bubblesortArray);
			Console.WriteLine("Ans:bubblesort reversly sorted element list => ");
			DisplayHelper.printArray(result);
		
        }
    }
}
