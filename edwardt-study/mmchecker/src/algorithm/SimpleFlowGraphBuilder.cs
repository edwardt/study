using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using mmchecker.maxflow;
using mmchecker.vm;
using mmchecker.util;
using mmchecker.vm.action;

namespace mmchecker.algorithm
{
    class SimpleFlowGraphBuilder
    {
        StateSet stateSet = new StateSet();
        SmallIntSet goodEndValues = new SmallIntSet();
        Graph mfGraph = new Graph();
        FreeStack<int> searchStack = new FreeStack<int>();
        DateTime startTime;
        int whiteVerticesCount;

        // statistics
        int nAmpleFound, nNoAmpleFound;

        private void Search(State st)
        {
            searchStack.Push(stateSet.GetStateId(st));
            int t1 = 0;
            int t2 = st.GetForwardStateCount() - 1;
            if (CheckerConfiguration.DoPOR)
            {
                for (int i = 0; i < st.ThreadCount; i++)
                {
                    int[] trans = st.GetThreadTransitions(i);
                    if (trans.Length == 0)
                        goto nextThread;
                    for (int j = 0; j < trans.Length; j++)
                        if (st.IsForwardEscaping(trans[j]) == true)
                            goto nextThread;
                    // now we found a potential ample set, must check C3 condition
                    t1 = trans[0];
                    t2 = trans[trans.Length - 1];
                    for(int j = t1; j <= t2; j++)
                    {
                        State stnew = st.Duplicate();
                        stnew.Forward(j);
                        if(stateSet.HasState(stnew))
                        {
                            int k = stateSet.GetStateId(stnew);
                            for (int u = 0; u < searchStack.Count; u++)
                                if(searchStack[u] == k)
                                    goto nextThread; //unfortunately this new state is on the search stack
                        }
                    }
                    nAmpleFound++;
                    goto foundAmple;
                nextThread: ;
                }
                nNoAmpleFound++;
            foundAmple: ;
            }

            for (int i = t1; i <= t2; i++)
			{
				State stnew = st.Duplicate();				
				stnew.Forward(i);
				if(stnew.IsDeadLock)
					stnew.Report(-2);

				bool visited = stateSet.HasState(stnew);
                if (visited == false)
                    stateSet.AddState(stnew);

                if (st.IsForwardReordering(i) != null)
                    mfGraph.AddEdge(stateSet.GetStateId(st), stateSet.GetStateId(stnew), 1, st.IsForwardReordering(i));
                else
                    mfGraph.AddEdge(stateSet.GetStateId(st), stateSet.GetStateId(stnew), 100000000, null);

                if (stnew.IsEndState == true)
                {
                    mfGraph.GetVertex(stateSet.GetStateId(stnew)).PropertyValue = stnew.ReportedValue;
                    if(CheckerConfiguration.DoTiming)
                        Console.WriteLine(DateTime.Now.Subtract(startTime));
                }else
                    if (visited == false)
                        Search(stnew);
			}
            searchStack.Pop();
        }

        int[] visited;

        /// <summary>
        /// Finds all property value reachable with solid edges only
        /// </summary>
        /// <param name="v"></param>
        protected void FindGoodValues(Vertex v)
        {
            foreach(Edge e in v.forward)
                if (e.capacity > 1)
                    if (visited[e.to.ID] == 0)
                    {
                        visited[e.to.ID] = 1;
                        whiteVerticesCount++;
                        if (e.to.PropertyValue != -1)
                            goodEndValues.Add(e.to.PropertyValue);
                        FindGoodValues(e.to);
                    }
        }

        protected void FindInvalidStates()
        {
            visited = new int[stateSet.Count];
            for (int i = 0; i < visited.Length; i++)
                visited[i] = 0;
            visited[0] = 1;
            whiteVerticesCount = 0;
            FindGoodValues(mfGraph.GetVertex(0));
            Console.WriteLine("Sequential Consistency states: " + whiteVerticesCount);
            
            int virtualSink = mfGraph.NewVertex().ID;
            for (int i = 0; i < virtualSink; i++)
            {
                int pvalue = mfGraph.GetVertex(i).PropertyValue;
                if(pvalue != -1)
                    if (goodEndValues.Has(pvalue) == false)
                        mfGraph.AddEdge(i, virtualSink, 100000000, null);
            }
            mfGraph.Source = mfGraph.GetVertex(0);
            mfGraph.Sink = mfGraph.GetVertex(virtualSink);

            // Print results
            int[] tmp = goodEndValues.GetElements();
            Console.Write("Good end values: ");
            foreach(int i in tmp)
                Console.Write("{0} ", i);
            Console.WriteLine();
        }

        /// <summary>
        /// Adds weights into flowgraph to favour connections that are in a larger 
        /// group of connections disabled by the same barrier
        /// </summary>
        /// <param name="g"></param>
        public static void AddWeight(Graph g)
        {
            IEnumerator<Vertex> vertexIter = g.vertices.GetEnumerator();
            Hashtable weights = new Hashtable();
            int max_weight = 0;
            int min_weight = 0xFFFFFFF;
            while (vertexIter.MoveNext())
            {
                Vertex v = vertexIter.Current;
                for (int i = 0; i < v.forward.Count; i++)
                {
                    Edge e = (Edge)v.forward[i];
                    if (e.ExtraInfo != null)
                    {
                        CILInstruction ins = ((DelayedAction)e.ExtraInfo).SourceInstruction;
                        if (weights[ins.ID] == null)
                            weights.Add(ins.ID, 1);
                        else
                        {
                            weights[ins.ID] = (int)weights[ins.ID] + 1;
                            if (max_weight < (int)weights[ins.ID])
                                max_weight = (int)weights[ins.ID];
                        }
                    }
                }
            }

            // print out the list of instruction usage with counters
            // first copy them to array to sort
            if (weights.Count == 0)
                return;
            IntPair[] arr = new IntPair[weights.Count];
            IDictionaryEnumerator iter = weights.GetEnumerator();
            int ix = 0;
            while (iter.MoveNext())
            {
                arr[ix++] = new IntPair((int)iter.Key, (int)iter.Value);
                if (min_weight > (int)iter.Value)
                    min_weight = (int)iter.Value;
            }
            IntPair.QSortIntPair(arr, 0, weights.Count - 1);

            StreamWriter fout = new StreamWriter("weightlog.tmp");
            for (int i = 0; i < arr.Length; i++)
                fout.WriteLine("{0} {1}", arr[i].First, arr[i].Second);

            int MAX_CAPACITY = 50;
            double scale;
            if (max_weight != min_weight)
                scale = (min_weight * MAX_CAPACITY) / (max_weight - min_weight);
            else
                scale = 0;
            // assigns the weights
            vertexIter = g.vertices.GetEnumerator();
            while (vertexIter.MoveNext())
            {
                Vertex v = vertexIter.Current;
                for (int i = 0; i < v.forward.Count; i++)
                {
                    Edge e = (Edge)v.forward[i];
                    if (e.ExtraInfo != null)
                    {
                        CILInstruction ins = ((DelayedAction)e.ExtraInfo).SourceInstruction;
                        e.capacity = (int)((1.0 * max_weight / (int)weights[ins.ID] - 1) * scale) + 1;
                        fout.Write("{0} ", e.capacity);
                    }
                }
            }
            fout.Close();
        }

        public Graph BuildGraph(State st0)
        {
            startTime = DateTime.Now;
            stateSet.AddState(st0);
            Search(st0);
            FindInvalidStates();
            AddWeight(mfGraph);

            Console.WriteLine("Build graph time: {0}", DateTime.Now.Subtract(startTime));
            Console.WriteLine("State count: {0}", stateSet.Count);
            Console.WriteLine("Ample/Full: {0} {1}", nAmpleFound, nNoAmpleFound);
            return mfGraph;
        }
    }
}
