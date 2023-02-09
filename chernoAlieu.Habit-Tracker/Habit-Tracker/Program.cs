﻿using Microsoft.Data.Sqlite;
using System.Globalization;
class Program
{
	static string connectionString = @"Data Source=habit-Tracker.db";

	static void Main(string[] args)
	{
		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();

			tableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS drinking_water (
								Id INTEGER PRIMARY KEY AUTOINCREMENT,
								Date TEXT,
								Quantity INTEGER
								)";
			tableCmd.ExecuteNonQuery();
			connection.Close();
		}
		GetUserInput();
	}

	static void GetUserInput()
	{
		Console.Clear();
		var closeApp = false;
		while (closeApp == false)
		{
			Console.WriteLine("\n\nMAIN MENU");
			Console.WriteLine("\nWhat would you like to do?");
			Console.WriteLine("\nType 0 to Close Application.");
			Console.WriteLine("Type 1 to View all Records");
			Console.WriteLine("Type 2 to Insert Record");
			Console.WriteLine("Type 3 to Delete Record");
			Console.WriteLine("Type 4 to Update Record");
			Console.WriteLine("-------------------------------------");

			string commandInput = Console.ReadLine();

			switch (commandInput)
			{
				case "0":
					Console.WriteLine("\nGoodbye!\n");
					closeApp = true;
					Environment.Exit(0);
					break;
				case "1":
					GetAllRecords();
					break;
				case "2":
					Insert();
					break;
				case "3":
					Delete();
					break;
				case "4":
					Update();
					break;
				default:
					Console.WriteLine("\nInvalid Command. Please type a number from 0 to 4.\n");
					break;
			}
		}
	}
	private static void Insert()
	{
		string date = GetDateInput();

		int quantity =
			GetNumberInput(@"\n\nPlease insert number of glasses or other measure of your choice " +
						   "(no decimals allowed)\n\n");

		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText = $"INSERT INTO drinking_water(date, quantity) VALUES('{date}',{quantity})";

			tableCmd.ExecuteNonQuery();
			connection.Close();
		}
	}
	internal static string GetDateInput()
	{
		Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.");

		string dateInput = Console.ReadLine();

		if (dateInput == "0") GetUserInput();
		while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
		{
			Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Type 0 to return to main menu or try again:\n\n");
			dateInput = Console.ReadLine();
		}
		return dateInput;
	}
	internal static int GetNumberInput(string message)
	{
		Console.WriteLine(message);
		string numberInput = Console.ReadLine();

		if (numberInput == "0") GetUserInput();

		while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
		{
			Console.WriteLine("\n\nInvalid number. Try again");
			numberInput = Console.ReadLine();
		}

		int finalInput = Convert.ToInt32(numberInput);
		return finalInput;
	}

	private static void GetAllRecords()
	{
		Console.Clear();
		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText =
				$"SELECT * FROM drinking_water ";

			List<DrinkingWater> tableData = new();
			SqliteDataReader reader = tableCmd.ExecuteReader();

			if (reader.HasRows)
			{
				while (reader.Read())
				{
					tableData.Add(
						new DrinkingWater
						{
							Id = reader.GetInt32(0),
							Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
							Quantity = reader.GetInt32(2)
						});
				}
			}
			else
			{
				Console.WriteLine("No rows found");
			}
			connection.Close();
			Console.WriteLine("----------------------------------------------\n");
			foreach (var drinkingWater in tableData)
			{
				Console.WriteLine($"{drinkingWater.Id} - {drinkingWater.Date.ToString("dd-MM-yyyy")} - Quantity: {drinkingWater.Quantity}");
			}
			Console.WriteLine("----------------------------------------------\n");
		}
	}
	private static void Delete()
	{
		Console.Clear();
		GetAllRecords();

		var recordId = GetNumberInput("\n\nPlease take the Id of the record you" +
		                              " want to delete or type 0 to go back to main menu");

		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";
			int rowCount = tableCmd.ExecuteNonQuery();

			if (rowCount == 0)
			{
				Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist. \n\n");
				Delete();
			}
		}
		Console.WriteLine($"\n\nRecord with Id {recordId} was deleted.\n\n");
		GetUserInput();
	}

	private static void Update()
	{
		Console.Clear();
		GetAllRecords();
		var recordId = GetNumberInput("\n\nPlease type the Id of the record you would like to update." +
		                              "Type 0 to go back to the main menu");
		using (var connection = new SqliteConnection(connectionString))
		{
			connection.Open();
			var checkCmd = connection.CreateCommand();
			checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
			int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());
			if (checkQuery == 0)
			{
				Console.WriteLine($"\n\nRecord with Id {recordId} doesn't exist.\n\n");
				connection.Close();
				Update();
			}
			string date = GetDateInput();
			int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your " +
			                              "choice(no decimals allowed)");
			var tableCmd = connection.CreateCommand();
			tableCmd.CommandText =
				$"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";
		}
	}
}
public class DrinkingWater
{
	public int Id { get; set; }
	public DateTime Date { get; set; }
	public int Quantity { get; set; }
}