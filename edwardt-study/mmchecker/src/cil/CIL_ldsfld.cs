using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_ldfld.
	/// </summary>
	public class CIL_ldsfld : CILInstruction
	{
		CILClass classType;
		CILVariable field;

		public CIL_ldsfld(string label, CILClass classType, CILVariable field) : base(label)
		{
			this.classType = classType;
			this.field = field;
		}

		public CILClass ClassType
		{
			get { return classType; }
		}

		public CILVariable Field
		{
			get { return field;	}
		}

		public override bool IsThreadLocal()
		{
			return false;
		}

		public override Semantic GetSemantic()
		{
			return Semantic.READ;
		}

		public override string ToString()
		{
			return label + ":  ldsfld     " + classType.Name + " " + field.Name;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			int src = threadState.SystemState.GetStaticVariable(classType, field);
			int dest = threadState.SystemState.Values.MakeValue(threadState.GetValue(src)).GUID;
			DelayedRead dr = new DelayedRead(dest, src, this);
			dr.SourceInstruction = this;
			threadState.ThreadStack.Push(dest);
			threadState.AddPendingAction(dr);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}

	}
}
