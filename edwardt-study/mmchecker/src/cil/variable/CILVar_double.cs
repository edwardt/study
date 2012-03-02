using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILVar_double.
	/// </summary>
	public class CILVar_double : CILVariable
	{
		public CILVar_double(string name) : base(name)
		{
		}

		public override string ToString()
		{
			return "double " + name;
		}

	}
}
