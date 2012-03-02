using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_ldloc.
	/// </summary>
	public class CIL_ldloc : CILInstruction
	{
		int localVarIndex;

		public CIL_ldloc(string label, int localVarIndex) : base(label)
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
			return label + ":  ldloc.     " + localVarIndex;
		}

		/// <summary>
		/// Execute a ldloc
		/// Initiate a DelayedRead and schedule it to the thread's queue
		/// </summary>
		/// <param name="threadState"></param>
		/// <returns></returns>
		public override CILInstruction Execute(ThreadState threadState)
		{
			VMValue source = threadState.GetLocalVariable(localVarIndex);
			VMValue target = threadState.SystemState.Values.MakeValue(source);
			target.IsThreadLocal = true;
			DelayedRead dr = new DelayedRead(target.GUID, source.GUID, this);
			dr.SourceInstruction = this;
			threadState.ThreadStack.Push(target.GUID);
			threadState.AddPendingAction(dr);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}
	}
}
