using System;

namespace mmchecker.vm.action
{
	/// <summary>
	/// Summary description for DelayedVolatileRead.
	/// </summary>
	public class DelayedVolatileWrite : DelayedWrite
	{
		public DelayedVolatileWrite(int destValue, int srcValue, CILInstruction sourceInstruction) : base(destValue, srcValue, sourceInstruction)
		{
		}

		public override DelayedAction Duplicate()
		{
			return new DelayedVolatileWrite(destValue, srcValue, sourceInstruction);
		}

		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.VWRITE;
		}

		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 7 to indicate that this is a DelayedVolatileWrite
			ss.WriteInt(7);
			ss.WriteGuid(srcValue);
			ss.WriteGuid(destValue);
		}


	}
}
