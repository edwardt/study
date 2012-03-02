using System;
using System.Collections;
using System.Collections.Generic;

namespace mmchecker.vm
{
	/// <summary>
	/// Contains the value for an instance of a class
    /// It also has two fields holdingLockThreadID and holdingLockCount to stores 
    ///  locking status on it.
    /// Object instances are accessible only through object pointers, VMValue_objects
	/// </summary>
	public class VMValue_objectinst : VMValue
	{
		CILClass classType;
		int holdingLockThreadID;
		int holdingLockCount;
        
        // Array of guids of non-static fields
		List<int> fields = new List<int>();

		/// <summary>
		/// Private constructor to be used by Duplicate()
		/// </summary>
		internal VMValue_objectinst()
		{
		}

		public VMValue_objectinst(int guid, VMValueManager valueFactory, CILClass classType) : base(guid)
		{
			this.classType = classType;
			this.holdingLockThreadID = -1;
			this.holdingLockCount = 0;

			IEnumerator iter = classType.GetFieldEnumerator();
			while(iter.MoveNext())
			{
				CILClassField field = (CILClassField)iter.Current;
				if(field.IsStatic == false)
				{
					VMValue f = valueFactory.MakeValue(field.Variable); 
					f.IsConcrete = true;
					fields.Add(f.GUID);
				}
			}
		}

		public int HoldingLockThreadID 
		{
			get { return holdingLockThreadID; }
			set { holdingLockThreadID = value; }
		}

		public int HoldingLockCount 
		{
			get { return holdingLockCount; }
			set { holdingLockCount = value; }
		}

		public int GetFieldGUID(string classname, string fieldname)
		{
			// TODO: support polymorphism by checking classname
			IEnumerator iter = classType.GetFieldEnumerator();
			int counter = 0;
			while(iter.MoveNext())
			{
				CILClassField field = (CILClassField)iter.Current;
				if(field.IsStatic == false)
				{
					if(field.Variable.Name == fieldname)
						break;
					counter++;
				}
			}
			return fields[counter];
		}

		public int[] GetAllFieldGuids()
		{
			int[] ret = new int[fields.Count];
			for(int i = 0; i < ret.Length; i++)
				ret[i] = fields[i];
			return ret;
		}

		public override void CopyFrom(VMValue other)
		{
			// TODO: object data assignment
			throw new Exception("TODO");
		}

		public override void CopyInternalData(VMValue other)
		{
			base.CopyInternalData (other);
			VMValue_objectinst theOther = (VMValue_objectinst)other;
			classType = theOther.classType;
			holdingLockThreadID = theOther.holdingLockThreadID;
			holdingLockCount = theOther.holdingLockCount;
			
			for(int i = 0; i < ((VMValue_objectinst)other).fields.Count; i++)
				fields.Add(theOther.fields[i]);
		}

		public override VMValue Duplicate()
		{
			VMValue_objectinst ret = new VMValue_objectinst();	
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
			for(int i = 0; i < fields.Count; i++)
				ss.Convert(fields[i]);
		}

		public override void SaveData(StateSnapshot ss)
		{
			base.SaveData(ss);
			ss.WriteInt(classType.ID);
			ss.WriteGuid(holdingLockThreadID);
			ss.WriteInt(holdingLockCount);
			for(int i = 0; i < fields.Count; i++)
				ss.WriteGuid(fields[i]);
		}

        public override void MarkAndSweep(int tid, VMValueManager valueMan)
        {
            int tmp = MaSProcess(tid);
            if (tmp != -2)
            {
                // check children with tid
                for (int i = 0; i < fields.Count; i++)
                {
                    VMValue v = valueMan.GetValue(fields[i]);
                    if (v != null)
                        v.MarkAndSweep(tmp, valueMan);
                }
            }
        }
    }
}