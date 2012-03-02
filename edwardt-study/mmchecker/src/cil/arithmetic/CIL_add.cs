using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_dup.
	/// </summary>
	public class CIL_add : CILInstruction
	{
		public CIL_add(string label) : base(label)
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
			return label + ":  add";
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_int32)
			{
				VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int32 value2 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int32 sum = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
				sum.Value = value1.Value + value2.Value;
				sum.IsThreadLocal = true;
				sum.IsConcrete = true;
				threadState.ThreadStack.Push(sum.GUID);
			}
			else if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_double)
			{
				VMValue_double value1 = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_double value2 = (VMValue_double)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_double sum = (VMValue_double)threadState.SystemState.Values.MakeValue(new CILVar_double(""));
				sum.Value = value1.Value + value2.Value;
				sum.IsThreadLocal = true;
				sum.IsConcrete = true;
				threadState.ThreadStack.Push(sum.GUID);
			}
			else if(threadState.GetValue(threadState.ThreadStack.Peek()) is VMValue_int64)
			{
				VMValue_int64 value1 = (VMValue_int64)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int64 value2 = (VMValue_int64)threadState.GetValue(threadState.ThreadStack.Pop());
				VMValue_int64 sum = (VMValue_int64)threadState.SystemState.Values.MakeValue(new CILVar_int64(""));
				sum.Value = value1.Value + value2.Value;
				sum.IsThreadLocal = true;
				sum.IsConcrete = true;
				threadState.ThreadStack.Push(sum.GUID);
			}else
				throw new Exception("Unknown data type on stack for add");

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
