using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using mmchecker.util;
using mmchecker.vm.observer;
using mmchecker.vm.action;

namespace mmchecker.vm
{
	/// <summary>
	/// The class contains all information of a state of
	/// the virtual machine
	/// </summary>
	public class State
	{
		// contains all ThreadState
		internal List<ThreadState> threads = new List<ThreadState>();

		// contains all static variables 
		// TODO: Change the name
		VMGlobalVariables heap;

		// 
		VMValueManager values;

		// Pointer to the parsed program text, is not stored during snapshot
		CILProgram program;

		// observers, just copy the observers pointer over, not in VM data structure
		ArrayList observers;

        List<int> steps = new List<int>();

		// TODO: change to more systematic state violation checking
		bool isEndState = false;
		int reportedValue;

		// Snapshot and its signature
		byte[] snapshot = null;
		byte[] snapshotSig = null;
		// Utility class to make md5 signature from snapshot
		static MD5 md5 = new MD5CryptoServiceProvider();

		/// <summary>
		/// Just initialize an empty state
		/// </summary>
		private State()
		{
		}

		public static State MakeInitialState(CILProgram program)
		{			
			State state = new State();
			state.values = new VMValueManager();
			state.heap = new VMGlobalVariables(state);
			state.program = program;

			VMValue_ftn ftn = state.Values.MakeFtnValue(program.EntryPoint);
			ftn.IsConcrete = true;
			VMValue_object obj = (VMValue_object)state.Values.MakeValue(new CILVar_object("", program.GetClass("[mscorlib]System.Object")));
			obj.IsConcrete = true;
			VMValue_threadstart ts = state.Values.MakeThreadStartValue(obj, ftn);
			ts.IsConcrete = true;
			VMValue_thread threadobj = state.Values.MakeThreadValue(ts);
			threadobj.IsConcrete = true;

			ThreadState initThread = new ThreadState(state, threadobj);
			state.AddThread(initThread);
			state.TakeSnapshot();
			return state;
		}

		/// <summary>
		/// Count the number of possible forward states from this state
		/// This includes 
		///  .Executing the instruction at pc
		///  .Complete a pending action
		/// for each thread
		/// </summary>
		/// <returns>The number of possible forward states from this state</returns>
		public int GetForwardStateCount()
		{			
			int total = 0;
			for(int i = 0; i < threads.Count; i++)
				total += threads[i].GetForwardStateCount();
			return total;
		}

		/// <summary>
		///  Used together with GetForwardStateCount
		///  Advance the state in one of the choice from 0 to (count - 1)
		/// </summary>
		/// <param name="choice">The branch to forward</param>
		public void Forward(int choice)
		{
            if (CheckerConfiguration.DoPrintExecution)
            {
                Console.WriteLine("=================================");
                Console.Write(ToString());
            }

            steps.Add(choice);
			for(int i = 0; i < threads.Count; i++)
			{
				int nThreadChoice = threads[i].GetForwardStateCount();
				if(nThreadChoice > choice)
				{
					threads[i].Forward(choice);
                    MarkAndSweep();
					TakeSnapshot();
					return;
				}
				else
				{
					choice -= nThreadChoice;
				}
			}
            throw new Exception("Should not come here");
		}

		/// <summary>
		///  Returns whether this forward choice requires reordering
		/// </summary>
		/// <param name="choice">The branch to forward</param>
		public DelayedAction IsForwardReordering(int choice)
		{
			for(int i = 0; i < threads.Count; i++)
			{
				int nThreadChoice = threads[i].GetForwardStateCount();
				if(nThreadChoice > choice)
				{
					return threads[i].IsForwardReordering(choice);					
				}
				else
				{
					choice -= nThreadChoice;
				}
			}
			// Should never come here
			Console.WriteLine("BUG: Should not come here, State::IsForwardReordering()");
			return null;
		}

        public bool IsForwardEscaping(int choice)
        {
            for (int i = 0; i < threads.Count; i++)
            {
                int nThreadChoice = threads[i].GetForwardStateCount();
                if (nThreadChoice > choice)
                {
                    return threads[i].IsForwardEscaping(choice);
                }
                else
                {
                    choice -= nThreadChoice;
                }
            }
            // Should never come here
            throw new Exception("Transition index exceeded number of transitions");
        }

        public int[] GetThreadTransitions(int threadIndex)
        {
            int skip = 0;
            for (int i = 0; i < threadIndex; i++)
                skip += threads[i].GetForwardStateCount();
            int[] ret = new int[threads[threadIndex].GetForwardStateCount()];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = skip + i;
            return ret;
        }

        public int GetStaticVariable(CILClass classType, CILVariable variable)
		{
			return heap.GetStaticVariable(classType, variable);
		}

		public int ThreadCount 
		{
			get { return threads.Count; }
		}

		public ThreadState GetThreadByIndex(int index)
		{
			return threads[index];
		}

		/// <summary>
		/// Counts the number of thread currently blocked by waiting 
		/// on a particular object instance
		/// </summary>
		/// <param name="guid">The guid of the object instance</param>
		/// <returns></returns>
		public int CountThreadWaitingOn(int guid)
		{
			int counter = 0;

			foreach(ThreadState ts in threads)
				if((ts.syncState == ThreadState.SyncState.WAITING) &&
					(ts.waitingOn == guid))
					counter++;
			return counter;
		}

		public CILProgram Program 
		{
			get { return program; }
		}

		public VMValueManager Values
		{
			get { return values; }
		}

		public bool IsEndState 
		{
			get { return isEndState || IsDeadLock; }
		}

        public int GetLocalDelayedActionCount()
        {
            int ret = 0;
            for (int i = 0; i < threads.Count; i++)
                ret += threads[i].GetLocalDelayedActionCount();
            return ret;
        }

        public bool IsDeadLock
		{
			get
			{
				if((GetForwardStateCount() == 0) && (GetLocalDelayedActionCount() == 0))
				{
					foreach(ThreadState ts in threads)
						if(ts.HasEnded() == false)
							return true;
					return false;
				}
				else
					return false;
			}
		}

		public int ReportedValue 
		{
			get { return reportedValue; }
		}

		/// <summary>
		/// Add a new thread to this state
		/// This is called when a new thread is started
		/// </summary>
		/// <param name="newThread">Reference to the ThreadState object</param>
		public void AddThread(ThreadState newThread)
		{
			threads.Add(newThread);
		}

		/// <summary>
		/// Gets the ThreadState object by thread id, which is the guid of
		/// the thread object.
		/// </summary>
		/// <param name="id">id of the thread, which is the guid of the thread object</param>
		/// <returns>Reference to the ThreadState with the specified id, or null if there is no such thread</returns>
		public ThreadState GetThreadByID(int id)
		{
			foreach(ThreadState ts in threads)
				if(ts.ThreadID == id)
					return ts;
			return null;
		}

		public override string ToString()
		{
            string ret = "";
			for(int i = 0; i < threads.Count; i++)
				ret += threads[i].ToString() + "\n";

            ret += "Forward choices : " + GetForwardStateCount() + " : ";

            for (int i = 0; i < ThreadCount; i++)
            {
                int[] trans = GetThreadTransitions(i);
                if (trans.Length == 0)
                    continue;
                ret += "[";
                for (int j = 0; j < trans.Length; j++)
                    if (IsForwardEscaping(trans[j]) == true)
                        ret += "G";
                    else
                        ret += "L";
                ret += "]";
            }
            ret += "\n";
            return ret;
		}

		/// <summary>
		/// Return a copy of this state
		/// </summary>
		/// <returns>Reference to a copy of this state</returns>
		public State Duplicate()
		{
			State ret = new State();
			// change this to a for instead of foreach to ensure 
			// the order in this and in the copy is the same for DelayedPulse
			for(int i = 0; i < threads.Count; i++)
				ret.threads.Add(threads[i].Duplicate(this, ret));
			ret.program = program;
			ret.heap = heap.Duplicate(this, ret);
			ret.values = values.Duplicate(); 
			ret.observers = observers;
			ret.steps.AddRange(steps);
			ret.snapshot = snapshot;
			return ret;
		}

		/// <summary>
		/// Add a new observer to the state. The change affects 
		/// all states that are duplicated or derived from this state
		/// </summary>
		/// <param name="o">The observer that will be watching this state</param>
		public void AddObserver(Observer o)
		{
			if(observers == null)
				observers = new ArrayList();
			observers.Add(o);
		}

		/// <summary>
		/// Utility method to report various events during execution
		/// Will be removed later for more systematic state observation
		/// </summary>
		/// <param name="v"></param>
		public void Report(int v)
		{
			// TODO: Change to more systematic state observation
			if(observers != null)
				foreach(Observer o in observers)
					o.Report(v);
			isEndState = true;
			reportedValue = v;
				 
			Console.Write("({0}): ", v);
			for(int i = 0; i < steps.Count; i++)
				Console.Write("{0} ", steps[i]);
			Console.WriteLine();
		}

		/// <summary>
		/// Take a snapshot of the state to a bytestream
		/// </summary>
		/// <returns></returns>
		private void TakeSnapshot()
		{
            // TODO: Incorporating snapshot with mark and sweep as they both traverse 
            // the memory graph, althought it may be very complex
			StateSnapshot ss = new StateSnapshot(values);
			heap.TakeSnapshot(ss);
			ThreadState[] arr = threads.ToArray();
/*			int i, j;
			for(i = 0; i < arr.Length; i++)
				for(j = i + 1; j < arr.Length; j++)
					if(arr[i].ThreadStack.Count > arr[j].ThreadStack.Count)
					{
						ThreadState tmp = arr[i];
						arr[i] = arr[j]; 
						arr[j] = tmp;
					}*/

			ss.WriteInt(arr.Length);
			for(int i = 0; i < arr.Length; i++)
				arr[i].TakeSnapshot(ss);
			snapshot = ss.GetStoringData();
			snapshotSig = md5.ComputeHash(snapshot);
		}

		public byte[] Snapshot 
		{
			get 
			{ 
				if(snapshot == null)
					throw new Exception("Should not come here");
				return snapshot; 
			}
		}
		
		public byte[] SnapshotSig
		{
			get
			{
				if(snapshotSig == null)
					throw new Exception("Should not come here");
				return snapshotSig;
			}
		}

		/// <summary>
		/// Returns the length of the longest DelayedAction queue
        /// Used to limit number of incomplete actions, will be removed later
        /// for a better design
		/// </summary>
		/// <returns></returns>
		public int GetMaxDAQLength()
		{
			int ret = 0;
			foreach(ThreadState ts in threads)
				if(ts.CountNLocalDelayedAction() > ret)
					ret = ts.CountNLocalDelayedAction();
			return ret;
		}

        /// <summary>
        /// Use Mark-and-Sweep algorithm to do dynamic escape analysis on VMValues
        /// The unreachable VMValues are also removed as GC would do.
        /// </summary>
        public void MarkAndSweep()
        {
            values.ClearMaS();
            foreach (ThreadState ts in threads)
                ts.MarkAndSweep();
            values.CleanGarbage();
        }
	}
}
