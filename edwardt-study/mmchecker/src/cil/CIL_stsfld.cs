using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_stfld.
	/// </summary>
	public class CIL_stsfld : CILInstruction
	{
		CILClass classType;
		CILVariable field;

		public CIL_stsfld(string label, CILClass classType, CILVariable field) : base(label)
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
			return true;
		}

		public override Semantic GetSemantic()
		{
			return Semantic.WRITE;
		}

		public override string ToString()
		{
			return label + ":  stsfld     " + classType.Name + " " + field.Name;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
			// NOTE: This is different than stfld and ldfld in which we already knew where
			// the write will write to, so it is possible to start this instruction anytime
			// even when the stack has only the inconcrete value
			// in ldfld and stfld we need to wait until the pointer on the stack is concrete 
			// to ensure local consistency in case two stfld writes to the same location
			return true;
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This stfld is not ready to execute");

			VMValue source = threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue target = threadState.GetValue(threadState.SystemState.GetStaticVariable(classType, field));
			DelayedWrite dw = new DelayedWrite(target.GUID, source.GUID, this);
			dw.SourceInstruction = this;
			//			target.AddDelayedAction(dw);
			threadState.AddPendingAction(dw);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}


	}
}
