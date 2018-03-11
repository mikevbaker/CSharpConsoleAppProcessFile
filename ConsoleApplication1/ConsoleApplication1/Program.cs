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
#if DEBUG
			DateTime startTime = DateTime.Now;
#endif
			if (args == null || args.Count() == 0 || !args[0].ToUpper().StartsWith("FILE"))
			{
				Console.WriteLine("ERROR!!");
				Console.WriteLine("ERROR!!");
				Console.WriteLine("Usage: ConsoleApplication1.exe file=myfile.txt [SortByStartDate] [Project=1]");
				Console.WriteLine("");
				Console.WriteLine("");
				return;
			}

			// get the arguments
			checkArg(args[0]);
			if (args.Count() > 1)
				checkArg(args[1]);
			if (args.Count() > 2)
				checkArg(args[2]);

			// we could trap for missing filename here but it is checked in FileProcessor
			try
			{
				FileProcessor fileProcessor = new FileProcessor();
				fileProcessor.processFile(filename, sortByDate, project);
			} catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
#if DEBUG
			DateTime endTime = DateTime.Now;
			Console.WriteLine("consumed " + (endTime - startTime).Milliseconds);
			Console.WriteLine("Press enter to close...");
			Console.ReadLine();
#endif
		}

		/// <summary>
		/// Process an arg and find out what option it specifies
		/// </summary>
		/// <param name="value">The argument including the arg name and value</param>
		static void checkArg(string value)
		{
			if (value.ToUpper().StartsWith("FILE"))
				filename = getArgVal(value);

			if (value.ToUpper().Equals("SORTBYSTARTDATE"))
				sortByDate = true;

			if (value.ToUpper().StartsWith("PROJECT"))
				project = getArgVal(value);

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
