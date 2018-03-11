using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
	public class TransactionItem
	{
		public string project { get; set; }
		public string description { get; set; }
		public DateTime startDate { get; set; }
		public string category { get; set; }
		public string responsible { get; set; }
		public string amount { get; set; }
		public string currency { get; set; }
		public string complexity { get; set; }

		private TransactionItem() { }

		public TransactionItem(string project, string description, 
			DateTime startDate, string category, string responsible,
			string amount, string currency, string complexity)
		{
			this.project = project;
			this.description = description;
			this.startDate = startDate;
			this.category = category;
			this.responsible = responsible;
			this.amount = amount;
			this.currency = currency;
			this.complexity = complexity;
		}
	}
}
