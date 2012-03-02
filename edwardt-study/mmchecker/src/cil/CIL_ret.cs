using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_ret.
	/// </summary>
	public class CIL_ret : CILInstruction
	{
		public CIL_ret(string label) : base(label)
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
			return label + ":  ret";
		}

		public override CILInstruction Execute(ThreadState threadState)
		{
			return threadState.MethodReturn();
		}	
	}
}
