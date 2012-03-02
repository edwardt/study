using System;
using System.Collections.Generic;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILProgram.
	/// </summary>
	public class CILProgram
	{
		List<CILClass> classes = new List<CILClass>();
		CILMethod entryPoint;

		public CILMethod EntryPoint
		{
			get { return entryPoint; }
			set { entryPoint = value; }
		}

		public CILProgram()
		{
		}

		public List<CILClass> Classes
		{
			get { return classes; }
		}

		public CILClass GetClass(string name)
		{
			foreach(CILClass theClass in classes)
				if(theClass.Name == name)
					return theClass;
			CILClass newClass = new CILClass(name);
			classes.Add(newClass);
			return newClass;
		}

		public int GetInstructionCount()
		{
			int ret = 0;
			foreach(CILClass c in classes)
				ret += c.GetInstructionCount();
			return ret;
		}

		public override string ToString()
		{
			return "CILProgram with " + classes.Count + " classes";
		}

		public string Print()
		{
			string ret = "";
			foreach(CILClass theClass in classes)
				ret += theClass.Print();
			return ret;
		}
	}
}
