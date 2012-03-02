using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_stfld.
	/// </summary>
	public class CIL_stfld : CILInstruction
	{
		string fieldType;
		string className;
		string fieldName;
		bool isVolatile;

		public CIL_stfld(string label, string fieldType, string className, string fieldName, bool isVolatile) : base(label)
		{
			this.fieldType = fieldType;
			this.className = className;
			this.fieldName = fieldName;
			this.isVolatile = isVolatile;
		}

		public string FieldType
		{
			get { return fieldType; }
		}

		public string ClassName
		{
			get { return className;	}
		}

		public string FieldName
		{
			get	{ return fieldName; }
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
			return label + ":  stfld      " + fieldType + " " + className + "::" + fieldName;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
            VMValue v2 = threadState.GetValue(threadState.ThreadStack.Peek(1));
			return (v1.IsConcrete && v2.IsConcrete);			
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("This stfld is not ready to execute");

			VMValue source = threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_object objptr = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_objectinst obj = (VMValue_objectinst)threadState.GetValue(objptr.ValueGUID);
			VMValue target = threadState.GetValue(obj.GetFieldGUID(className, fieldName));
			DelayedWrite dw;
			if(isVolatile)
				dw = new DelayedVolatileWrite(target.GUID, source.GUID, this);
			else
				dw = new DelayedWrite(target.GUID, source.GUID, this);
			dw.SourceInstruction = this;
//			target.AddDelayedAction(dw);
			threadState.AddPendingAction(dw);
			return threadState.CurrentMethod.GetNextInstruction(this);
		}
	}
}
