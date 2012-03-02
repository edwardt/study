using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_int32.
	/// </summary>
	public class CILVar_int32 : CILVariable
	{
		public CILVar_int32(string name) : base(name)
		{
		}

		public override string ToString()
		{
			return "int32 " + name;
		}
	}
}
