using System;
using System.Collections;
using System.Collections.Generic;

namespace mmchecker.vm
{
	/// <summary>
	/// Contains local variables' names and values for a method
	/// </summary>
	public class VMLocalVariableBlock
	{
		ThreadState threadState;

		// contains guids of local variables
		List<int> variables = new List<int>();

        // contains guids of arguments passed to the method
		List<int> arguments = new List<int>();

		public VMLocalVariableBlock(ThreadState threadState, CILMethod method)
		{
			this.threadState = threadState;
			IEnumerator iter = method.GetLocalVariableEnumerator();
			while(iter.MoveNext())
			{
				CILVariable variable = (CILVariable)iter.Current;
				VMValue v = threadState.SystemState.Values.MakeValue(variable);
				v.IsThreadLocal = true;
				v.IsConcrete = true;
				variables.Add(v.GUID);
			}			
		}

		public VMValue GetVariableValue(int index)
		{
			return threadState.GetValue(variables[index]);
		}

		public void AddArgumentFront(VMValue value)
		{
			arguments.Insert(0, value.GUID);
		}

		public void AddArgumentFront(int valueguid)
		{
			arguments.Insert(0, valueguid);
		}

		public VMValue GetArgument(int index)
		{
			return threadState.GetValue(arguments[index]);
		}
		
		/// <summary>
		/// Private constructor for Duplicate()
		/// </summary>
		private VMLocalVariableBlock()
		{
		}

		public VMLocalVariableBlock Duplicate(ThreadState oldState, ThreadState newState)
		{
			VMLocalVariableBlock ret = new VMLocalVariableBlock();
			ret.threadState = newState;

			for(int i = 0; i < variables.Count; i++)
				ret.variables.Add(variables[i]);
	
			for(int i = 0; i < arguments.Count; i++)
				ret.arguments.Add(arguments[i]);

			return ret;
		}

		public void TakeSnapshot(StateSnapshot ss)
		{
			ss.WriteInt(variables.Count);
			for(int i = 0; i < variables.Count; i++)
				ss.WriteGuid(variables[i]);

			ss.WriteInt(arguments.Count);
			for(int i = 0; i < arguments.Count; i++)
				ss.WriteGuid(arguments[i]);
		}

        public void MarkAndSweep(int threadID, VMValueManager valueMan)
        {
            foreach (int i in variables)
                valueMan.GetValue(i).MarkAndSweep(threadID, valueMan);
            foreach (int i in arguments)
                valueMan.GetValue(i).MarkAndSweep(threadID, valueMan);
        }
	}
}
