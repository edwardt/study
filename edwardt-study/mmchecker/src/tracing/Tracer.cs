using System;
using mmchecker.vm;

namespace mmchecker.tracing
{
	/// <summary>
	/// Summary description for Tracer.
	/// </summary>
	public class Tracer
	{
		public Tracer()
		{

		}

		public static void GoTrace(CILProgram program)
		{
			State state = State.MakeInitialState(program);
			bool quit = false;
			int idx = -1;
			int fwd = -1;
			while(quit == false)
			{
				Console.Write(">");
				string command = Console.ReadLine();
				string[] st = command.Split(' ');
				switch(st[0])
				{
					case "h":
					case "help":
						PrintHelp();
						break;
					case "restart":
						state = State.MakeInitialState(program);
						Console.WriteLine("Restarted from initial state");
						break;
					case "view":
					case "v":
						Console.Write(state.ToString());
						break;
					case "thread":
					case "t":
						idx = -1;
						try { idx = Int32.Parse(st[1]);}catch(Exception){}
						if((idx >= 0) && (idx < state.ThreadCount))
						{
							Console.WriteLine("Forward state count: {0}", state.GetThreadByIndex(idx).GetForwardStateCount());
							Console.WriteLine("Next instruction: {0}", state.GetThreadByIndex(idx).PC);
							Console.WriteLine(state.GetThreadByIndex(idx).ToString());
						}else
							Console.WriteLine("Please specify a thread");
						break;
					case "c":
					case "trace":
						idx = -1;
						fwd = -1;
						try { idx = Int32.Parse(st[1]);}catch(Exception){}
						try { fwd = Int32.Parse(st[2]);}catch(Exception){}
						if((idx >= 0) && (idx < state.ThreadCount) &&
							(fwd >= 0) && (fwd < state.GetThreadByIndex(idx).GetForwardStateCount()))
						{				
							int tmp = 0;
							for(int i = 0; i < idx; i++)
								tmp += state.GetThreadByIndex(i).GetForwardStateCount();
							state.Forward(tmp + fwd);
                            Console.Write(state.ToString());
                        }
						else
							Console.WriteLine("trace thread_index forward_choice");
						break;
					case "forward":
					case "f":
						int j;
						for(j = 1; j < st.Length; j++)
						{
							fwd = -1;
							try { fwd = Int32.Parse(st[1]);}
							catch(Exception){}
							if((fwd >= 0) && (fwd < state.GetForwardStateCount()))
							{
								state.Forward(fwd);
							}
							else
							{
								if(j == 1)
									Console.WriteLine("forward [choice]*");
								break;
							}
						}						
						break;
					case "q":
					case "quit":
					case "exit":
						quit = true;
						break;
					default:
						Console.WriteLine("Unknown command, please type command help");
						break;
				}
				PrintSeparator();
			}
		}

		static void PrintHelp()
		{
			Console.WriteLine("[v]iew: view current state details");
			Console.WriteLine("[t]hread x: view thread x details");
			Console.WriteLine("tra[c]e x y: run choice y on thread x");
			Console.WriteLine("[f]orward x: forward choice x");
			Console.WriteLine("[r]estart: start from initial state");
		}

		static void PrintSeparator()
		{
			Console.WriteLine("------------------------------------------------");
		}
	}
}
