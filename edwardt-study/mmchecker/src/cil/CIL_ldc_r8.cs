using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary> ldc.r8
	/// ... -> ..., value
	/// </summary>
	public class CIL_ldc_r8 : CILInstruction
	{
		double value;

		public CIL_ldc_r8(string label, double value) : base (label)
		{
			this.value = value;	
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
			return label + ":  "
				+ "ldc.r8     " + value;
		}

		/// <summary>
		/// Push a 4-byte-int onto the stack
		/// </summary>
		/// <param name="threadState"></param>
		/// <returns></returns>
		public override CILInstruction Execute(ThreadState threadState)
		{
			VMValue_double theConstant = (VMValue_double)threadState.SystemState.Values.MakeValue(new CILVar_double(""));
			theConstant.Value = value;
			theConstant.IsThreadLocal = true;
			theConstant.IsConcrete = true;
			
			threadState.ThreadStack.Push(theConstant.GUID);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		public override bool CanExecute(ThreadState threadState)
		{
			// can always load a constant onto the stack
			return true;
		}
	}
}
