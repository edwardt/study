using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_ldftn.
	/// </summary>
	public class CIL_ldftn : CILInstruction
	{
		CILMethod theMethod;

		public CIL_ldftn(string label, CILMethod theMethod) : base(label)
		{
			this.theMethod = theMethod;
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
			return label + ":  ldftn      " + theMethod.ToString();
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{			
			VMValue_ftn ftn = threadState.SystemState.Values.MakeFtnValue(theMethod);
			ftn.IsConcrete = true;
			threadState.ThreadStack.Push(ftn.GUID);

			return threadState.CurrentMethod.GetNextInstruction(this);
		}


	}
}
