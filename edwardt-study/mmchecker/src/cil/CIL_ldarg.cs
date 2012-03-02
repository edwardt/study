using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_ldarg.
	/// </summary>
	public class CIL_ldarg : CILInstruction
	{
		int argIndex;

		public CIL_ldarg(string label, int argIndex) : base (label)
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
			return label + ":  ldarg      " + argIndex;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return threadState.GetLocalArgument(argIndex).IsConcrete;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// TODONOW: Fix this when semantic for ldarg and starg is clear
			if(CanExecute(threadState) == false)
				throw new Exception("This instruction is not ready to execute");
			VMValue source = threadState.GetLocalArgument(argIndex);
			VMValue target = threadState.SystemState.Values.MakeValue(source);
			target.IsThreadLocal = true;
			DelayedRead dr = new DelayedRead(target.GUID, source.GUID, this);
			dr.SourceInstruction = this;
			threadState.ThreadStack.Push(target.GUID);			
			threadState.AddPendingAction(dr);
			return threadState.CurrentMethod.GetNextInstruction(this);			
		}


	}
}
