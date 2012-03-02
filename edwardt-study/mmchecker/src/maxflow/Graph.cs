using System;
using System.Collections;
using System.Collections.Generic;

namespace mmchecker.maxflow
{
	/// <summary>
	/// Summary description for Graph.
	/// </summary>
	public class Graph
	{
		public List<Vertex> vertices;
		Vertex source;
		Vertex sink;
		Edge[] mincut;

		public Graph()
		{
            vertices = new List<Vertex>();
		}

		public void AddVertex()
		{
			vertices.Add(new Vertex(vertices.Count));
		}

		public Vertex GetVertex(int id)
		{
            while (id >= vertices.Count)
                vertices.Add(new Vertex(vertices.Count));
			return (Vertex)vertices[id];
		}

		public Vertex NewVertex()
		{
			AddVertex();
			return GetVertex(vertices.Count - 1);
		}

		public void AddEdge(int id1, int id2, int capacity)
		{
			AddEdge(id1, id2, capacity, null);
		}
		
		public void AddEdge(int id1, int id2, int capacity, object extraInfo)
		{
			Vertex v1 = GetVertex(id1);
			Vertex v2 = GetVertex(id2);
			if(v1.FindEdgeTo(v2) == null)
			{
				Edge e = new Edge(GetVertex(id1), GetVertex(id2), capacity, 0, extraInfo);
				v1.AddForwardEdge(e);
				v2.AddBackwardEdge(e);
			}
			else
			{
				if(capacity == 1)
				{
					v1.FindEdgeTo(v2).ExtraInfo = null;
					v1.FindEdgeTo(v2).capacity = 1000000000;
				}
//				Console.WriteLine("WARNING: Adding the same edge");
			}
		}

		public Vertex Source
		{
			get { return source; }
			set { source = value; }
		}

		public Vertex Sink 
		{
			get { return sink; }
			set { sink = value; }
		}

		public int CountVertex()
		{
			return vertices.Count;
		}

		public int CountEdge()
		{
			int count = 0;
			IEnumerator<Vertex> iter = vertices.GetEnumerator();
			while(iter.MoveNext())
			{
				count += iter.Current.forward.Count;
			}
			return count;

		}

		public int FordFulkerson()
		{
			int flow = 0;
			// TODO: don't need to allocate so many, gauze the buffer size carefully to reduce its size
			int dmax = vertices.Count;
			int mincutValue;
			Vertex[] d = new Vertex[dmax];
			int d1, d2;
			IEnumerator<Vertex> iter;
			ArrayList aMinCut = new ArrayList();

			Vertex p;
			Edge tmpe;
			while(true)
			{
				iter = vertices.GetEnumerator();
				while(iter.MoveNext())
					iter.Current.back = null;
				source.back = source;
				
				d[0] = source;
				d1 = 0;
				d2 = 1;
				while(d1 != d2)
				{
					foreach(Edge e in d[d1].forward)
						if(e.flow < e.capacity)
							if(e.to.back == null)
							{
								e.to.back = d[d1];
								e.to.backType = 1;
								d[d2] = e.to;
								d2 = (d2 + 1) % dmax;
								if(d2 == d1)
								{
									Console.WriteLine("FATAL ERROR: Buffer is not long enough");									
								}
							}
					foreach(Edge e in d[d1].backward)
						if(e.flow > 0)
							if(e.from.back == null)
							{
								e.from.back = d[d1];
								e.from.backType = 2;
								d[d2] = e.from;
								d2 = (d2 + 1) % dmax;
								if(d2 == d1)
								{
									Console.WriteLine("FATAL ERROR: Buffer is not long enough");
								}
							}
					d1++;
					// found a path, can stop now
					if(sink.back != null)
						break;
				}

				// done searching for path, now check whether we can increase the flow
				if(sink.back == null)
					break;
				flow++;
				p = sink;
				while(p != source)
				{
					if(p.backType == 1)
					{
						tmpe = p.back.FindEdgeTo(p);
						tmpe.flow++;
					}
					else
					{
						tmpe = p.FindEdgeTo(p.back);
						tmpe.flow--;
					}
					p = p.back;
				}
			}

			// now we can use the information stored in .back to find the mincut
			iter = vertices.GetEnumerator();
			mincutValue = 0;
			while(iter.MoveNext())
			{
				Vertex v = iter.Current;
				if(v.back != null)
					foreach(Edge e in v.forward)
						if(e.to.back == null)
						{
							aMinCut.Add(e);
							mincutValue += e.flow;
						}
			}
			mincut = new Edge[aMinCut.Count];
			for(int i = 0; i < mincut.Length; i++)
				mincut[i] = (Edge)aMinCut[i];

			// correctness check of output
			if(mincutValue != flow)
				Console.WriteLine("Mincut != flow");
			iter = vertices.GetEnumerator();
			int sum;
			while(iter.MoveNext())
			{
				p = iter.Current;
				sum = 0;
				foreach(Edge e in p.forward)
				{
					if(e.capacity < e.flow)
						Console.WriteLine("flow > capacity");
					sum += e.flow;
				}
				foreach(Edge e in p.backward)
					sum -= e.flow;
				if(p == source)
				{
					if(sum != flow)
						Console.WriteLine("flow from source is wrong");
				}
				else if(p == sink)
				{
					if(sum != -flow)
						Console.WriteLine("flow to sink is wrong");
				}else if(sum != 0)
					Console.WriteLine("flow in != flow out");
			}
			return flow;
		}

		public void PrintGraph()
		{
			IEnumerator<Vertex> iter = vertices.GetEnumerator();
			while(iter.MoveNext())
			{
				foreach(Edge e in iter.Current.forward)
					Console.WriteLine("{0} {1} {2} {3}", e.from.id, e.to.id, e.capacity, e.flow);
			}
		}

		public Edge[] MinCut
		{
			get { return mincut; }
		}
	}
}
