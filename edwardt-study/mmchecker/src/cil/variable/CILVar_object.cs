using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILVar_object.
	/// </summary>
	public class CILVar_object : CILVariable
	{
		CILClass classType;

		public CILVar_object(string name, CILClass classType) : base(name)
		{
			this.classType = classType;
		}

		public CILClass ClassType
		{
			get { return classType; }
		}

		public override string ToString()
		{
			return "class " + classType.Name + " " + name;
		}
	}
}
