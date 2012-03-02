using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_nop.
	/// </summary>
	public class CIL_nop : CILInstruction
	{
		public CIL_nop(string label) : base(label)
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
			return label + ":  nop";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

	}
}
