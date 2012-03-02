using System;
using mmchecker.vm;
using mmchecker.vm.action;

namespace mmchecker
{
	/// <summary>
	/// Summary description for CIL_ldfld.
	/// </summary>
	public class CIL_ldfld : CILInstruction
	{
		string fieldType;
		string className;
		string fieldName;
		bool isVolatile;

		public CIL_ldfld(string label, string fieldType, string className, string fieldName, bool isVolatile) : base(label)
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
			return Semantic.READ;
		}

		public override string ToString()
		{
			return label + ":  ldfld      " + fieldType + " " + className + "::" + fieldName;
		}

		public override bool CanExecute(mmchecker.vm.ThreadState threadState)
		{
            VMValue v1 = threadState.GetValue(threadState.ThreadStack.Peek());
			return (v1.IsConcrete);
		}

		public override CILInstruction Execute(mmchecker.vm.ThreadState threadState)
		{
			if(CanExecute(threadState) == false)
				throw new Exception("ldfld is not ready to execute now");
			VMValue_object objptr = (VMValue_object)threadState.GetValue(threadState.ThreadStack.Pop());
			VMValue_objectinst obj = (VMValue_objectinst)threadState.GetValue(objptr.ValueGUID);
			int src = obj.GetFieldGUID(className, fieldName);
			int dest = threadState.SystemState.Values.MakeValue(threadState.GetValue(src)).GUID;
            threadState.GetValue(dest).IsThreadLocal = true;            
			DelayedRead dr;
			if(isVolatile)
				dr = new DelayedVolatileRead(dest, src, this);
			else
				dr = new DelayedRead(dest, src, this);
			dr.SourceInstruction = this;
			threadState.ThreadStack.Push(dest);
			threadState.AddPendingAction(dr);
			return threadState.CurrentMethod.GetNextInstruction(this);

		}

	}
}
