using System;

namespace mmchecker.maxflow
{
	/// <summary>
	/// Summary description for Edge.
	/// </summary>
	public class Edge
	{
		internal Vertex from;
		internal Vertex to;
		internal int capacity;
		internal int flow;
		object extraInfo;

		public Edge(Vertex from, Vertex to, int capacity, int flow, object extraInfo)
		{
			this.from = from;
			this.to = to;
			this.capacity = capacity;
			this.flow = flow;
			this.extraInfo = extraInfo;
		}

		public object ExtraInfo 
		{
			get { return extraInfo; }
			set { extraInfo = ExtraInfo; }
		}
	}
}
