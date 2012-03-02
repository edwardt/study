using System;

namespace mmchecker
{
	/// <summary>
	/// Duplicates the top value on the stack
	/// ..., v -> ..., v, v
	/// </summary>
	public class CIL_dup : CILInstruction
	{
		public CIL_dup(string label) : base(label)
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
			return label + ":  dup";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			// we can always duplicate the guid on the stack, regardless whether the
			// value is concrete or dependent on anything
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// just duplicate the top guid on the stack, if the value is not concrete yet 
			// it will be concrete later by the operation on the "original" guid (same guid)
			threadState.ThreadStack.Push(threadState.ThreadStack[threadState.ThreadStack.Count - 1]);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}


	}
}
