using System;

namespace mmchecker.vm.action
{
	/// <summary>
	/// DummyDelayedAction for flow graph algorithm (TODO: explain more)
	/// It is not usable in any saving/duplicating operation
	/// </summary>
	public class DummyDelayedAction : DelayedAction
	{
		public DummyDelayedAction(CILInstruction sourceInstruction)
		{
			this.sourceInstruction = sourceInstruction;	
		}

		public override bool CanComplete(ThreadState threadState)
		{
			throw new Exception("Should not be called");
		}
		
		public override bool DoesConflict(DelayedAction da)
		{
			throw new Exception("Should not be called");
		}

		public override void Complete(ThreadState threadState)
		{
			throw new Exception("Should not be called");
		}

		public override void TakeSnapshot(StateSnapshot ss)
		{
			throw new Exception("Should not be called");
		}

		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			throw new Exception("Should not be called");
		}
		
		public override DelayedAction Duplicate()
		{
			return new DummyDelayedAction(sourceInstruction);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            // donothing, this is a dummy delayed action
        }

        public override bool Escape(VMValueManager valueMan)
        {
            // TODO: check this, it may be false which makes analysis running faster
            return true;
        }
    }
}
