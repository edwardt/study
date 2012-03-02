using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Branch on less than or equal
	/// ..., value1, value2 -> ...
	/// Branch to label if value1 <= value2
	/// </summary>
	public class CIL_ble : CILInstruction
	{
		string branchLabel;

		public CIL_ble(string label, string branchLabel) : base(label)
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
			return label + ":  ble        " + branchLabel;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This blt instruction is not ready to execute");
			VMValue v2 = threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue v1 = threadState.GetValue(threadState.ThreadStack.Pop());

			if((v1 is VMValue_int32) && (v2 is VMValue_int32))
			{
				VMValue_int32 vi1, vi2;
				vi1 = (VMValue_int32)v1;
				vi2 = (VMValue_int32)v2;
				if(vi1.Value <= vi2.Value)
					return threadState.CurrentMethod.GetInstruction(branchLabel);
				else
					return threadState.CurrentMethod.GetNextInstruction(this);			
			}
			else
			{
				double d1, d2;
				if(v1 is VMValue_int32)
					d1 = ((VMValue_int32)v1).Value;
				else if(v1 is VMValue_double)
					d1 = ((VMValue_double)v1).Value;
				else
					throw new Exception("Unknown data type for bne");

				if(v2 is VMValue_int32)
					d2 = ((VMValue_int32)v2).Value;
				else if(v2 is VMValue_double)
					d2 = ((VMValue_double)v2).Value;
				else
					throw new Exception("Unknown data type for bne");

				if(d1 <= d2)
					return threadState.CurrentMethod.GetInstruction(branchLabel);
				else
					return threadState.CurrentMethod.GetNextInstruction(this);			
			}
		}


		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
			return (v1.IsConcrete && v2.IsConcrete);
		}

	}
}
