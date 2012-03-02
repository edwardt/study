using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Branch on greater than or equal
	/// ..., value1, value2 -> ...
	/// Branch to label if value1 >= value2
	/// </summary>
	public class CIL_bge : CILInstruction
	{
		string branchLabel;

		public CIL_bge(string label, string branchLabel) : base(label)
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
			return label + ":  bge        " + branchLabel;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This blt instruction is not ready to execute");
			if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_int32)
			{
				VMValue_int32 value2 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
				if(value1.Value >= value2.Value)
					return threadState.CurrentMethod.GetInstruction(branchLabel);
				else
					return threadState.CurrentMethod.GetNextInstruction(this);			
			}
			else if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_double)
			{
				VMValue_double value2 = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_double value1 = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
				if(value1.Value >= value2.Value)
					return threadState.CurrentMethod.GetInstruction(branchLabel);
				else
					return threadState.CurrentMethod.GetNextInstruction(this);			
			}
			else
				 throw new Exception("Unknown data type for bge instruction");
		}


		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
			return (v1.IsConcrete && v2.IsConcrete);
		}

	}
}
