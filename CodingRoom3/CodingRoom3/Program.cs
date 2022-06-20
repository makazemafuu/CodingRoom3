using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SelectionCandidat
{
	class Program
	{
		static void Main(string[] args)
		{
			/* Get data from input file */
			var data = new List<String>();
			string line;

			while ((line = Console.ReadLine()) != null)
			{
				data.Add(line);
			}

			// ADD YOUR CODE HERE
			int grille = int.Parse(data[0]);

			int[][] map = new int[grille][];
			for (int k = 0; k < grille; k++)
				map[k] = new int[grille];

			int coins = int.Parse(data[3]);
			int remainingCoins = coins;
			int remainingInstructions = int.Parse(data[2]);
			string[] playerPosition = data[1].Split(',');
			string instructions;
			int coordX = int.Parse(playerPosition[0]);
			int coordY = int.Parse(playerPosition[1]);

			// Setting up the needed variables to place every coin on the map
			int coinsPositionX;
			int coinsPositionY;
			string[] coinsPosition;

			// Each following list will have for value a path to a direction in first string, then the location of each coin in it
			// And a 1 at the end if there is a piece that can be reached with one move

			List<String> north = new List<string>();
			List<String> east = new List<string>();
			List<String> south = new List<string>();
			List<String> west = new List<string>();
			List<String> northEast = new List<string>();
			List<String> southEast = new List<string>();
			List<String> southWest = new List<string>();
			List<String> northWest = new List<string>();

			// Setting up all the coins on the map

			for (int i = 0; i < remainingCoins; i++)
			{
				coinsPosition = data[i + 4].Split(',');
				coinsPositionX = int.Parse(coinsPosition[0]);
				coinsPositionY = int.Parse(coinsPosition[1]);
				map[(grille - 1) - coinsPositionX][coinsPositionY] = 2;
			}
			// Parsing and applying the move instructions

			MapBuild(grille, ref map, coordX, coordY, ref remainingCoins);

			// This while applies the move the hero has to make

			while (remainingInstructions-- > 0)
			{
				instructions = MagicCompass(coordX, coordY, grille, map, ref north, ref east, ref south, ref west, ref northEast, ref southEast, ref southWest, ref northWest);
				if (instructions == "u")
					coordY += 1;
				else if (instructions == "ur")
				{
					coordX += 1;
					coordY += 1;
				}
				else if (instructions == "ul")
				{
					coordY += 1;
					coordX -= 1;
				}
				else if (instructions == "l")
					coordX -= 1;
				else if (instructions == "r")
					coordX += 1;
				else if (instructions == "d")
					coordY -= 1;
				else if (instructions == "dr")
				{
					coordX += 1;
					coordY -= 1;
				}
				else if (instructions == "dl")
				{
					coordX -= 1;
					coordY -= 1;
				}
				Console.WriteLine($"{coordX},{coordY}");
				MapBuild(grille, ref map, coordX, coordY, ref remainingCoins);

				//DisplayMap(grille, map);

				if (remainingCoins <= 0)
					break;
			}
			Console.Write(coins - remainingCoins);
		}

		// Method to set the map up to date
		private static void MapBuild(int grille, ref int[][] map, int coordX, int coordY, ref int remainingCoins)
		{
			for (int i = 0; i <= grille - 1; i++)
			{
				for (int j = 0; j <= grille - 1; j++)
				{
					if (grille - 1 - coordX == i && coordY == j)
					{
						if (map[i][j] == 2)
							remainingCoins--;
						map[i][j] = 1;
					}
					else if (map[i][j] == 2)
						continue;
					else
						map[i][j] = 0;
				}
			}
		}

		// Method to display the map with a 1 at the hero location, a 2 on every coin and a 0 for the empty place
		private static void DisplayMap(int grille, int[][] map)
		{
			for (int y = grille - 1; y >= 0; y--)
			{
				for (int x = 0; x <= grille - 1; x++)
				{
					Console.Write($"{map[grille - 1 - x][y]} ");
				}
				Console.Write('\n');

			}
			Console.WriteLine("--------------");
		}

		// Method that will determinate the better move to do
		private static string MagicCompass(int coordX, int coordY, int grille, int[][] map, ref List<String> north, ref List<String> east, ref List<String> south, ref List<String> west, ref List<String> northEast, ref List<String> southEast, ref List<String> southWest, ref List<String> northWest)
		{
			List<List<string>> waylist = new List<List<string>>(8);
			int coins = closeCoins(map, coordX, coordY, grille);

			waylist.Add(north = GetNorthCoins(coordX, coordY, grille, map));
			waylist.Add(east = GetEastCoins(coordX, coordY, grille, map));
			waylist.Add(south = GetSouthCoins(coordX, coordY, grille, map));
			waylist.Add(west = GetWestCoins(coordX, coordY, grille, map));
			waylist.Add(northEast = GetNorthEastCoins(coordX, coordY, grille, map));
			waylist.Add(southEast = GetSouthEastCoins(coordX, coordY, grille, map));
			waylist.Add(southWest = GetSouthWestCoins(coordX, coordY, grille, map));
			waylist.Add(northWest = GetNorthWestCoins(coordX, coordY, grille, map));

			// If there is only one piece close then I move on it without using the magic compass
			if (coins == 1)
			{
				foreach (List<string> way in waylist.ToList())
				{
					if (!String.Equals(way[way.Count - 1], "1"))
						waylist.Remove(way);
				}
				return waylist[0][0];
			}
			// If there is more than one coin reachable in one move, then I remove all the other paths
			if (coins > 1)
				foreach (List<string> way in waylist.ToList())
				{
					if (!String.Equals(way[way.Count - 1], "1"))
						waylist.Remove(way);
				}
			while (waylist.Count > 1)
			{
				if (waylist[0].Count > waylist[1].Count)
					waylist.RemoveAt(1);
				else
					waylist.RemoveAt(0);
			}

			// I sort the ways, the one with the lowest numbers of coins in it is removed 
			// I return the string containing the good way

			return waylist[0][0];
		}

		// The following method will return the number of coins reachable by one move to the hero
		private static int closeCoins(int[][] map, int coordX, int coordY, int grille)
		{
			int coins = 0;
			int x = coordX;
			int y = coordY + 1;
			for (int i = 0; i < 8; i++)
			{
				if (x < grille && x >= 0 && y < grille && y >= 0)
				{
					if (map[grille - 1 - x][y] == 2)
						coins++;
				}
				if (x < coordX + 1 && y == coordY + 1)
					x++;
				else if (x == coordX + 1 && y > coordY - 1)
					y--;
				else if (x > coordX - 1 && y == coordY - 1)
					x--;
				else if (y < coordY + 1 && x == coordX - 1)
					y++;
				if (x == coordX - 1 && y == coordY + 1)
				{
					coordX = x;
					coordY = y;
				}
			}
			return coins;
		}
		// All the methods below build a list that contain the position of each coins in their direction, then add a 1 at the end if there is a coin reachable by one move
		private static List<String> GetNorthCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordY += 1;
			goldCoins.Add("u");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = grille - 1; y >= coordY; y--)
			{
				for (int x = 0; x <= grille - 1; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
		private static List<String> GetEastCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordX += 1;
			goldCoins.Add("r");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = grille - 1; y >= 0; y--)
			{
				for (int x = coordX; x <= grille - 1; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}

		private static List<String> GetSouthCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordY -= 1;
			goldCoins.Add("d");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = coordY; y >= 0; y--)
			{
				for (int x = 0; x <= grille - 1; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
		private static List<String> GetWestCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordX -= 1;
			goldCoins.Add("l");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = grille - 1; y >= 0; y--)
			{
				for (int x = 0; x <= coordX; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
		private static List<String> GetNorthEastCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordY += 1;
			coordX += 1;
			goldCoins.Add("ur");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = grille - 1; y >= coordY; y--)
			{
				for (int x = coordX; x <= grille - 1; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
		private static List<String> GetSouthEastCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordY -= 1;
			coordX += 1;
			goldCoins.Add("dr");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = coordY; y >= 0; y--)
			{
				for (int x = coordX; x <= grille - 1; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
		private static List<String> GetSouthWestCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordY -= 1;
			coordX -= 1;
			goldCoins.Add("dl");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = coordY; y >= 0; y--)
			{
				for (int x = 0; x <= coordX; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
		private static List<String> GetNorthWestCoins(int coordX, int coordY, int grille, int[][] map)
		{
			List<String> goldCoins = new List<String>();
			coordY += 1;
			coordX -= 1;
			goldCoins.Add("ul");
			if (coordY >= grille || coordX >= grille || coordX < 0 || coordY < 0)
				return goldCoins;
			for (int y = grille - 1; y >= coordY; y--)
			{
				for (int x = 0; x <= coordX; x++)
				{
					if (map[grille - 1 - x][y] == 2)
						goldCoins.Add($"{x},{y}");
				}
			}
			if (map[grille - 1 - coordX][coordY] == 2)
				goldCoins.Add("1");
			return (goldCoins);
		}
	}
}