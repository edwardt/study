using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_newarr.
	/// </summary>
	public class CIL_newarr : CILInstruction
	{
		CILVariable elementType;

		public CIL_newarr(string label, CILVariable elementType) : base(label)
		{
			this.elementType = elementType;
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
			return label + ":  newarr     " + elementType.ToString() + "[]";
		}

		public override bool CanExecute(ThreadState threadState)
		{
			// TODO: Can be more relaxed but not important now, check semantic later
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return v1.IsConcrete;
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This add instruction is not ready to execute");
			VMValue_int32 value1 = (VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_arrayinst arrayInst = threadState.SystemState.Values.MakeArrayInst(value1.Value, elementType);
			VMValue_array arr = threadState.SystemState.Values.MakeArray();
			arr.ArrayInstGuid = arrayInst.GUID;
			arr.IsConcrete = true;
			arr.IsThreadLocal = true;
			threadState.ThreadStack.Push(arr.GUID);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

	}
}
