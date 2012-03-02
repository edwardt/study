using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Load an element of the array to the stack
	/// ..., array, index -> ..., data
	/// </summary>
	public class CIL_ldelem : CILInstruction
	{
		public CIL_ldelem(string label) : base(label)
		{
			// do nothing
		}

		public override bool IsThreadLocal()
		{
			return true;
		}

		public override Semantic GetSemantic()
		{
			return Semantic.READ;
		}

		public override string ToString()
		{
			return label + ":  ldelem";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
			return (v1.IsConcrete && v2.IsConcrete);			
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This ldelem is not ready to execute");
            int index = ((VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop())).Value;
            VMValue_array arr = (VMValue_array)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_arrayinst arrinst = (VMValue_arrayinst)threadState.GetValue(arr.ArrayInstGuid);			
			
			VMValue destValue = threadState.SystemState.Values.MakeValue(threadState.GetValue(arrinst.GetElement(index)));
            destValue.IsThreadLocal = true;
			threadState.ThreadStack.Push(destValue.GUID);

			DelayedRead dr = new DelayedRead(destValue.GUID, arrinst.GetElement(index), this);
			threadState.AddPendingAction(dr);

			return threadState.CurrentMethod.GetNextInstruction(this);
		}
	}
}
