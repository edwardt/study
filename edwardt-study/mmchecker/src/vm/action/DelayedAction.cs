using System;

namespace mmchecker.vm.action
{
	/// <summary>
	/// Abstract class for a delayed action
	/// Currently it can be an uncompleted read or an uncompleted write
	/// </summary>
	public abstract class DelayedAction
	{
		protected CILInstruction sourceInstruction;

		public DelayedAction()
		{			
		}

		public CILInstruction SourceInstruction 
		{
			get { return sourceInstruction; }
			set { sourceInstruction = value; }
		}

		public abstract CILInstruction.Semantic GetSemantic();

		public abstract bool DoesConflict(DelayedAction da);

		public abstract bool CanComplete(ThreadState threadState);

		public abstract void Complete(ThreadState threadState);

		public abstract DelayedAction Duplicate();

		public abstract void TakeSnapshot(StateSnapshot ss);

        // Mark and sweep for garbage collection and thread locality check
        public abstract void MarkAndSweep(int tid, VMValueManager valueMan);

        /// <summary>
        /// Returns whether a DelayedAction accesses some escaping data
        /// Whenever we are unsure, returning true is always safe and correct
        /// </summary>
        /// <returns></returns> 
        public virtual bool Escape(VMValueManager valueMan)
        {
            return true;
        }
    }
}
