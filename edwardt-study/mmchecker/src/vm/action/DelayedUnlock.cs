using System;
using mmchecker.util;

namespace mmchecker.vm.action
{
	/// <summary>
	/// Summary description for DelayedLock.
	/// </summary>
	public class DelayedUnlock : DelayedAction
	{
		internal int obj;

		public DelayedUnlock(int obj, CILInstruction sourceInstruction)
		{
			this.sourceInstruction = sourceInstruction;
			this.obj = obj;
		}

		public override DelayedAction Duplicate()
		{
			return new DelayedUnlock(obj, sourceInstruction);
		}

		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.UNLOCK;
		}

		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 3 to indicate that this is a DelayedUnlock
			ss.WriteInt(3);
			ss.WriteGuid(obj);
		}


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
			return false;
		}

		public override bool CanComplete(ThreadState threadState)
		{
			// A DelayedUnlock can be completed even if the thread does not
			// own the lock, then the execution does not affect anything
			// except that the instruction acts as a barrier preventing
			// instruction before to be completed after the unlock
			return true;
		}

		public override void Complete(ThreadState threadState)
		{
			VMValue_objectinst vinst = (VMValue_objectinst)threadState.GetValue(obj);
			if(vinst.HoldingLockThreadID == threadState.ThreadID)
			{
				vinst.HoldingLockCount--;
				if(vinst.HoldingLockCount == 0)
					vinst.HoldingLockThreadID = -1;
			}
			else
			{
				// If we unlock a lock that 
				// this thread does not possess, the instruction
				// executes normally but has no effect
			}
			if(CheckerConfiguration.DoPrintExecution)
			{
				Console.WriteLine("Commiting Unlock in thread " + threadState.CurrentMethod.Name + " on " + obj);
			}
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