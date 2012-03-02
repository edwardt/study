using System;

namespace mmchecker.vm.action
{
	/// <summary>
	/// Summary description for DelayedVolatileRead.
	/// </summary>
	public class DelayedVolatileRead : DelayedRead
	{
		public DelayedVolatileRead(int destValue, int srcValue, CILInstruction sourceInstruction) : base(destValue, srcValue, sourceInstruction)
		{
		}

		public override DelayedAction Duplicate()
		{
			return new DelayedVolatileRead(destValue, srcValue, sourceInstruction);
		}

		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.VREAD;
		}

		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 6 to indicate that this is a DelayedVolatileRead
			ss.WriteInt(6);
			ss.WriteGuid(srcValue);
			ss.WriteGuid(destValue);
		}
	}
}
