using System;
using System.Collections;

namespace mmchecker.maxflow
{
	/// <summary>
	/// Summary description for Vertex.
	/// </summary>
	public class Vertex
	{
		internal ArrayList forward, backward;
		internal Vertex back;
		internal int backType;
		internal int id;
        internal int color;
        internal int propertyValue = -1;

		public Vertex(int id)
		{
			this.id = id;
			forward = new ArrayList();
			backward = new ArrayList();
		}

		internal void AddForwardEdge(Edge e)
		{
			forward.Add(e);
		}

		internal void AddBackwardEdge(Edge e)
		{
			backward.Add(e);
		}

		internal Edge FindEdgeTo(Vertex v)
		{
			foreach(Edge e in forward)
				if(e.to == v)
					return e;
			return null;
		}

		public int ID 
		{
			get { return id; }
		}

        public int Color
        {
            get { return color; }
            set { color = value; }
        }

        public int PropertyValue
        {
            get { return propertyValue; }
            set { propertyValue = value; }
        }
	}
}
