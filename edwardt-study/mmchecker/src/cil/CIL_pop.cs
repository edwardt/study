using System;

namespace mmchecker
{
	/// <summary>
	/// Removes the top element of the stack
	/// </summary>
	public class CIL_pop : CILInstruction
	{
		public CIL_pop(string label) : base(label)
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
			return label + ":  pop";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// just need to remove and discard the top element of the stack
			threadState.ThreadStack.Pop();
			return threadState.CurrentMethod.GetNextInstruction(this);
		}


	}
}
