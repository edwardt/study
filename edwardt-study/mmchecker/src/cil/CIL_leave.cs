using System;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_leave.
	/// </summary>
	public class CIL_leave : CILInstruction
	{
		string branchLabel;

		public CIL_leave(string label, string branchLabel) : base(label)
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
			return label + ":  leave     " + branchLabel;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// TODONOW: implement this correctly, now we just ignore 
			// and assumes that final block follows this
//			return threadState.CurrentMethod.GetInstruction(branchLabel);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

	}
}
