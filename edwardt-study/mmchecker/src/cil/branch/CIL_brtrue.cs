using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// brtrue
	/// Branch on non-false or non-null
	/// </summary>
	public class CIL_brtrue : CILInstruction
	{
		string branchLabel;

		public CIL_brtrue(string label, string branchLabel) : base(label)
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
			return label + ":  brtrue     " + branchLabel;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// TODO: must support other types of data beside int32
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			VMValue v = threadState.GetValue(threadState.ThreadStack.Pop());
			if(v is VMValue_int32)
			{
				VMValue_int32 value1 = (VMValue_int32)v;
				if(value1.Value != 0)
					return threadState.CurrentMethod.GetInstruction(branchLabel);
				else
					return threadState.CurrentMethod.GetNextInstruction(this);			
			}
			else if(v is VMValue_object)
			{
				VMValue_object value1 = (VMValue_object)v;
				if(value1.ValueGUID == -1)
					return threadState.CurrentMethod.GetNextInstruction(this);
				else
					return threadState.CurrentMethod.GetInstruction(branchLabel);					
			}
			else if(v is VMValue_double)
			{
				VMValue_double value1 = (VMValue_double)v;
				if(value1.Value != 0)
					return threadState.CurrentMethod.GetInstruction(branchLabel);
				else
					return threadState.CurrentMethod.GetNextInstruction(this);			
			}
				throw new Exception("Unknown data type for brtrue");
		}

		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return v1.IsConcrete;
		}


	}
}
