using System;
using mmchecker.util;

namespace mmchecker.vm.action
{
	/// <summary>
	/// The delayed action created by Monitor.Wait()
	/// Its issuing stage is thread local
	/// In completion stage
	///   . Set thread state to ThreadState.Status.WAIT
	///   . Unlock the threads
	///   . Change the DelayedAction into DelayedLock with same number 
	///		of lock times. The replacement and creation is done by
	///		class ThreadState
	/// </summary>
	public class DelayedWait : DelayedAction
	{
		internal int obj;
		internal int lockCount;

		public DelayedWait(int obj, CILInstruction sourceInstruction, int lockCount)
		{
			this.sourceInstruction = sourceInstruction;
			this.obj = obj;															
			this.lockCount = lockCount;
		}

		// Duplicate the DelayedWait, called when duplicating the state
		public override DelayedAction Duplicate()
		{
			return new DelayedWait(obj, sourceInstruction, lockCount);
		}

		// Return the semantic of this lock, which is Semantic.LOCK
		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.WAIT;
		}

		// Dump the data structure to the snapshot
		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 8 to indicate that this is a DelayedWait
			ss.WriteInt(8);
			ss.WriteInt(lockCount);
			ss.WriteGuid(obj);
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
			return threadState.syncState == ThreadState.SyncState.RUNNING;				
		}

		public override void Complete(ThreadState threadState)
		{
			// we need to change this DelayedWait into DelayedLock but this
			// can only be done in ThreadState code
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
