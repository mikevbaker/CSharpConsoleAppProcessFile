using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	class Program
	{
		static string filename = "";
		static bool sortByDate = false;
		static string project = "";

		/// <summary>
		/// Main entry point. Usage: ConsoleApplication1.exe file=myfile.txt [SortByStartDate] [Project=1]
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			if (checkArgs(args))
			{
				try
				{
					FileProcessor fileProcessor = new FileProcessor();
					fileProcessor.processFile(filename, sortByDate, project);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			Console.WriteLine("Finished");
			Console.ReadKey();
		}

		static bool checkArgs(string[] args)
		{
			bool argsOK = true;
			// verify minimum arguments
			// ISSUE - Requires File argument to be first even though it could be made to work with at least one 'File' argument
			if (args == null || args.Count() == 0 || !args[0].ToUpper().StartsWith("FILE"))
				argsOK = false;

			// get the arguments
			if (argsOK)
				argsOK = checkArg(args[0]);

			if (argsOK && args.Count() > 1)
				argsOK = checkArg(args[1]);

			if (argsOK && args.Count() > 2)
				argsOK = checkArg(args[2]);

			if (!argsOK)
			{
				Console.WriteLine("");
				Console.WriteLine("ERROR!!");
				Console.WriteLine("ERROR!!");
				Console.WriteLine("Usage: ConsoleApplication1.exe file=myfile.txt [SortByStartDate] [Project=1]");
				Console.WriteLine("");
			}

			return argsOK;
		}

		/// <summary>
		/// Process an arg and find out what option it specifies
		/// </summary>
		/// <param name="value">The argument including the arg name and value</param>
		static bool checkArg(string value)
		{
			bool result = true;

			if (value.ToUpper().StartsWith("FILE"))
				filename = getArgVal(value);

			else if (value.ToUpper().Equals("SORTBYSTARTDATE"))
				sortByDate = true;

			else if (value.ToUpper().StartsWith("PROJECT"))
				project = getArgVal(value);

			else // none of the args matched so this one is bad
				result = false;

			return result;
		}

		static string getArgVal(string arg)
		{
			// prevent crash by ensuring proper count
			if (arg.Split('=').Count() > 1)
				return arg.Split('=')[1];
			else
				return "";
		}
	}


}
