using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// OR two integer
	/// ..., value1, value2 -> ..., (value1 | value2)
	/// </summary>
	public class CIL_or : CILInstruction
	{
		public CIL_or(string label) : base(label)
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
			return label + ":  or";
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_int32 value2 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_int32 sum = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
			sum.Value = value1.Value | value2.Value;
			sum.IsThreadLocal = true;
			sum.IsConcrete = true;
			threadState.ThreadStack.Push(sum.GUID);
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
