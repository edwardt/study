using System;
using System.IO;

namespace mmchecker.maxflow
{
	/// <summary>
	/// Summary description for TestMaxFlow.
	/// </summary>
	public class TestMaxFlow
	{
		public static void Test()
		{
			Graph g = new Graph();
			StreamReader fin = new StreamReader("maxflow.in");
			string str = fin.ReadLine();
			string[] ss = str.Split();
			int s = Int32.Parse(ss[0]);
			int t = Int32.Parse(ss[1]);
			int i, j, c;
			while(true)
			{
				str = fin.ReadLine();
				if(str == null)
					break;
				if(str.Length < 4)
					break;

				ss = str.Split();
				i = Int32.Parse(ss[0]);
				j = Int32.Parse(ss[1]);
				c = Int32.Parse(ss[2]);
				g.AddEdge(i, j, c);
			}
			g.Source = g.GetVertex(s);
			g.Sink = g.GetVertex(t);
			Console.WriteLine(g.FordFulkerson());

			int n = 10000;
			g = new Graph();
			Random r = new Random();
			g.Source = g.GetVertex(0);
			for(i = 0; i < n; i++)
				for(j = 0; j < n; j++)
					if(i != j)
						if(r.Next(100) < 1)			
							g.AddEdge(i, j, r.Next(2) + 1);
			g.Sink = g.GetVertex(n - 1);
			Console.WriteLine(g.FordFulkerson());
		}
	}
}
