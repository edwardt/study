using System;
using mmchecker.util;

namespace mmchecker.vm.action
{
	/// <summary>
	/// The delayed action created by Monitor.Enter()
	/// Its issuing stage is thread local
	/// In completion stage, it does a write on the object's lock flag
	/// </summary>
	public class DelayedLock : DelayedAction
	{
		internal int obj;
		int nTimes;

		// The completion of wait requires acquiring 
		// the lock the number of times equal to before completing it
		public DelayedLock(int obj, CILInstruction sourceInstruction, int nTimes)
		{
			this.sourceInstruction = sourceInstruction;
			this.obj = obj;
			this.nTimes = nTimes;
		}

		// For normal lock, we lock only once
		public DelayedLock(int obj, CILInstruction sourceInstruction)
		{
			this.sourceInstruction = sourceInstruction;
			this.obj = obj;
			this.nTimes = 1;	
		}

		// Duplicate the DelayedLock, called when duplicating the state
		public override DelayedAction Duplicate()
		{
			return new DelayedLock(obj, sourceInstruction, nTimes);
		}

		// Return the semantic of this lock, which is Semantic.LOCK
		public override mmchecker.CILInstruction.Semantic GetSemantic()
		{
			return CILInstruction.Semantic.LOCK;
		}

		// Dump the data structure to the snapshot
		public override void TakeSnapshot(StateSnapshot ss)
		{
			// write identifier 2 to indicate that this is a DelayedLock
			ss.WriteInt(2);
			ss.WriteGuid(obj);
			ss.WriteInt(nTimes);
		}

		// Checks data dependency with other delayed actions
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

		// A lock action can be completed if the lock is free
		// or it is currently hold by the same thread
		public override bool CanComplete(ThreadState threadState)
		{
			// A DelayedLock can only be completed if the object instance is not locked
			// or it is already locked by this thread
			VMValue_objectinst vinst = (VMValue_objectinst)threadState.GetValue(obj);
			if((vinst.HoldingLockThreadID == threadState.ThreadID) ||
				(vinst.HoldingLockThreadID == -1))
				return true;
			else
				return false;					
		}

		// In completion stage, the lock action will actually 
		// set the object's lock field to be locked by this thread
		public override void Complete(ThreadState threadState)
		{
			VMValue_objectinst vinst = (VMValue_objectinst)threadState.GetValue(obj);
			if(vinst.HoldingLockThreadID == -1)
			{
				vinst.HoldingLockThreadID = threadState.ThreadID;
				vinst.HoldingLockCount = nTimes;
			}
			else
				vinst.HoldingLockCount += nTimes;
			if(CheckerConfiguration.DoPrintExecution)
			{
				Console.WriteLine("Commiting Lock in thread " + threadState.CurrentMethod.Name + " on " + obj);
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
