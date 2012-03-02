using System;

namespace mmchecker.vm
{
	/// <summary>
	/// VMStaticVariable stores the static variables of a class
	/// The name of the VMStaticVariable is in format Class.VariableName
	/// </summary>
	public class VMStaticVariable
	{
		string name;
		long valueguid;

		public VMStaticVariable(string name, VMValue value)
		{
			this.name = name;
			this.valueguid = value.GUID;
		}

		public long ValueGUID 
		{
			get { return valueguid; }
		}

		public override string ToString()
		{
			return "VMStaticVariable(name=" + name + ",valueguid=" + valueguid + ")";
		}
	}
}
