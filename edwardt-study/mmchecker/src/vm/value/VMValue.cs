using System;

namespace mmchecker.vm
{
	/// <summary>
	/// VMValue is the 
	/// </summary>
	public abstract class VMValue
	{
		// TODO: check access modifiers
		protected int guid;
		internal int isConcrete = 0;

		// TODO: value thread locality is determined by traversing, 
		// most probably by GC, it is not a fixed, settable value
		protected bool isThreadLocal = false;

        // data structure for dynamic escape analysis
        protected int ownerThread;
        protected bool escape;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		internal VMValue()
		{
		}

		public VMValue(int guid)
		{
			this.guid = guid;
		}

		public abstract void CopyFrom(VMValue other);

		public bool IsConcrete 
		{
			get { return isConcrete == 1; }
			set { isConcrete = value ? 1 : 0; }
		}

		public int GUID 
		{
			get { return guid; }
		}

		public abstract VMValue Duplicate();

		public virtual void CopyInternalData(VMValue other)
		{
			this.guid = other.guid;
			this.isConcrete = other.isConcrete;
			this.isThreadLocal = other.isThreadLocal;
		}

		public bool IsThreadLocal 
		{
			get { return isThreadLocal; }
			set { isThreadLocal = value; }
		}

        public bool Escape
        {
            get { return escape; }
        }

		/// <summary>
		/// Called during TakeSnapshot in State so that all guids
		/// are reached and renamed
		/// This function is to be overriden by subclasses to traverse
		/// to guids linked to them
		/// </summary>
		public virtual void TraverseGuids(StateSnapshot ss)
		{
			// do nothing
		}

		/// <summary>
		/// Called during TakeSnapshot in State to dump the data of this value
		/// </summary>
		/// <param name="ss"></param>
		public virtual void SaveData(StateSnapshot ss)
		{
			ss.WriteInt(isConcrete);
		}
        /// <summary>
        /// Explores all reachable VMValue from this to mark 
        /// <param>tid: ID (guid) of the thread checking this VMValue</param>
        /// </summary>
        public abstract void MarkAndSweep(int tid, VMValueManager valueMan);

        public void ClearMaS()
        {
            escape = false;
            ownerThread = -1;
        }

        protected int MaSProcess(int tid)
        {
            if (!escape)
            {
                // we only explore more if this value is not escaping
                if (tid == -1)
                {
                    // the parent Value is escaping, and this Value is not escaping
                    // so we need to set it to escaping, and check its children
                    escape = true;
                    return -1;
                }
                else
                {
                    if (ownerThread != tid)
                    {
                        // if ownerThread == tid, we are still in this thread's accessible area
                        // so we only check if the ownerThread is different
                        if (ownerThread == -1)
                        {
                            // if the ownerThread is -1, we haven't checked this value,
                            // so we need to explore, else we just skip
                            ownerThread = tid;
                            return tid;
                        }
                        else
                        {
                            // ownerThread is different, which means this value is escaping
                            // we set this to escaping, and then explore again
                            escape = true;
                            return -1;
                        }
                    }
                }
            }
            return -2;
        }
    }
}
