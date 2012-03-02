using System;

namespace mmchecker.vm
{
	/// <summary>
	/// This class stores an instance of System.Threading.ThreadStart, and is a subclass
    /// of VMValue_objectinst. It contains a guid to the object and ftn that start the thread
	/// </summary>
	public class VMValue_threadstart : VMValue_objectinst
	{
		int objGuid;
		int ftnGuid;

		/// <summary>
		/// Private constructor to be used by Duplicate() only
		/// </summary>
		private VMValue_threadstart()
		{			
		}

		public VMValue_threadstart(int guid, VMValueManager valueFactory, int objGuid, int ftnGuid) 
			: base(guid, valueFactory, new CILClass("[mscorlib]System.Threading.ThreadStart"))
		{
			this.objGuid = objGuid;
			this.ftnGuid = ftnGuid;
		}

		public int ObjGUID
		{
			get { return objGuid; }
		}

		public int FtnGUID
		{
			get { return ftnGuid; }
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData(other);
			objGuid = ((VMValue_threadstart)other).objGuid;
			ftnGuid = ((VMValue_threadstart)other).ftnGuid;
		}


		public override VMValue Duplicate()
		{
			// TODO: design to copy the fields systematically
			VMValue_threadstart ret = new VMValue_threadstart();
			ret.CopyInternalData(this);
			return ret;
		}

		public override void CopyFrom(VMValue other)
		{
			objGuid = ((VMValue_threadstart)other).objGuid;
			ftnGuid = ((VMValue_threadstart)other).ftnGuid;
		}

		/// <summary>
		/// Called during TakeSnapshot in State so that all guids
		/// are reached and renamed
		/// This function is to be overriden by subclasses to traverse
		/// to guids linked to them
		/// </summary>
		public override void TraverseGuids(StateSnapshot ss)
		{
			ss.Convert(objGuid);
			ss.Convert(ftnGuid);
		}

		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData(ss);
			ss.WriteGuid(objGuid);
			ss.WriteGuid(ftnGuid);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            int tmp = MaSProcess(tid);
            if (tmp != -2)
            {
                VMValue v = valueMan.GetValue(objGuid);
                if (v != null)
                    v.MarkAndSweep(tmp, valueMan);
                v = valueMan.GetValue(ftnGuid);
                if (v != null)
                    v.MarkAndSweep(tmp, valueMan);
            }
        }
    }
}
