using System;
using System.Collections;

namespace mmchecker.vm
{
	/// <summary>
	/// Summary description for VMValueFactory.
	/// </summary>
	public class VMValueManager
	{
		int nextguid;
		Hashtable allValues;

		public VMValueManager()
		{
			nextguid = 0;
			allValues = new Hashtable();
		}

		public VMValue MakeValue(CILVariable variable)
		{
			VMValue ret;

			if(variable is CILVar_int32)
			{
				ret = new VMValue_int32(nextguid, 0);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(variable is CILVar_int64)
			{
				ret = new VMValue_int64(nextguid, 0);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(variable is CILVar_double)
			{
				ret = new VMValue_double(nextguid, 0);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(variable is CILVar_object)
			{
				ret = new VMValue_object(nextguid, ((CILVar_object)variable).ClassType, -1);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(variable is CILVar_array)
			{
				ret = new VMValue_array(nextguid, -1);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else
				throw new Exception("Unknown value type to make a value");
		}

		/// <summary>
		/// Make a new value of the same type of the given value
		/// The given value is not modified after the operation, only its type is used
		/// </summary>
		/// <param name="value">The given value to get the type</param>
		/// <returns>A new value of the same type of the given value</returns>
		public VMValue MakeValue(VMValue value)
		{
			VMValue ret;
			if(value is VMValue_int32)
			{				
				ret = new VMValue_int32(nextguid, 0);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(value is VMValue_int64)
			{				
				ret = new VMValue_int64(nextguid, 0);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(value is VMValue_double)
			{				
				ret = new VMValue_double(nextguid, 0);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(value is VMValue_object)
			{
				ret = new VMValue_object(nextguid, ((VMValue_object)value).ClassType, -1);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else if(value is VMValue_array)
			{
				ret = new VMValue_array(nextguid, -1);
				allValues.Add(nextguid, ret);
				nextguid++;
				return ret;
			}
			else
				throw new Exception("Unknown value type to make a value");
		}

		public VMValue_object MakeNullValue()
		{
			VMValue_object ret = new VMValue_object(nextguid, null, -1);
			allValues.Add(nextguid, ret);
			nextguid++;
			return ret;
		}

		public VMValue_objectinst MakeObjectInstance(CILVariable variable)
		{
			int guid = nextguid;
			nextguid++;
			VMValue_objectinst ret = new VMValue_objectinst(guid, this, ((CILVar_object)variable).ClassType);
			allValues.Add(guid, ret);
			return ret;
		}

		public VMValue_ftn MakeFtnValue(CILMethod method)
		{
			VMValue_ftn ret = new VMValue_ftn(nextguid, method);
			allValues.Add(nextguid, ret);
			nextguid++;
			return ret;
		}

		public VMValue_threadstart MakeThreadStartValue(VMValue_object obj, VMValue_ftn method)
		{
			int guid = nextguid++;
			VMValue_threadstart ret = new VMValue_threadstart(guid, this, obj.GUID, method.GUID);
			allValues.Add(guid, ret);
			return ret;
		}

		public VMValue_thread MakeThreadValue(VMValue_threadstart threadStart)
		{
			int guid = nextguid++;
			VMValue_thread ret = new VMValue_thread(guid, this, threadStart.GUID);
			allValues.Add(guid, ret);
			return ret;
		}

		public VMValue GetValue(int guid)
		{
			return (VMValue)allValues[guid];
		}

		public VMValueManager Duplicate()
		{
			VMValueManager ret = new VMValueManager();
			ret.nextguid = nextguid;

			IDictionaryEnumerator iter = allValues.GetEnumerator();
			while(iter.MoveNext())
				ret.allValues.Add(iter.Key, ((VMValue)iter.Value).Duplicate());

			return ret;
		}

		public VMValue_array MakeArray()
		{
			int guid = nextguid++;
			VMValue_array ret = new VMValue_array(guid, -1);
			allValues.Add(guid, ret);
			return ret;			
		}

		public VMValue_arrayinst MakeArrayInst(int size, CILVariable elementType)
		{
			int guid = nextguid++;
			VMValue_arrayinst ret = new VMValue_arrayinst(guid, size, elementType, this);
			allValues.Add(guid, ret);
			return ret;			
		}

        public void ClearMaS()
        {
            IDictionaryEnumerator iter = allValues.GetEnumerator();
            while (iter.MoveNext())
                ((VMValue)iter.Value).ClearMaS();
        }

        /// <summary>
        /// Clean up VMValues that are not visited by mark-and-sweep algorithm
        /// These values are unreachable and can be collected by garbage collector
        /// TODO: Lower nextguid or change method of allocation new guid
        /// TODO: Consider efficient removal of multiple entries in hash table
        /// TODO: Garbage collectors may call destructor of objects
        /// </summary>
        public void CleanGarbage()
        {
            // TODO: clean up values with ownerThread == -1
            // right now we don't clean up to avoid introducing bugs into checking 
        }
	}
}
