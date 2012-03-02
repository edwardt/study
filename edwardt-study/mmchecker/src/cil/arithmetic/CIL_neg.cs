using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Negate the value on the stack, can be integer or floating point
	/// ..., value -> ..., (-value)
	/// </summary>
	public class CIL_neg : CILInstruction
	{
		public CIL_neg(string label) : base(label)
		{
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
			return label + ":  neg";
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_int32)
			{
				VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int32 neg = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
				neg.Value = -value1.Value;
				neg.IsThreadLocal = true;
				neg.IsConcrete = true;
				threadState.ThreadStack.Push(neg.GUID);
			}
			else if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_double)
			{
				VMValue_double value1 = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_double neg = (VMValue_double)threadState.SystemState.Values.MakeValue(new CILVar_double(""));
				neg.Value = -value1.Value;
				neg.IsThreadLocal = true;
				neg.IsConcrete = true;
				threadState.ThreadStack.Push(neg.GUID);
			}
			else if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_int64)
			{
				VMValue_int64 value1 = (VMValue_int64)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int64 neg = (VMValue_int64)threadState.SystemState.Values.MakeValue(new CILVar_int64(""));
				neg.Value = -value1.Value;
				neg.IsThreadLocal = true;
				neg.IsConcrete = true;
				threadState.ThreadStack.Push(neg.GUID);
			}
			else
				throw new Exception("Unknown data type on stack for add");
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return (v1.IsConcrete);
		}
	}
}
