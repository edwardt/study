using System;
using System.IO;
using mmchecker;

namespace mmchecker.vm
{
	/// <summary>
	/// Summary description for OrderingTable.
	/// </summary>
	public class OrderingTable
	{
		static int[,] ruleTable;

		public OrderingTable()
		{
		}

		static public void LoadTable(string filename)
		{
			ruleTable = new int[9,9];
			int i, j;
			StreamReader fin = new StreamReader(filename);
			for(i = 0; i < 9; i++)
			{
				string line = fin.ReadLine();
				string[] ss = line.Split(' ');
				for(j = 0; j < 9; j++)
					ruleTable[i,j] = Int32.Parse(ss[j]);			
			}
			fin.Close();
		}

		static public bool IsAllowed(CILInstruction.Semantic before, CILInstruction.Semantic after)
		{
			return ruleTable[(int)before, (int)after] == 1;
		}
	}
}
