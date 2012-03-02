using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Convert an int64 to int32 
	/// ..., value -> ..., value (int64)
	/// </summary>
	public class CIL_conv_i4 : CILInstruction
	{
		public CIL_conv_i4(string label) : base(label)
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
			return label + ":  conv.i4";
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			VMValue_int64 value1 = (VMValue_int64)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_int32 newvalue = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
			newvalue.Value = (int)value1.Value;
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
