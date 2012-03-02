using System;
using System.Collections.Generic;

namespace mmchecker.vm
{
	/// <summary>
	/// Containing variables of global scope in the virtual machine
	/// Right now there are only static fields of classes are global
	/// </summary>
	public class VMGlobalVariables
	{
		// contains name-value pairs of static variables
		List<string> names = new List<string>();
		List<int> values = new List<int>();

		State systemState;

		public VMGlobalVariables(State systemState)
		{
			this.systemState = systemState;
		}

		public void AddStaticVariable(CILClass classType, CILVariable variable)
		{
			string name = classType.Name + ":" + variable.Name;
			VMValue v = systemState.Values.MakeValue(variable);
			v.IsConcrete = true;
			v.IsThreadLocal = false;
			for(int i = 0; i < names.Count; i++)
				if(name == names[i])
					return;
				else if(name.CompareTo(names[i]) < 0)
				{
					names.Insert(i, name);
					values.Insert(i, v.GUID);					
					return;
				}
			names.Add(name);
			values.Add(v.GUID);
		}

        /// <summary>
        /// Get the guid of a static variable, which is a static field of a class.
        /// It is global and accessible by a pair of a class name and a variable name. 
        /// </summary>
        /// <param name="classType">The class containing the static field</param>
        /// <param name="variable">The static variable</param>
        /// <returns>The guid of the static variable</returns>
		public int GetStaticVariable(CILClass classType, CILVariable variable)
		{
			string name = classType.Name + ":" + variable.Name;
			for(int i = 0; i < names.Count; i++)
				if(names[i] == name)
					return values[i];
			AddStaticVariable(classType, variable);		
			// the second time we must have got the variable on the list already
			return GetStaticVariable(classType, variable);
		}

		public VMGlobalVariables Duplicate(State oldState, State newState)
		{
			VMGlobalVariables ret = new VMGlobalVariables(newState);
			for(int i = 0; i < names.Count; i++)
			{
				ret.names.Add(names[i]);
				ret.values.Add(values[i]);
			}
			return ret;
		}

		public void TakeSnapshot(StateSnapshot ss)
		{
			ss.WriteInt(names.Count);
			for(int i = 0; i < names.Count; i++)
			{
				ss.WriteString(names[i]);				
				ss.WriteGuid(values[i]);
			}
		}
	}
}
