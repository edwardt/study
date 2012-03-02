using System;
using System.Collections;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_newobj.
	/// </summary>
	public class CIL_newobj : CILInstruction
	{
		CILClass classType;
		string ctorSig;

		public CIL_newobj(string label, CILClass classType, string ctorSig) : base (label)
		{
			this.classType = classType;
			this.ctorSig = ctorSig;
		}

		public override bool IsThreadLocal()
		{
			// TODO: newobj is a complex instruction, need to consider
			// and implement fully to support all possible behaviours
			// newobj should be local but then the control will be 
			// transferred to the constructor code, and execution may 
			// be blocked there
			// newobj leads to a full method call
			return true;
		}

		public override Semantic GetSemantic()
		{
			return Semantic.NONE;
		}	

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("FATAL: Calling execute on an unexecutable instruction state");

			// thread special data type
			//  .ThreadStart
			//  .Thread
			if(classType.Name == "[mscorlib]System.Threading.ThreadStart")
			{
				VMValue_ftn method = (VMValue_ftn)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_object theobj = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_threadstart ts = threadState.SystemState.Values.MakeThreadStartValue(theobj, method);
				ts.IsConcrete = true;
				VMValue_object ret = (VMValue_object)threadState.SystemState.Values.MakeValue(new CILVar_object("", new CILClass("[mscorlib]System.Threading.ThreadStart")));
				ret.ValueGUID = ts.GUID;
				ret.IsThreadLocal = true;
				ret.IsConcrete = true;
				threadState.ThreadStack.Push(ret.GUID);
			}
			else if(classType.Name == "[mscorlib]System.Threading.Thread")
			{
				VMValue_object o = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_thread thethread = threadState.SystemState.Values.MakeThreadValue((VMValue_threadstart)threadState.GetValue(o.ValueGUID));
				thethread.IsConcrete = true;
				VMValue_object ret = (VMValue_object)threadState.SystemState.Values.MakeValue(new CILVar_object("", new CILClass("[mscorlib]System.Threading.Thread")));
				ret.ValueGUID = thethread.GUID;
				ret.IsThreadLocal = true;
				ret.IsConcrete = true;
				// TODO: be careful if we support enumeration of threads
				// then created threads are not local, maybe not here
				// because the thread has not started and cannot be
				// enumerated but it may have connection.
				threadState.ThreadStack.Push(ret.GUID);
			}
			else
			{
				// TODONOW: call the constructor function
				// right now we only make the data for the object instance and clean the stack, put the new obj on
				if(ctorSig.Equals("()") == false)
				{
					int counter = 1;
					int i;
					for(i = 0; i < ctorSig.Length; i++)
						if(ctorSig[i] == ',')
							counter++;
					for(i = 0; i < counter; i++)
						threadState.ThreadStack.Pop();
				}
				VMValue_object obj = (VMValue_object)threadState.SystemState.Values.MakeValue(new CILVar_object("", classType));
				VMValue_objectinst objinst = (VMValue_objectinst)threadState.SystemState.Values.MakeObjectInstance(new CILVar_object("", classType));
				objinst.IsConcrete = true;
				obj.ValueGUID = objinst.GUID;
				obj.IsThreadLocal = true;
				obj.IsConcrete = true;
				threadState.ThreadStack.Push(obj.GUID);
				return threadState.CallFunction(classType.GetMethod(".ctor", "()"));
			}
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		public override bool CanExecute(ThreadState threadState)
		{
			if(ctorSig.Equals("()"))
				return true;
			else
			{
				int counter = 1;
				for(int i = 0; i < ctorSig.Length; i++)
					if(ctorSig[i] == ',')
						counter++;
				FreeStack<int> fs = threadState.ThreadStack;
				for(int i = 1; i <= counter; i++)
					if(((VMValue)threadState.GetValue(fs[fs.Count - i])).IsConcrete == false)
						return false;
				return true;
			}
		}

		public override string ToString()
		{
			return label + ":  newobj     " + classType.Name + "::.ctor(" + ctorSig + ")";
		}
	}
}
