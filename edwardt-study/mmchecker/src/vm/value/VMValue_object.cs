using System;
using System.Collections;

namespace mmchecker.vm
{
	/// <summary>
	/// This class stores the value for data of type object, which is essentially
    /// a pointer to object instances
    /// The field valueguid stores the guid of the object it points to
    /// valueguid = -1 iff the pointer is null
	/// </summary>
	public class VMValue_object : VMValue
	{
		CILClass classType;
		int valueguid;

		/// <summary>
		/// Private constructor, to be used by Duplicate() only
		/// </summary>
		private VMValue_object()
		{
		}

		public VMValue_object(int guid, CILClass classType, int objinstGuid) : base(guid)
		{
			this.classType = classType;
			this.valueguid = objinstGuid;
		}

		public CILClass ClassType 
		{
			get { return classType; }
		}

		public int ValueGUID
		{
			get { return valueguid; }
			set { this.valueguid = value; }
		}

		public override void CopyFrom(VMValue other)
		{
			this.valueguid = ((VMValue_object)other).valueguid;
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData(other);
			classType = ((VMValue_object)other).classType;
			valueguid = ((VMValue_object)other).valueguid;
		}

		public override VMValue Duplicate()
		{
			VMValue ret = new VMValue_object();
			ret.CopyInternalData(this);
			return ret;
		}

		/// <summary>
		/// Called during TakeSnapshot in State so that all guids
		/// are reached and renamed
		/// This function is to be overriden by subclasses to traverse
		/// to guids linked to them
		/// </summary>
		public override void TraverseGuids(StateSnapshot ss)
		{
			ss.Convert(valueguid);
		}

		/// <summary>
		/// Called during TakeSnapshot in State to dump the data of this value
		/// </summary>
		/// <param name="ss"></param>
		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData(ss);			
			ss.WriteGuid(valueguid);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            int tmp = MaSProcess(tid);
            if (tmp != -2)
            {
                // check children with tid
                VMValue v = valueMan.GetValue(valueguid);
                if (v != null)
                    v.MarkAndSweep(tmp, valueMan);
            }
        }
    }
}
