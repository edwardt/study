using System;

namespace mmchecker
{
	/// <summary>
	/// Unconditional branch
	/// </summary>
	public class CIL_br : CILInstruction
	{
		string branchLabel;

		public CIL_br(string label, string branchLabel) : base(label)
		{
			this.branchLabel = branchLabel;
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
			return label + ":  br         " + branchLabel;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			return threadState.CurrentMethod.GetInstruction(branchLabel);
		}


	}
}
