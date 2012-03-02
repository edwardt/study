using System;

namespace mmchecker
{
	/// <summary>
	/// CILVariable of array type, right now only supports int32 arrays
	/// </summary>
	public class CILVar_array : CILVariable
	{
		CILVariable elementType;

		public CILVar_array(string name, CILVariable elementType) : base(name)
		{
			this.elementType = elementType;
		}

		public override string ToString()
		{
			return elementType.ToString() + "[] " + name;
		}
	}
}
