using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_stloc.
	/// </summary>
	public class CIL_stloc : CILInstruction
	{
		int localVarIndex;

		public CIL_stloc(string label, int localVarIndex) : base(label)
		{
			this.localVarIndex = localVarIndex;
		}

		public int LocalVarIndex
		{
			get {return localVarIndex; }
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
			return label + ":  stloc.     " + localVarIndex;
		}

		/// <summary>
		/// Execute a stloc
		/// It will initiate a write and schedule it to the thread's queue
		/// </summary>
		/// <param name="threadState"></param>
		/// <returns></returns>
		public override CILInstruction Execute(ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("Can't execute this stloc now");

			VMValue source = threadState.GetValue((int)threadState.ThreadStack.Pop());
			VMValue target = threadState.GetLocalVariable(localVarIndex);
			DelayedWrite dw = new DelayedWrite(target.GUID, source.GUID, this);
			dw.SourceInstruction = this;
			threadState.AddPendingAction(dw);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

		/// <summary>
		/// stloc can always be executed, even if the value on the stack
		/// is not concrete yet
		/// </summary>
		/// <param name="threadState"></param>
		/// <returns></returns>
		public override bool CanExecute(ThreadState threadState)
		{
			return true;
		}
	}
}
