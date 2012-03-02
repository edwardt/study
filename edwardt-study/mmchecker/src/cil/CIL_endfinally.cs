using System;

namespace mmchecker
{
	/// <summary>
	/// CIL instructions for exception handling
	/// In this version this is ignored by the virtual machine, with 
	/// strict assumption about location of these blocks, refer to Parser's code
	/// </summary>
	public class CIL_endfinally : CILInstruction
	{
		public CIL_endfinally(string label) : base(label)
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
			return label + ":  endfinally";
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			// just ignore and increase pc
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

	}
}
