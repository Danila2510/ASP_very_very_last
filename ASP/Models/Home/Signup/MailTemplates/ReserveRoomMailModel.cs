using ASP.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.Home.Signup.MailTemplates
{
	public class ReserveMailModel
	{
		public string? User { get; set; }
		public string? Room { get; set; }
		public string? Location { get; set; }
		public string? Date { get; set; }
		public string? Price { get; set; }
		public string? GetSubject()
		{
			return "Reservation";
		}
		public string GetBody()
		{
			return $"<h2>Hello {User}. We are informing you about your  reservation{Location}, Room {Room}, price {Price}₴.</h2>" +
			$"<p style='color: orange'>Reservation date {Date}</p>";
		}
	}
}