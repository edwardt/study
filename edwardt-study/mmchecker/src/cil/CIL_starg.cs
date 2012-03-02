using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_starg.
	/// </summary>
	public class CIL_starg : CILInstruction
	{
		int argIndex;

		public CIL_starg(string label, int argIndex) : base (label)
		{
			this.argIndex = argIndex;
		}

		public int ArgIndex
		{ 
			get { return argIndex; } 
		}

		public override bool IsThreadLocal()
		{
			return true;
		}

		public override Semantic GetSemantic()
		{
			return Semantic.NONE;
		}

		public override string ToString()
		{
			return label + ":  starg      " + argIndex;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			// can always execute a starg
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// TODONOW: Fix this when semantic for ldarg and starg is clear
			if(CanExecute(threadState) == false)
				throw new Exception("This instruction is not ready to execute");
			VMValue source = threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue target = threadState.GetLocalArgument(argIndex);
			target.IsThreadLocal = true;
			DelayedWrite dw = new DelayedWrite(target.GUID, source.GUID, this);
			threadState.AddPendingAction(dw);
			return threadState.CurrentMethod.GetNextInstruction(this);			
		}
	}
}
