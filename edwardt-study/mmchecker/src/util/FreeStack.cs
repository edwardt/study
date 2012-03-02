using System;
using System.Collections.Generic;
using System.Collections;

namespace mmchecker
{
	/// <summary>
	/// Summary description for FreeStack.
	/// </summary>
	public class FreeStack <T> : List<T>
	{

		public FreeStack()
		{
		}

		public void Push(T obj)
		{
			this.Add(obj);
		}

		public T Pop()
		{
			T ret = this[this.Count - 1];
			this.RemoveAt(this.Count - 1);
			return ret;
		}

		public T Peek()
		{
			return this[this.Count - 1];
		}

		public T Peek(int index)
		{
			return this[this.Count - 1 - index];
		}
	}
}
