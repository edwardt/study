using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Store a data to an element of the array
	/// ..., array, index, data -> ...
	/// </summary>
	public class CIL_stelem : CILInstruction
	{
		public CIL_stelem(string label) : base(label)
		{
			// do nothing
		}

		public override bool IsThreadLocal()
		{
			return true;
		}

		public override Semantic GetSemantic()
		{
			return Semantic.WRITE;
		}

		public override string ToString()
		{
			return label + ":  stelem";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
            //VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
            VMValue v3 = threadState.GetValue(threadState.ThreadStack.Peek(2));
			// NOTE: No need for v1 to be concrete, data dependency
			// will stop the DelayedWriteArray to complete
			return (v2.IsConcrete && v3.IsConcrete);			
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This stelem is not ready to execute");
            int gValue = threadState.ThreadStack.Pop();
			int index = ((VMValue_int32)threadState.GetValue(threadState.ThreadStack.Pop())).Value;
			VMValue_array arr = (VMValue_array)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_arrayinst arrinst = (VMValue_arrayinst)threadState.GetValue(arr.ArrayInstGuid);
			DelayedWrite dw = new DelayedWrite(arrinst.GetElement(index), gValue, this);
			threadState.AddPendingAction(dw);

			return threadState.CurrentMethod.GetNextInstruction(this);
		}


	}
}
