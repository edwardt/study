using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Load the length of the array to the stack
	/// ..., array -> ..., length
	/// </summary>
	public class CIL_ldlen : CILInstruction
	{
		public CIL_ldlen(string label) : base(label)
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
			return label + ":  ldlen";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return (v1.IsConcrete);			
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This ldelem is not ready to execute");
            VMValue_array arr = (VMValue_array)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_arrayinst arrinst = (VMValue_arrayinst)threadState.GetValue(arr.ArrayInstGuid);			
			
			VMValue_int32 lengthValue = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
			lengthValue.IsConcrete = true;
			lengthValue.IsThreadLocal = true;
			lengthValue.Value = arrinst.GetLength();
			threadState.ThreadStack.Push(lengthValue.GUID);

			return threadState.CurrentMethod.GetNextInstruction(this);
		}
	}
}
