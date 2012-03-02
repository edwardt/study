using System;
using System.Collections.Generic;
using mmchecker.util;
using mmchecker.vm.action;

namespace mmchecker.vm
{
	/// <summary>
	/// 
	/// </summary>
	public class ThreadState
	{
		State systemState;
		int threadID;

		// contains VMValue
		FreeStack<int> threadStack = new FreeStack<int>();

		// contains VMLocalVariableBlock
		FreeStack<VMLocalVariableBlock> localVariableBlocks = new FreeStack<VMLocalVariableBlock>();

		// contains ReturnAddress old address to go back for instruction ret
		FreeStack<ReturnAddress> returnAddresses = new FreeStack<ReturnAddress>();

		// Keep a list of DelayedAction
		// TODO: Check internal access
		internal List<DelayedAction> pendingActions = new List<DelayedAction>();

		// current execution program counter and its holder
		CILMethod currentMethod;

		// current instruction to be executed
		// pc == null does not mean thread has ended, because the thread may have incomplete actions
		CILInstruction pc;

		// This is "thread state" in .NET and java definition
		// because we used up the term "state" for model-checking states 
		// we use SyncState to substitute for ThreadState
		public enum SyncState
		{
			RUNNING = 0,
			WAITING = 1
		}
		internal SyncState syncState = SyncState.RUNNING;

		// Keeps the guid of the object this thread is being blocked
		// by waiting on, -1 if thread state is running
		internal int waitingOn = -1;

		/// <summary>
		/// Private constructor for Duplicate()
		/// </summary>
		private ThreadState()
		{
		}

		/// <summary>
		/// Create a new thread based on the information in the thread object
		/// </summary>
		/// <param name="systemState">The State from which this thread is created</param>
		/// <param name="threadobj">The thread object containing information to initialize this thread</param>
		public ThreadState(State systemState, VMValue_thread threadobj)
		{
			this.systemState = systemState;			
            VMValue_threadstart ts = (VMValue_threadstart)GetValue(threadobj.ThreadStartGUID);
            VMValue_object startingObject = (VMValue_object)systemState.Values.MakeValue(GetValue(ts.ObjGUID));
            startingObject.ValueGUID = ((VMValue_object)GetValue(ts.ObjGUID)).ValueGUID;
            startingObject.IsConcrete = true;
            startingObject.IsThreadLocal = true;
//			VMValue_object startingObject = (VMValue_object)GetValue(ts.ObjGUID);
			VMValue_ftn ftn = (VMValue_ftn)GetValue(ts.FtnGUID);
			InitializeThread(threadobj.GUID, startingObject, ftn.Method);
		}

		/// <summary>
		/// Performs initialization of data structure for a newly created thread
		/// It sets up the environment for the first method that runs the thread
		/// </summary>
		/// <param name="threadID">Thread id, which is the guid of the thread object</param>
		/// <param name="startingObject">The object from which the thread is started</param>
		/// <param name="startingMethod">The thread function</param>
		private void InitializeThread(int threadID, VMValue_object startingObject, CILMethod startingMethod)
		{
			this.threadID = threadID;
			currentMethod = startingMethod;
			syncState = SyncState.RUNNING;

			localVariableBlocks.Push(new VMLocalVariableBlock(this, startingMethod));
			if(startingObject != null)
				localVariableBlocks.Peek().AddArgumentFront(startingObject);
			pc = currentMethod.GetFirstInstruction();
		}

		/// <summary>
		/// Count the number of state can branch from this state
		///  .Execute the current instruction
		///  .Complete one of the possible pending actions 
		/// </summary>
		/// <returns>The number of valid transitions from this state</returns>
		public int GetForwardStateCount()
		{
			int count;

			if((syncState == SyncState.RUNNING) && 
				(pc != null) && 
				(pc.CanExecute(this)))
				count = 1;
			else
				count = 0;

			for(int i = 0; i < pendingActions.Count; i++)
				if(IsDelayedActionLocal(i) == false)
					if(CanCompleteAction(i))
					{
						if(pendingActions[i] is DelayedPulse)
						{
							DelayedPulse dp = (DelayedPulse)pendingActions[i];
							if(dp.isPulseAll)
								count++;
							else
							{
								int ctwo = systemState.CountThreadWaitingOn(dp.obj);
								if(ctwo == 0)
									ctwo = 1;
								count += ctwo;
							}
						}else
							count++;
					}

			return count;
		}

		/// <summary>
		/// Transform to next state by choice in the order of
		///  .Execute the current instruction
		///  .Complete one of the pending actions
		/// </summary>
		/// <param name="choice">Indicate the index of the transition to execute</param>
		public void Forward(int choice)
		{
			if((syncState == SyncState.RUNNING) && 
				(pc != null) && 
				(pc.CanExecute(this)))
			{
				if(choice == 0)
				{
					if(CheckerConfiguration.DoPrintExecution)
						Console.WriteLine("Thread {0} executing {1}", currentMethod.ToString(), pc.ToString());
					pc = pc.Execute(this);
					GreedyPORExecution();
					return;
				}
				else
				{
					choice--;
				}
			}
			// we didn't have to execute the pc
			// instead now we have to execute one of the pending actions
			for(int i = 0; i < pendingActions.Count; i++)
				if(IsDelayedActionLocal(i) == false)
					if(CanCompleteAction(i))
					{
						if(pendingActions[i] is DelayedPulse)
						{
							DelayedPulse dp = (DelayedPulse)pendingActions[i];
							if(dp.isPulseAll)
								choice--;
							else
							{
								int ctwo = systemState.CountThreadWaitingOn(dp.obj);
								if(ctwo == 0)
									ctwo = 1;
								choice -= ctwo;
							}
						}
						else
							choice--;

						if(choice < 0)
						{
							DelayedAction da = (DelayedAction)pendingActions[i];
							if(da is DelayedWait)
							{
								DelayedWait dw = (DelayedWait)da;
								DelayedLock dl = new DelayedLock(dw.obj, dw.SourceInstruction, dw.lockCount);
								pendingActions[i] = dl;
							}
							else if(da is DelayedPulse)
							{
								DelayedPulse dp = (DelayedPulse)da;
								if(dp.isPulseAll == true)
								{
									foreach(ThreadState ts in systemState.threads)
										if((ts.syncState == ThreadState.SyncState.WAITING) &&
											(ts.waitingOn == dp.obj))
										{
											ts.syncState = ThreadState.SyncState.RUNNING;
											ts.waitingOn = -1;
										}
								}
								else
								{
									// only if there is at least one thread waiting we resume it
									// else we don't do anything in completing this pulse
									// in another word, the pulse signal is lost
									int ctwo = systemState.CountThreadWaitingOn(dp.obj);
									if(ctwo > 0)
									{										
										for(int j = 0; j < systemState.threads.Count; j++)
										{
											ThreadState ts = (ThreadState)systemState.threads[j];
											if((ts.syncState == SyncState.WAITING) &&
												(ts.waitingOn == dp.obj))
											{
												choice++;
												if(choice == 0)
												{
													ts.syncState = SyncState.RUNNING;
													ts.waitingOn = -1;
													break;
												}
											}
										}
									}
								}
								pendingActions.RemoveAt(i);
							}
							else
							{
								pendingActions.RemoveAt(i);
								da.Complete(this);
							}
							GreedyPORExecution();
							break;
						}
					}
		}

		/// <summary>
		/// Returns whether a forward choice needs reordering
		/// </summary>
		/// <param name="choice"></param>
		/// <returns></returns>
		public DelayedAction IsForwardReordering(int choice)
		{
			if((syncState == SyncState.RUNNING) && 
				(pc != null) && 
				(pc.CanExecute(this)))
			{
				if(choice == 0)
				{
					if(pendingActions.Count == 0)
						return null;
					else
						return new DummyDelayedAction(pc);
				}
				else
				{
					choice--;
				}
			}

			// Just a safety check: The first delayed action must be non-local
			// because else POR execution must have completed it
			if(IsDelayedActionLocal(0) == true)
			{
				Console.WriteLine(this.SystemState.ToString());
				throw new Exception("The first delayed action must be non local");
			}


			// if we don't complete the first delayed action, we need to reorder something
			if(choice == 0) // since choice = 0, we need the first action to be completable so that we don't reorder anything
				if(CanCompleteAction(0))
					return null;
		
			// specific for DelayedPulse, the first completable action 
			// supply more than one choice
			if((CanCompleteAction(0) &&
				pendingActions[0] is DelayedPulse))
			{
				DelayedPulse dp = (DelayedPulse)pendingActions[0];
				if(dp.isPulseAll == false)
				{
					// if there are $m$ threads waiting on obj
					// and the choice is from 0 to $m-1$ then 
					// we do not reorder anything in this transition of choice
					if(systemState.CountThreadWaitingOn(dp.obj) > choice)
						return null;
				}
			}

			for(int i = 0; i < pendingActions.Count; i++)
				if(IsDelayedActionLocal(i) == false)
					if(CanCompleteAction(i))
					{
						if(pendingActions[i] is DelayedPulse)
						{
							DelayedPulse dp = (DelayedPulse)pendingActions[i];
							if(dp.isPulseAll)
								choice--;
							else
							{
								int ctwo = systemState.CountThreadWaitingOn(dp.obj);
								if(ctwo == 0)
									ctwo = 1;
								choice -= ctwo;
							}
						}
						else
							choice--;
						if(choice < 0)
							return (DelayedAction)pendingActions[i];
					}
			throw new Exception("IsForwardReordering(): Should never come here");
		}

        /// <summary>
        /// Returns whether a transition accesses escaping data structures
        /// </summary>
        /// <param name="choice"></param>
        /// <returns></returns>
        public bool IsForwardEscaping(int choice)
        {
            if ((syncState == SyncState.RUNNING) &&
                (pc != null) &&
                (pc.CanExecute(this)))
            {
                if (choice == 0)
                {
                    return pc.Escape();
                }
                else
                {
                    choice--;
                }
            }

            for (int i = 0; i < pendingActions.Count; i++)
                if (IsDelayedActionLocal(i) == false)
                    if (CanCompleteAction(i))
                    {
                        if (pendingActions[i] is DelayedPulse)
                        {
                            DelayedPulse dp = (DelayedPulse)pendingActions[i];
                            if (dp.isPulseAll)
                                choice--;
                            else
                            {
                                int ctwo = systemState.CountThreadWaitingOn(dp.obj);
                                if (ctwo == 0)
                                    ctwo = 1;
                                choice -= ctwo;
                            }
                        }
                        else
                            choice--;
                        if (choice < 0)
                            return pendingActions[i].Escape(systemState.Values);
                    }
            throw new Exception("IsForwardReordering(): Should never come here");
        }

		/// <summary>
		/// Tries to execute as many actions that are locally visible to the thread as possible
		/// </summary>
		private void GreedyPORExecution()
		{
			bool found = true;
			while(found && (systemState.IsEndState == false))
			{
				found = false;
				if((syncState == SyncState.RUNNING) && 
					(pc != null) &&
					(pc.IsThreadLocal()) &&
					(pc.CanExecute(this)))
					{
						if(CheckerConfiguration.DoPrintExecution)
							Console.WriteLine("Thread {0} [POR] executing {1}", currentMethod.ToString(), pc.ToString());
						pc = pc.Execute(this);
						found = true;
					}
				for(int i = 0; i < pendingActions.Count; i++)
				{
					if(IsDelayedActionLocal(i) && 
						(CanCompleteAction(i)))
					{
						// TODO: we don't check for DelayedWait and DelayedPulse 
						// special conditions here because they are always local
						// but becareful later when we use escape analysis
						DelayedAction da = (DelayedAction)pendingActions[i];
						pendingActions.RemoveAt(i);
						if(CheckerConfiguration.DoPrintExecution)
							Console.Write("Thread {0} [POR] ", currentMethod.ToString());
						da.Complete(this);
						found = true;
						break;
					}
				}
			}
		}

        public int GetLocalDelayedActionCount()
        {
            int ret = 0;
            for (int i = 0; i < pendingActions.Count; i++)
                if (IsDelayedActionLocal(i))
                    ret++;
            return ret;
        }

        /// <summary>
        /// Adds an uncompleted action to the thread queue
        /// </summary>
        /// <param name="da">The uncompleted action to be executed later</param>
        public void AddPendingAction(DelayedAction da)
		{
			pendingActions.Add(da);
		}

		private bool IsDelayedActionLocal(int idx)
		{
			DelayedAction da = (DelayedAction)pendingActions[idx];
			if(da is DelayedRead)
			{
				DelayedRead dr = (DelayedRead)da;
				if(GetValue(dr.Source).IsThreadLocal == false)
					return false;
				if(GetValue(dr.Destination).IsThreadLocal == false)
					return false;
				return true;
			}
			else if(da is DelayedWrite)
			{
				DelayedWrite dw = (DelayedWrite)da;
				if(GetValue(dw.Source).IsThreadLocal == false)
					return false;
				if(GetValue(dw.Destination).IsThreadLocal == false)
					return false;
				return true;
			}else if(da is DelayedLock)
				return false;
			else if(da is DelayedUnlock)
				return false;
			else if(da is DelayedPulse)
				return false;
			else if(da is DelayedWait)
				return false;
			else
				throw new Exception("Unknown delayed action type");
		}

		private bool CanCompleteAction(int idx)
		{
			DelayedAction da = (DelayedAction)pendingActions[idx];
			if(da.CanComplete(this) == false)
				return false;
			int i;
			for(i = 0; i < idx; i++)
			{
				DelayedAction bef = (DelayedAction)pendingActions[i];
				if(OrderingTable.IsAllowed(bef.GetSemantic(), da.GetSemantic()) == false)
					return false;
				if(da.DoesConflict(bef))
					return false;
			}
			return true;
		}

		public bool HasEnded()
		{
			return (pc == null) && (pendingActions.Count == 0);
		}
		
		public override string ToString()
		{
			string strStack = "[";
			for(int i = 0; i < threadStack.Count; i++)
			{
				if(i != 0)
					strStack += ",";
				VMValue v = (VMValue)GetValue(threadStack[i]);
				if(v is VMValue_int32)
					strStack += ((VMValue_int32)v).Value;
				else if(v is VMValue_object)
					strStack += ((VMValue_object)v).ClassType;
				else
					strStack += "x";
				if(v.IsConcrete == false)
					strStack += "(*)";				
			}
			strStack += "]";

			string strAction = "[";
			for(int i = 0; i < pendingActions.Count; i++)
			{
				DelayedAction da = (DelayedAction)pendingActions[i];
				bool isLocal = IsDelayedActionLocal(i);
				if(isLocal == false)
					strAction += "(";
				if(da is DelayedRead)
					strAction += "R";
				else if(da is DelayedWrite)
					strAction += "W";
				else if(da is DelayedLock)
					strAction += "L";
				else if(da is DelayedUnlock)
					strAction += "U";
				else if(da is DelayedWait)
					strAction += "A";
				else if(da is DelayedPulse)
					strAction += "P";
				else
					throw new Exception("Unknown delayed action type to display");
				if(isLocal == false)
					strAction += ")";
			}
			strAction += "]";

			string strSyncState = "";
			if(syncState == SyncState.WAITING)
				strSyncState += " [Waiting]";

			if((pc == null) || (currentMethod == null))
				return "Thread still have "
					+ strAction + "  "
					+ strStack + strSyncState;
			else
				return "Thread " + currentMethod.Name + strSyncState 
					+ " " + pc.Label + " " 
					+ strAction + "  "
					+ strStack;
		}

		public ThreadState Duplicate(State oldState, State newState)
		{
			ThreadState ret = new ThreadState();
			ret.systemState = newState;
			ret.threadID = threadID;
			ret.syncState = syncState;
			ret.waitingOn = waitingOn;

			for(int i = 0; i < threadStack.Count; i++)
				ret.threadStack.Add(threadStack[i]);

			for(int i = 0; i < localVariableBlocks.Count; i++)
				ret.localVariableBlocks.Push(localVariableBlocks[i].Duplicate(this, ret));

			IEnumerator<DelayedAction> iter = pendingActions.GetEnumerator();
			while(iter.MoveNext())
				ret.pendingActions.Add(iter.Current.Duplicate());

			ret.returnAddresses = new FreeStack<ReturnAddress>();
			foreach(ReturnAddress ra in returnAddresses)
				ret.returnAddresses.Add(ra.Duplicate());

			ret.currentMethod = currentMethod;
			ret.pc = pc;
			return ret;
		}

		public CILInstruction MethodReturn()
		{
			localVariableBlocks.Pop();
			if(returnAddresses.Count == 0)
			{
				return null;
			}
			else
			{
				ReturnAddress ra = returnAddresses.Pop();
				currentMethod = ra.theMethod;
				pc = currentMethod.GetNextInstruction(ra.thePC);
				return pc;
			}
		}

		public CILInstruction CallFunction(CILMethod theMethod)
		{
			ReturnAddress ra = new ReturnAddress(currentMethod, pc);
			returnAddresses.Push(ra);

			VMLocalVariableBlock vBlock = new VMLocalVariableBlock(this, theMethod);
			localVariableBlocks.Push(vBlock);
			// The first argument is the object itself
			for(int i = -1; i < theMethod.ParameterCount; i++)
				vBlock.AddArgumentFront(GetValue(threadStack.Pop()));
			currentMethod = theMethod;

			return theMethod.GetFirstInstruction();
		}

		public void TakeSnapshot(StateSnapshot ss)
		{
			ss.WriteGuid(threadID);

			ss.WriteInt((int)syncState);
			ss.WriteGuid(waitingOn);

			ss.WriteInt(threadStack.Count);
			for(int i = 0; i < threadStack.Count; i++)
				ss.WriteGuid(threadStack[i]);

			ss.WriteInt(localVariableBlocks.Count);
			for(int i = 0; i < localVariableBlocks.Count; i++)
			{
				localVariableBlocks[i].TakeSnapshot(ss);
			}

			ss.WriteInt(returnAddresses.Count);
			for(int i = 0; i < returnAddresses.Count; i++)
			{
				ReturnAddress ra = returnAddresses[i];
				ra.TakeSnapshot(ss);
			}

			ss.WriteInt(currentMethod.ID);
			ss.WriteInt(currentMethod.GetInstructionIndex(pc));

			ss.WriteInt(pendingActions.Count);
			for(int i = 0; i < pendingActions.Count; i++)
			{
				DelayedAction da = (DelayedAction)pendingActions[i];
				da.TakeSnapshot(ss);
			}
		}

		public CILInstruction PC 
		{
			get { return pc; }
		}

		public CILMethod CurrentMethod 
		{
			get { return currentMethod; }
		}

		public FreeStack<int> ThreadStack 
		{
			get { return threadStack; }
		}

		public State SystemState 
		{
			get { return systemState; }
		}

		public VMValue GetLocalVariable(int localVarIndex)
		{
			return localVariableBlocks.Peek().GetVariableValue(localVarIndex);
		}

		public VMValue GetLocalArgument(int argIndex)
		{
			return localVariableBlocks.Peek().GetArgument(argIndex);
		}

		public VMValue GetValue(int guid)
		{
			return systemState.Values.GetValue(guid);
		}

		public int ThreadID 
		{
			get { return threadID; }
		}

        /// <summary>
        /// Count the number of delayed actions that are local
        /// Used to limit number of incomplete actions at a time
        /// TODO: Will be removed later for better limiting code
        /// </summary>
        /// <returns></returns>
		public int CountNLocalDelayedAction()
		{
			int counter = 0;
			for(int i = 0; i < pendingActions.Count; i++)
				if(IsDelayedActionLocal(i))
					counter++;
			return counter;
		}

        public void MarkAndSweep()
        {
            foreach (int i in threadStack)
                GetValue(i).MarkAndSweep(threadID, systemState.Values);
            foreach (VMLocalVariableBlock l in localVariableBlocks)
                l.MarkAndSweep(threadID, systemState.Values);
            foreach (DelayedAction da in pendingActions)
                da.MarkAndSweep(threadID, systemState.Values);
        }
	}

	class ReturnAddress
	{
		public CILMethod theMethod;
		public CILInstruction thePC;

		public ReturnAddress(CILMethod theMethod, CILInstruction thePC)
		{
			this.theMethod = theMethod;
			this.thePC = thePC;
		}

		public ReturnAddress Duplicate()
		{
			return new ReturnAddress(theMethod, thePC);
		}

		public void TakeSnapshot(StateSnapshot ss)
		{
			ss.WriteInt(theMethod.ID);
			ss.WriteInt(theMethod.GetInstructionIndex(thePC));
		}
	}
}
