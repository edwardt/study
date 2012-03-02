using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// brtrue
	/// Branch on non-false or non-null
	/// </summary>
	public class CIL_brfalse : CILInstruction
	{
		string branchLabel;

		public CIL_brfalse(string label, string branchLabel) : base(label)
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
			return label + ":  brfalse     " + branchLabel;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// TODO: must support other types of data beside int32
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			if(value1.Value == 0)
				return threadState.CurrentMethod.GetInstruction(branchLabel);
			else
				return threadState.CurrentMethod.GetNextInstruction(this);			
		}

		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return v1.IsConcrete;
		}


	}
}
