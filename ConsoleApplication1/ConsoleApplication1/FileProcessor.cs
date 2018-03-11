using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
	class FileProcessor
	{
		// Defined constants for column names
		private const string PROJECT = "Project";
		private const string DESC = "Description";
		private const string ST_DATE = "Start date";
		private const string CAT = "Category";
		private const string RESP = "Responsible";
		private const string AMT = "Savings amount";
		private const string CURRENCY = "Currency";
		private const string COMPLEXITY = "Complexity";

		// Defined constants for complexities
		private const string SIMPLE = "Simple";
		private const string MOD = "Moderate";
		private const string HAZARD = "Hazardous";

		// Defined constants for values & patterns
		private const char VALUE_SEP = '\t';
		private const string NULL_VAL = "NULL";

		// the columns we expect
		string[] lstcolumns = { PROJECT, DESC, ST_DATE, CAT, RESP, AMT, CURRENCY, COMPLEXITY };
		// record of the order for each column
		Dictionary<string, int> dictColoumnOrder;
		// list of valid complexities
		string[] lstComplexities = { SIMPLE, MOD, HAZARD };
		// list of items from file
		List<TransactionItem> transactionList;
		// The root directory of the application
		string rootDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
		// file to be processed
		string filename = "";
		// flag for sort by date
		bool sortByDate = false;
		// if != "" then filter by that project
		string filterProjectId = "";

		/// <summary>
		/// processFile stores the options, reads in a source file and controls operation
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="sortByDate"></param>
		/// <param name="filterProjectId"></param>
		/// <returns></returns>
		public bool processFile(string filename, bool sortByDate, string filterProjectId)
		{
			this.filename = filename;
			this.sortByDate = sortByDate;
			this.filterProjectId = filterProjectId;

			transactionList = new List<TransactionItem>();
			dictColoumnOrder = new Dictionary<string, int>();
			bool result = true;

			string fileContents = getFileContents(filename);
			if (fileContents.Length > 0)
			{
				string[] fileLines = fileContents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

				foreach (string line in fileLines)
				{
					if (checkLineOK(line))
					{
						addTransaction(line);
					}
				}
				prepareList();
				writeResultToConsole();
				writeNewFile();
			}
			else
			{
				writeError("File not found or was empty.");
				result = false;
			}

			return result;
		}

		#region Input Processing

		/// <summary>
		/// Get the contents of the source file
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		private string getFileContents(string filename)
		{
			if (checkRelativeFileExists(filename))
			{
				return readRelativeTextFile(filename);
			}
			return "";
		}

		/// <summary>
		/// Check if a file relative to the exe exists
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		private bool checkRelativeFileExists(string filename)
		{
			string fullPath = rootDir + Path.DirectorySeparatorChar + filename;
			return File.Exists(fullPath);
		}

		/// <summary>
		/// Read the contents of a file relative to the exe
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		private string readRelativeTextFile(string filename)
		{
			string fullPath = rootDir + Path.DirectorySeparatorChar + filename;
			return File.ReadAllText(fullPath, Encoding.GetEncoding("iso-8859-1")); // ISSUE using UTF8 mangled the wide chars
		}

		/// <summary>
		/// Read the header line and record the column order
		/// </summary>
		/// <param name="line"></param>
		private void setColumnOrder(string line)
		{
			string[] lstColumnNames = line.Split(VALUE_SEP);
			int columnNum = 0;
			foreach (string columnName in lstColumnNames)
			{
				if (lstcolumns.Contains(columnName))
				{
					dictColoumnOrder.Add(columnName, columnNum++);
				}
			}
			if (dictColoumnOrder.Count() != lstcolumns.Count())
			{
				string msg = "Incorrect number of column titles found." + Environment.NewLine + "Expecting : " + lstcolumns.ToString();
				writeError(msg);
				quit(msg);
			}
		}

		/// <summary>
		/// Check if the source line passes tests
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		private bool checkLineOK(string line)
		{
			string[] parts = line.Split(VALUE_SEP);

			// test for lines that are ignored
			if (line.Length == 0 || line[0] == '#')
				return false;

			// check proper number of items
			if (parts.Count() != 8)
			{
				return false;
			}

			// if it's the header line then set the column order
			if (lstcolumns.Contains(parts[0]))
			{
				setColumnOrder(line);
				return false;
			}
			// we should have a column order by now
			if (dictColoumnOrder.Count == 0)
			{
				quit("Header row not found before content row.");
			}

			// check that the date parses
			if (!checkDate(parts[dictColoumnOrder[ST_DATE]]))
			{
				return false;
			}
			// check amount
			if (!checkAmout(parts[dictColoumnOrder[AMT]]))
			{
				return false;
			}
			// check complexity
			if (!checkValueInList(COMPLEXITY, parts[dictColoumnOrder[COMPLEXITY]], lstComplexities))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Verify date parses
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		private bool checkDate(string date)
		{
			// check date
			try
			{
				DateTime startDate = DateTime.Parse(date);
			}
			catch (Exception ex)
			{
				writeException("Error parsing date:", ex);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Check that the amount is actually a double
		/// </summary>
		/// <param name="amt">string amount to check</param>
		/// <returns></returns>
		private bool checkAmout(string amt)
		{
			// NULL is OK
			if (amt.Equals(NULL_VAL))
				return true;

			try
			{
				double.Parse(amt);
			}
			catch (Exception ex)
			{
				writeException("Error parsing amout:", ex);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Check that the value passed in exists in the list passed in
		/// </summary>
		/// <param name="name">The name of the item used in error report if failed</param>
		/// <param name="value">The value to look for</param>
		/// <param name="list">The list to check</param>
		/// <returns></returns>
		private bool checkValueInList(string name, string value, string[] list)
		{
			if (!list.Contains(value))
			{
				writeError("Error: Unexpected value " + value + " in " + name + ".");
				return false;
			}
			return true;
		}

		#endregion

		#region Output Processing

		/// <summary>
		/// Add a transaction to the list.
		/// </summary>
		/// <param name="line">Source string read from file</param>
		private void addTransaction(string line)
		{
			transactionList.Add(createTransaction(line));
		}

		/// <summary>
		/// Create a transaction from the source line. All checking of values is already done at this point.
		/// </summary>
		/// <param name="line">Source line read from the file</param>
		/// <returns></returns>
		private TransactionItem createTransaction(string line)
		{
			string[] parts = line.Split(VALUE_SEP);
			string project = parts[dictColoumnOrder[PROJECT]];
			string desc = parts[dictColoumnOrder[DESC]];
			DateTime startDate;
			DateTime.TryParse(parts[dictColoumnOrder[ST_DATE]], out startDate);
			string category = parts[dictColoumnOrder[CAT]];
			string responsible = parts[dictColoumnOrder[RESP]];
			string amount = parts[dictColoumnOrder[AMT]];
			string currency = parts[dictColoumnOrder[CURRENCY]];
			string complexity = parts[dictColoumnOrder[COMPLEXITY]];

			return new TransactionItem(project, desc, startDate,
				category, responsible, amount, currency, complexity);

		}

		/// <summary>
		/// Create the header for the output
		/// </summary>
		/// <returns></returns>
		private string createHeader()
		{
			string[] header = new string[lstcolumns.Count()];
			foreach (string column in lstcolumns)
			{
				header[dictColoumnOrder[column]] = lstcolumns[dictColoumnOrder[column]];
			}
			return string.Join(VALUE_SEP.ToString(), header);
		}

		/// <summary>
		/// Prepare the list for output based on the sort & filter options passed in.
		/// </summary>
		private void prepareList()
		{
			// filter the list if we have a project Id
			if (!filterProjectId.Equals(""))
			{
				transactionList = transactionList.FindAll(x => x.project.Equals(filterProjectId));
			}
			// sort the list if flag was set
			if (sortByDate)
			{
				transactionList.Sort((x, y) => x.startDate.CompareTo(y.startDate));
			}
		}

		/// <summary>
		/// Write the result lines and header out to the console.
		/// </summary>
		private void writeResultToConsole()
		{
			Console.WriteLine(createHeader());
			// repeat with each transaction, get the values from the item in column order
			foreach (TransactionItem item in transactionList)
			{
				Console.WriteLine(getValuesInColumnOrder(item));
			}
		}

		/// <summary>
		/// Output the results to a new file with "_out" appended to the filename.
		/// </summary>
		private void writeNewFile()
		{
			StringBuilder sb = new StringBuilder("");
			sb.AppendLine(createHeader());
			// repeat with each transaction, get the values from the item in column order
			foreach (TransactionItem item in transactionList)
			{
				sb.AppendLine(getValuesInColumnOrder(item));
			}
			string fileExt = Path.GetExtension(filename);
			writeRelativeTextFile(filename.Replace(fileExt, "_out" + fileExt), sb.ToString());
		}

		/// <summary>
		/// Write out the text to the filename relative to the exe
		/// </summary>
		/// <param name="filename">Filename of the output</param>
		/// <param name="text">Contents to write</param>
		private void writeRelativeTextFile(string filename, string text)
		{
			string fullpath = rootDir + Path.DirectorySeparatorChar + filename;
			File.WriteAllText(fullpath, text, Encoding.UTF8);
		}

		/// <summary>
		/// Get the values of a transaction in the order according to the header.
		/// </summary>
		/// <param name="item">The transaction item to process</param>
		/// <returns></returns>
		private string getValuesInColumnOrder(TransactionItem item)
		{
			string[] values = new string[lstcolumns.Count()];
			foreach (string column in lstcolumns)
			{
				values[dictColoumnOrder[column]] = getValueForColumn(item, lstcolumns[dictColoumnOrder[column]]);
			}
			return string.Join(VALUE_SEP.ToString(), values);
		}

		/// <summary>
		/// Get the string value of a property of the specified item.
		/// </summary>
		/// <param name="item">The transaction item to process</param>
		/// <param name="column">The column name to retrieve</param>
		/// <returns></returns>
		private string getValueForColumn(TransactionItem item, string column)
		{
			// perhaps column should be an enum... Hmmm
			switch (column)
			{
				case PROJECT:
					return item.project;

				case DESC:
					return item.description;

				case ST_DATE:
					return item.startDate.ToString("yyyy-MM-DD HH:MM:SS.sss");

				case CAT:
					return item.category;

				case RESP:
					return item.responsible;

				case AMT:
					return item.amount.ToString();

				case CURRENCY:
					return item.currency;

				case COMPLEXITY:
					return item.complexity;

			}
			// returning from here MUST BE AN ERROR
			quit("Unsupported value requested from transaction item.");

			return ""; // placating compiler
		}

		#endregion

		#region Utility

		/// <summary>
		/// Explode an exception (drilling into all inner exceptions) and write the messages to string.
		/// </summary>
		/// <param name="msg">Base message to provide with the exception message</param>
		/// <param name="ex">Exception to process</param>
		private void writeException(string msg, Exception ex)
		{
			string strOutput = msg + Environment.NewLine;
			while (ex != null)
			{
				strOutput += ex.Message + Environment.NewLine;
				ex = ex.InnerException;
			}
			writeError(strOutput);
		}

		/// <summary>
		/// Write an error message to the designated output (currently console).
		/// </summary>
		/// <param name="msg">Message to write</param>
		private void writeError(string msg)
		{
			Console.WriteLine(msg);
		}

		/// <summary>
		/// Throw an exception causing the app to quit.
		/// </summary>
		/// <param name="msg"></param>
		private void quit(string msg)
		{
			throw new Exception("Fatal Error in processing file: " + msg);
		}

		#endregion
	}
}
