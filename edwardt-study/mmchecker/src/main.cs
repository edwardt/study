using System;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using mmchecker;
using mmchecker.vm;
using mmchecker.util;
using mmchecker.maxflow;
using mmchecker.vm.observer;
using mmchecker.vm.action;
using mmchecker.tracing;
using mmchecker.algorithm;

namespace mmchecker
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Class1
	{
		static CILProgram program;
		static StateSet stateSet = new StateSet();
		static maxflow.Graph mfGraph = new maxflow.Graph();
		static ArrayList goodReportedValues = new ArrayList();

		// keeps the time that we start making flow graph 
		// to display the time needed to find invalid states
		static TimeSpan maxflowTime;

    	/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
//			TestMaxFlow.Test();
//			return;
			if(args.Length < 1)
			{
				Console.WriteLine("mmchecker test");
				return;
			}

			if(args[0].Equals("maketest"))
			{
				MakeTest(args[1]);
				return;
			}

			CheckerConfiguration.LoadCheckerConfiguration("general.conf");
			CheckerConfiguration.LoadCheckerConfiguration(args);
			OrderingTable.LoadTable(CheckerConfiguration.GetProperty("memorymodel"));

			string programname = CheckerConfiguration.GetProperty("program");
			CILParser Parser = new CILParser();
			program = Parser.Parse(programname + ".il");
			StreamWriter st = new StreamWriter(programname + ".parse");
			st.Write(program.Print());
			st.Close();
		
			// catch -trace option
			if(CheckerConfiguration.GetNumericProperty("trace") == 1)
			{
				Tracer.GoTrace(program);
				return;
			}

			State st0 = State.MakeInitialState(program);
			
			Observer ob;
			
			switch(CheckerConfiguration.GetProperty("observer"))
			{
				case "SimpleObserver":
					ob = new SimpleObserver();
					break;
				case "SimpleCumulativeObserver":
					ob = new SimpleCumulativeObserver();
					break;
				default:
					throw new Exception("Unknown observer " + CheckerConfiguration.GetProperty("observer") + " to create");
			}
			st0.AddObserver(ob);

			Random r;
			switch(CheckerConfiguration.GetProperty("search"))
			{
				case "left":
					while(st0.GetForwardStateCount() > 0)
					{
						st0 = st0.Duplicate();
						st0.Forward(0);
					}
					break;
				case "random":
					r = new Random();
					while(st0.GetForwardStateCount() > 0)
					{
						st0 = st0.Duplicate();
						st0.Forward(r.Next(st0.GetForwardStateCount()));
					}
					break;
				case "repeated_random":
					r = new Random();
					while(true)
					{
						st0 = State.MakeInitialState(program);
						st0.AddObserver(ob);						
						while(st0.GetForwardStateCount() > 0)
						{
							st0 = st0.Duplicate();
							st0.Forward(r.Next(st0.GetForwardStateCount()));
						}
					}
				case "guided":
					GuidedSearch(st0);
					break;
				case "checkoptimality":
				case "co":
					CheckOptimality(st0);
					break;
				case "full":
					Search(st0);
					break;
				case "maxflow":
					if(CheckerConfiguration.NumThreads > 1)
					{
						FindBarrierMaxflowParallel f = new FindBarrierMaxflowParallel();
						f.Execute();
					}else
						FindBarrierByMaxflow(st0);
					break;
				default:
					throw new Exception("Unknown search algorithm " + CheckerConfiguration.GetProperty("search_algorithm"));
			}
			ob.PrintReport();
			Console.WriteLine("Bytecodes: {0}", program.GetInstructionCount());
			Console.WriteLine("State count: {0}", stateSet.Count);
			if(CheckerConfiguration.DoTiming)
			{
				Console.WriteLine("Time used: {0}", Process.GetCurrentProcess().TotalProcessorTime);
				Console.WriteLine("Maxflow time: {0}", maxflowTime);
			}
		}

		public static void FindBarrierByMaxflow(State st0)
		{
            SimpleFlowGraphBuilder builder = new SimpleFlowGraphBuilder();
            mfGraph = builder.BuildGraph(st0);
            Console.WriteLine("Done, running maxflow algorithm with {0} vertices {1} edges", mfGraph.CountVertex(), mfGraph.CountEdge());
			Console.WriteLine("Ford-Fulkerson: {0}", mfGraph.FordFulkerson());
            DateTime maxflowStartTime = DateTime.Now;
            Edge[] edges = mfGraph.MinCut;
            maxflowTime = DateTime.Now.Subtract(maxflowStartTime);
			ArrayList positions = new ArrayList();
			foreach(Edge e in edges)
			{
				CILInstruction inst = ((DelayedAction)e.ExtraInfo).SourceInstruction;
				bool found = false;
				foreach(CILInstruction ii in positions)
					if(ii == inst)
					{
						found = true;
						break;
					}
				if(found == false)
					positions.Add(inst);
			}
			foreach(CILInstruction inst in positions)
				Console.WriteLine(inst.ParentMethod + "::" + inst.ToString());
		}


		public static void GuidedSearch(State state)
		{
			string path = CheckerConfiguration.GetProperty("guided_path");
			Strtok st = new Strtok(path);
			
			while(true)
			{
				string sc = st.NextToken();
				if(sc == null)
					break;
				int choice = Int32.Parse(sc);
				if(choice < state.GetForwardStateCount())
				{
					DelayedAction da = state.IsForwardReordering(choice);
					state.Forward(choice);
					if(CheckerConfiguration.DoPrintExecution)
						if(da != null)
							Console.WriteLine("Reordered: " + da.SourceInstruction.ToString());
				}
				else
				{
					Console.WriteLine("wrong path is supplied");
					break;
				}
			}
		}

		public static void Search(State st)
		{			
			if(stateSet.HasState(st))
				return;
			stateSet.AddState(st);

			int n = st.GetForwardStateCount();
			for(int i = 0; i < n; i++)
			{
				State stnew = st.Duplicate();
				stnew.Forward(i);
				if(stnew.IsDeadLock)
					stnew.Report(-1);
				Search(stnew);
			}
		}


		public static void FindGoodState(State st)
		{
			int n = st.GetForwardStateCount();
			bool visited;
			for(int i = 0; i < n; i++)
				if(st.IsForwardReordering(i) == null)
				{
					State stnew = st.Duplicate();				
					stnew.Forward(i);
					if(stnew.IsDeadLock)
						stnew.Report(-1);

					visited = stateSet.HasState(stnew);
				
					if(visited == false)
					{
						stateSet.AddState(stnew);						
						if(stnew.IsEndState == true)
						{
							foreach(int x in goodReportedValues)
								if(x == stnew.ReportedValue)
									goto nexti;
							goodReportedValues.Add(stnew.ReportedValue);
						}
						else
						{
							FindGoodState(stnew);
						}						
					}
				nexti:;
				}
		}

		public static void MakeTest(string filename)
		{
			int nVariables = 4;
			int nThreads = 2;
			int nInstructions = 7;

			StreamWriter fout = new StreamWriter(filename);
			fout.WriteLine("using System.Threading;");
			fout.WriteLine();
			fout.WriteLine("public class Test");
			fout.WriteLine("{");

			// variables
			for(int i = 0; i < nVariables; i++)
				fout.WriteLine("	int {0};", (char)('a' + i));
			
			// Threads
			for(int i = 0; i < nThreads; i++)
				MakeTestThread(fout, "Go" + i, nVariables, nInstructions);

			// Main
			fout.WriteLine("	public static void Main()");
			fout.WriteLine("	{");
			fout.WriteLine("		Test p = new Test();");
			for(int i = 0; i < nThreads; i++)
				fout.WriteLine("		Thread t{0} = new Thread(new ThreadStart(p.Go{0}));", i);
			for(int i = 0; i < nThreads; i++)
				fout.WriteLine("		t{0}.Start();", i);
			for(int i = 0; i < nThreads; i++)
				fout.WriteLine("		t{0}.Join();", i);
			fout.WriteLine("		MMC.Report(p.a);");
			fout.WriteLine("	}");
			fout.WriteLine("}");
			fout.Close();
		}

		public static void MakeTestThread(StreamWriter fout, string name, int nVariables, int nInstructions)
		{
			fout.WriteLine();
			fout.WriteLine("	void {0}()", name);
			fout.WriteLine("	{");
			for(int i = 0; i < nInstructions; i++)
			{
				fout.WriteLine("		{0}", MakeTestInstruction(nVariables));
			}
			fout.WriteLine("	}");
			fout.WriteLine("");
		}

		public static string MakeTestInstruction(int nVariables)
		{
			if(rand(2) == 0)
			{
				char c1 = (char)('a' + rand(nVariables));
				char c2 = (char)('a' + rand(nVariables));
				if(c1 != c2)
					return c1 + " = " + c2 + ";";
				else
					return MakeTestInstruction(nVariables);
			}
			else
				return (char)('a' + rand(nVariables)) + " = " + rand(nVariables) + ";";
		}

		static Random rnd = new Random();

		public static int rand(int max)
		{
			return rnd.Next(max);
		}

		// data structures for exhaustive check for optimality 
		// of putting the smallest number of barriers.

		// store the id of the state violating the property
		static int badState; 
		// keeps the list of location that was reordered at least once
		// this is the candidate location to put memory barriers.
		static Hashtable possibleLocations; 
		static int[] marker;
		static bool found;
		static int last;
		static State initialState;
		static CILInstruction[] ins;

		public static void FindBadState(State st)
		{
			bool visited;

            int t1 = 0, t2 = st.ThreadCount - 1;

			for(int i = t1; i <= t2; i++)
			{
                int[] trans = st.GetThreadTransitions(i);
                for (int j = 0; j < trans.Length; j++)
                {
                    State stnew = st.Duplicate();
                    stnew.Forward(trans[j]);
                    if (stnew.IsDeadLock)
                        stnew.Report(-1);

                    visited = stateSet.HasState(stnew);
                    if (visited == false)
                    {
                        stateSet.AddState(stnew);
                        if (stnew.IsEndState == true)
                        {
                            foreach (int x in goodReportedValues)
                                if (x == stnew.ReportedValue)
                                    goto skip1;
                            badState = stateSet.GetStateId(stnew);
                        skip1: ;
                        }
                    }

                    if (st.IsForwardReordering(trans[j]) != null)
                    {
                        CILInstruction inst = (st.IsForwardReordering(trans[j])).SourceInstruction;
                        if (possibleLocations.ContainsKey(inst.ID) == false)
                            possibleLocations.Add(inst.ID, inst);
                    }

                    if ((visited == false) && (stnew.IsEndState == false))
                        FindBadState(stnew);
                }
			}
		}

		public static void FindFirstBadState(State st)
		{
			int n = st.GetForwardStateCount();
			bool visited;
			for(int i = 0; i < n; i++)
			{
				State stnew = st.Duplicate();				
				stnew.Forward(i);
				if(stnew.IsDeadLock)
					stnew.Report(-1);

				visited = stateSet.HasState(stnew);
				if(visited == false)
				{
					stateSet.AddState(stnew);
					if(stnew.IsEndState == true)
					{
						foreach(int x in goodReportedValues)
							if(x == stnew.ReportedValue)
								goto skip1;
						badState = stateSet.GetStateId(stnew);
						return;
                    skip1: ;
					}
				}
				
				if(st.IsForwardReordering(i) != null)
				{
					CILInstruction inst = (st.IsForwardReordering(i)).SourceInstruction;
					if(possibleLocations.ContainsKey(inst.ID) == false)
						possibleLocations.Add(inst.ID, inst);
				}
			
				if((visited == false) && (stnew.IsEndState == false))
                    if(badState == -1) // break the recursion if we found bad state
                        FindFirstBadState(stnew);
			}
		}

		// . Check for optimal number of barriers needed for a program
		// by trying to put at every possible configurations starting
		// from the configuration with 1, 2, 3, ... barriers.
		// . We could not do this checking directly on the flow graph
		// because it is not possible to check whether a configuration 
		// is disabled by a particular barrier (suppose there is a branch
		// and the barrier comes before it, the transition is a reordering
		// between an instruction after the branch and the one before the 
		// branch. The barriers may even change the branch condition 
		// . So for every configuration we need to run the reachablity
		// analysis again.
		public static void CheckOptimality(State st0)
		{
			int i;

			stateSet.AddState(st0);
			FindGoodState(st0);

			Console.Write("Good end values:");
			for(i = 0; i < goodReportedValues.Count; i++)
				Console.Write(" {0}", goodReportedValues[i]);
			Console.WriteLine();			

			stateSet = new StateSet();
			stateSet.AddState(st0);
			badState = -1;
			possibleLocations = new Hashtable();
			FindBadState(st0);

			int nLocations = 0;
			IDictionaryEnumerator iter = possibleLocations.GetEnumerator();
			while(iter.MoveNext())
				if(((CILInstruction)iter.Value).ParentMethod.Name.Equals(".ctor") == false)
					nLocations++;
			
			marker = new int[nLocations];
			for(i = 0; i < marker.Length; i++)
				marker[i] = 0;

			ins = new CILInstruction[marker.Length];
			iter = possibleLocations.GetEnumerator();
			i = 0;
			while(iter.MoveNext())
				if(((CILInstruction)iter.Value).ParentMethod.Name.Equals(".ctor") == false)
					ins[i++] = (CILInstruction)iter.Value;

			found = false;
			int nBarriers = 1;
			initialState = st0;
			while(found == false)
			{
				Console.WriteLine("progress: nBarriers = {0}", nBarriers);
				last = -1;
				GenerateCases(nBarriers);
				nBarriers++;
			}

			for(i = 0; i < marker.Length; i++)
				if(marker[i] == 1)
					Console.WriteLine(ins[i].ParentMethod.Name + ":" + ins[i]);
			
		}

		static void GenerateCases(int depth)
		{
			for(int i = last + 1; i < marker.Length;i++)
			{
				if(marker[i] == 0)
				{
					marker[i] = 1;
					int oldLast = last;
					last = i;
					if(depth >= 4)
						Console.WriteLine("progress: {0}: {1} of {2}", depth, i, marker.Length);
					CILInstruction barrier = new CIL_call("xxxx", program.GetClass("[mscorlib]System.Threading.Thread").GetMethod("MemoryBarrier", "()"));
					if(ins[i].ParentMethod.InsertInstruction(ins[i].ID, barrier) == false)
						throw new Exception("Exception at insert instruction");
					if(depth == 1)
					{						
						badState = -1;
						stateSet = new StateSet();
						stateSet.AddState(initialState);							  
						FindFirstBadState(initialState);
						if(badState == -1)
						{
							found = true;
							return;
						}
					}
					else
					{
						GenerateCases(depth - 1);
						if(found)
							return;
					}
					if(ins[i].ParentMethod.RemoveInstruction(barrier.ID) == false)
						throw new Exception("Exception at remove instruction");
					last = oldLast;
					marker[i] = 0;
				}
			}
		}
	}
}
