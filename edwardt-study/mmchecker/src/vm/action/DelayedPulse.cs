using System;
using mmchecker.util;

namespace mmchecker.vm.action
{
	/// <summary>
	/// The delayed action created by Monitor.Pulse()
	/// Its issuing stage is thread local and do nothing
	/// In completion stage
	///   if it is a normal pulse
	///   . Generate a transition for each thread waiting on the lock
	///     the chosen thread state is set to ACTIVE (in ecma it is 
	///     set to PULSED, in this implementation we only have ACTIVE
	///     and WAIT for simplicity)
	///   if it is a PulseAll
	///   . Generate one transition that set all thread waiting on 
	///     the lock to be ACTIVE
	/// </summary>
	public class DelayedPulse : DelayedAction
	{
		internal int obj;
		internal bool isPulseAll;

		public DelayedPulse(int obj, CILInstruction sourceInstruction, bool isPulseAll)
		{
			this.sourceInstruction = sourceInstruction;
			this.obj = obj;
			this.isPulseAll = isPulseAll;
		}

		// Duplicate the DelayedWait, called when duplicating the state
		public override DelayedAction Duplicate()
		{
			return new DelayedPulse(obj, sourceInstruction, isPulseAll);
		}

		// Return the semantic of this pulse, which is Semantic.PULSE
		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.PULSE;
		}

		// Dump the data structure to the snapshot
		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 9 to indicate that this is a DelayedPulse
			if(isPulseAll)
			{
				ss.WriteInt(10);
				ss.WriteGuid(obj);
			}
			else
			{
				ss.WriteInt(9);
				ss.WriteGuid(obj);
			}
		}

		// Checks data dependency with other delayed actions
		// because this is a synchronization action, it should 
		// conflict with all other synchronization action else
		// we can produce deadlock
		public override bool DoesConflict(DelayedAction da)
		{
			if(da is DelayedLock)
			{
				DelayedLock dl = (DelayedLock)da;
				if(dl.obj == this.obj)
					return true;
			}
			else if(da is DelayedUnlock)
			{
				DelayedUnlock du = (DelayedUnlock)da;
				if(du.obj == this.obj)
					return true;
			}
			else if(da is DelayedWait)
			{
				if(((DelayedWait)da).obj == this.obj)
					return true;
			}
			else if(da is DelayedPulse)
			{
				if(((DelayedPulse)da).obj == this.obj)
					return true;
			}
			return false;
		}

		public override bool CanComplete(ThreadState threadState)
		{
			// TODO: (much later) support exception here
			VMValue_objectinst vinst = (VMValue_objectinst)threadState.GetValue(obj);
			if(vinst.HoldingLockThreadID != threadState.ThreadID)
				throw new Exception("Calling pulse on a lock we don't possess");

			return true;				
		}

		public override void Complete(ThreadState threadState)
		{
			// we need to activate one of the threads
			// the work is done in ThreadState class
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            valueMan.GetValue(obj).MarkAndSweep(tid, valueMan);
        }

        public override bool Escape(VMValueManager valueMan)
        {
            return valueMan.GetValue(obj).Escape;
        }
    }
}
