using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Convert an int32 to int64 
	/// ..., value -> ..., value (int64)
	/// </summary>
	public class CIL_conv_i8 : CILInstruction
	{
		public CIL_conv_i8(string label) : base(label)
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
			return label + ":  conv.i8";
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_int64 newvalue = (VMValue_int64)threadState.SystemState.Values.MakeValue(new CILVar_int64(""));
			newvalue.Value = (long)value1.Value;
			newvalue.IsThreadLocal = true;
			newvalue.IsConcrete = true;
			threadState.ThreadStack.Push(newvalue.GUID);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		public override bool CanExecute(ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return v1.IsConcrete;
		}
	}
}
