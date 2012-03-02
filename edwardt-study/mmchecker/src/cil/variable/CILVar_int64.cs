using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_int32.
	/// </summary>
	public class CILVar_int64 : CILVariable
	{
		public CILVar_int64(string name) : base(name)
		{
		}

		public override string ToString()
		{
			return "int64 " + name;
		}
	}
}
