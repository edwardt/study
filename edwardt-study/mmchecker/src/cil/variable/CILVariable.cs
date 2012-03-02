using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILVariable.
	/// </summary>
	public abstract class CILVariable
	{
		protected string name;		

		public CILVariable(string name)
		{
			this.name = name;
		}

		public string Name
		{
			get { return name; }
		}
	}
}
