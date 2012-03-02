using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary> ldc.i4.*
	/// ... -> ..., value
	/// </summary>
	public class CIL_ldc_i4 : CILInstruction
	{
		int value;

		public CIL_ldc_i4(string label, int value) : base (label)
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
				+ "ldc.i4     " + value;
		}

		/// <summary>
		/// Push a 4-byte-int onto the stack
		/// </summary>
		/// <param name="threadState"></param>
		/// <returns></returns>
		public override CILInstruction Execute(ThreadState threadState)
		{
			VMValue_int32 theConstant = (VMValue_int32)threadState.SystemState.Values.MakeValue(new CILVar_int32(""));
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
