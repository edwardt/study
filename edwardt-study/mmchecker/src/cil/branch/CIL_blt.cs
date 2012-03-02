using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Unconditional branch
	/// </summary>
	public class CIL_blt : CILInstruction
	{
		string branchLabel;

		public CIL_blt(string label, string branchLabel) : base(label)
		{
			this.branchLabel = branchLabel;
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
			return label + ":  blt        " + branchLabel;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This blt instruction is not ready to execute");
			VMValue_int32 value2 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			if(value1.Value < value2.Value)
				return threadState.CurrentMethod.GetInstruction(branchLabel);
			else
				return threadState.CurrentMethod.GetNextInstruction(this);			
		}


		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
			return (v1.IsConcrete && v2.IsConcrete);
		}

	}
}
