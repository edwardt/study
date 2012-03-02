using System;
using mmchecker.vm;

namespace mmchecker
{
	/// <summary> ldnull
	/// ... -> ..., null
	/// </summary>
	public class CIL_ldnull : CILInstruction
	{
		public CIL_ldnull(string label) : base (label)
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
			return label + ":  ldnull";				
		}

		/// <summary>
		/// Push a null pointer value onto the stack
		/// </summary>
		/// <param name="threadState"></param>
		/// <returns></returns>
		public override CILInstruction Execute(ThreadState threadState)
		{
			VMValue_object v = (VMValue_object)threadState.SystemState.Values.MakeNullValue();
			v.IsThreadLocal = true;
			v.IsConcrete = true;
			
			threadState.ThreadStack.Push(v.GUID);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		public override bool CanExecute(ThreadState threadState)
		{
			// can always load a constant onto the stack
			return true;
		}
	}
}
