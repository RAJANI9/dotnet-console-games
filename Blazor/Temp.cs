﻿using System.Diagnostics;

namespace Blazor;

public static class Temp
{
	public static async Task Run(Action stateHasChanged)
	{
		if (Console.refresh is null)
		{
			Console.refresh = () =>
			{
				stateHasChanged();
			};
		}

		Random random = new();

		const string menu = @"
  Quick Draw

  Face your opponent and wait for the signal. Once the
  signal is given, shoot your opponent by pressing [space]
  before they shoot you. It's all about your reaction time.

  Choose Your Opponent:
  [1] Easy....1000 milliseconds
  [2] Medium...500 milliseconds
  [3] Hard.....250 milliseconds
  [4] Harder...125 milliseconds
  [escape] give up";

		const string wait = @"
  Quick Draw



              _O                          O_            
             |/|_          wait          _|\|           
             /\                            /\           
            /  |                          |  \          
  ------------------------------------------------------";

		const string fire = @"
  Quick Draw

                         ********                       
                         * FIRE *                       
              _O         ********         O_            
             |/|_                        _|\|           
             /\          spacebar          /\           
            /  |                          |  \          
  ------------------------------------------------------";

		const string loseTooSlow = @"
  Quick Draw



                                        > ╗__O          
           //            Too Slow           / \         
          O/__/\         You Lose          /\           
               \                          |  \          
  ------------------------------------------------------";

		const string loseTooFast = @"
  Quick Draw



                         Too Fast       > ╗__O          
           //           You Missed          / \         
          O/__/\         You Lose          /\           
               \                          |  \          
  ------------------------------------------------------";

		const string win = @"
  Quick Draw



            O__╔ <                                      
           / \                               \\         
             /\          You Win          /\__\O        
            /  |                          /             
  ------------------------------------------------------";


		try
		{
			while (true)
			{
				Console.Clear();
				Console.WriteLine(menu);
				TimeSpan? requiredReactionTime = null;
				while (requiredReactionTime is null)
				{
					Console.CursorVisible = false;
					switch ((await Console.ReadKey(true)).Key)
					{
						case ConsoleKey.D1 or ConsoleKey.NumPad1: requiredReactionTime = TimeSpan.FromMilliseconds(1000); break;
						case ConsoleKey.D2 or ConsoleKey.NumPad2: requiredReactionTime = TimeSpan.FromMilliseconds(0500); break;
						case ConsoleKey.D3 or ConsoleKey.NumPad3: requiredReactionTime = TimeSpan.FromMilliseconds(0250); break;
						case ConsoleKey.D4 or ConsoleKey.NumPad4: requiredReactionTime = TimeSpan.FromMilliseconds(0125); break;
						case ConsoleKey.Escape: return;
					}
				}
				Console.Clear();
				TimeSpan signal = TimeSpan.FromMilliseconds(random.Next(5000, 35000));
				Console.WriteLine(wait);
				Stopwatch stopwatch = new();
				stopwatch.Restart();
				bool tooFast = false;
				while (stopwatch.Elapsed < signal && !tooFast)
				{
					if (Console.KeyAvailable && (await Console.ReadKey(true)).Key is ConsoleKey.Spacebar)
					{
						tooFast = true;
					}
				}
				Console.Clear();
				Console.CursorVisible = false;
				Console.WriteLine(fire);
				stopwatch.Restart();
				bool tooSlow = true;
				TimeSpan reactionTime = default;
				while (!tooFast && stopwatch.Elapsed < requiredReactionTime && tooSlow)
				{
					if (Console.KeyAvailable && (await Console.ReadKey(true)).Key is ConsoleKey.Spacebar)
					{
						tooSlow = false;
						reactionTime = stopwatch.Elapsed;
					}
				}
				Console.Clear();
				Console.WriteLine(
					tooFast ? loseTooFast :
					tooSlow ? loseTooSlow :
					$"{win}{Environment.NewLine}  Reaction Time: {reactionTime.TotalMilliseconds} milliseconds");
				Console.WriteLine("  Play Again [enter] or quit [escape]?");
				Console.CursorVisible = false;
			GetEnterOrEscape:
				switch ((await Console.ReadKey(true)).Key)
				{
					case ConsoleKey.Enter: break;
					case ConsoleKey.Escape: return;
					default: goto GetEnterOrEscape;
				}
			}
		}
		finally
		{
			Console.Clear();
			Console.CursorVisible = true;
			Console.Write("Quick Draw was closed.");
		}
	}

	public static class Console
	{
		public static Action? refresh;

		public static Queue<ConsoleKeyInfo> inputBuffer = new();

		public static string state = string.Empty;

		public static void Clear()
		{
			state = string.Empty;
			refresh?.Invoke();
			//await Task.Yield();
		}

		public static void Write(object o)
		{
			state += o.ToString();
			refresh?.Invoke();
			//await Task.Yield();
		}

		public static void WriteLine()
		{
			state += Environment.NewLine;
			refresh?.Invoke();
			//await Task.Yield();
		}

		public static void WriteLine(object o)
		{
			state += o.ToString() + Environment.NewLine;
			refresh?.Invoke();
			//await Task.Yield();
		}

		public static async Task<ConsoleKeyInfo> ReadKey(bool capture)
		{
			//return new('\0', ConsoleKey.Escape, false, false, false);

			while (!KeyAvailable)
			{
				await Task.Delay(10);
				await Task.Yield();
				refresh?.Invoke();
			}
			return inputBuffer.Dequeue();
		}

		public static bool _cursorVisible;

		public static bool CursorVisible
		{
			get => _cursorVisible;
			set => _cursorVisible = value;
		}

		public static bool _keyAvailable;

		public static bool KeyAvailable
		{
			get => inputBuffer.Count > 0;
		}
	}
}
