using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILMethodField.
	/// </summary>
	public class CILClassField
	{
		CILVariable variable;
		bool isStatic;

		public CILClassField(CILVariable variable, bool isStatic)
		{
			this.variable = variable;
			this.isStatic = isStatic;
		}

		public bool IsStatic
		{
			get { return isStatic; }
		}

		public CILVariable Variable
		{
			get { return variable; }
		}

		public override string ToString()
		{
			if(isStatic)
				return "static " + variable.ToString();
			else
				return variable.ToString();
		}
	}
}
