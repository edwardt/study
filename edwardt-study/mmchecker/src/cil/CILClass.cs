using System;
using System.Collections.Generic;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CILClass.
	/// </summary>
	public class CILClass
	{
		string name;
		// unique id for each class
		int id; 

		// contains CILMethod
		List<CILMethod> methods = new List<CILMethod>();

		// contains CILClassField
		List<CILClassField> fields = new List<CILClassField>();

		// contains name of CILClass
		List<string> parentClasses = new List<string>();

		bool isInitialized = false;
		
		public bool IsInitialized
		{
			get { return isInitialized; }
			set { isInitialized = value; }
		}

		// generate unique id for each class
		static int classIdGenerator = 0;

		/// <summary>
		/// Initialize an empty class for reference
		/// class detail will be filled later
		/// </summary>
		public CILClass(string name)
		{
			this.name = name;
			this.id = classIdGenerator++;
		}

		public List<CILClassField> Fields
		{
			get { return fields; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public int ID 
		{
			get { return id; }
		}

		public void AddField(CILClassField field)
		{
			fields.Add(field);
		}

		public void AddParentClass(string name)
		{
			// TODO
			// right now store the name because we don't care
			// but in future ParentClasses should store CILClass
			// CILClass should be abled to be instantiated with only a name
			// then add in details later
			// so we must have a central store of all the classes
			// and when we parse a new class, pick the class from there to process
			parentClasses.Add(name);
		}

		public CILMethod GetMethod(string methodName, string methodSig)
		{
			// TODO
			// now methods are identified by name only
			// must change to method signature soon
			foreach(CILMethod method in methods)
				if((method.Name == methodName) && (method.Sig == methodSig))
					return method;
			CILMethod ret = new CILMethod(this, methodName, methodSig);
			methods.Add(ret);
			return ret;
		}

		public CILClassField GetField(string name)
		{
			foreach(CILClassField field in fields)
				if(field.Variable.Name == name)
					return field;
			return null;
		}

		public IEnumerator<CILClassField> GetFieldEnumerator()
		{
			return fields.GetEnumerator();
		}

		public int GetInstructionCount()
		{
			int ret = 0;
			foreach(CILMethod m in methods)
				ret += m.GetInstructionCount();
			return ret;
		}

		public override string ToString()
		{
			return "class " + name;
		}

		public string Print()
		{
			string ret = "class  " + Name + "\n";

			foreach(string parentclass in parentClasses)
				ret = ret + "   extends " + parentclass.ToString() + "\n";

			ret += "{\n";
			foreach(CILClassField field in fields)
				ret += field.ToString() + "\n";

			foreach(CILMethod themethod in methods)
				ret += themethod.Print();
			ret += "}\n\n";

			return ret;
		}
	}
}
