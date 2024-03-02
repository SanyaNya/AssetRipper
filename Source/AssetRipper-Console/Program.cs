using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;

namespace AssetRipper.Console
{
	internal class Program
	{
		private static List<string> inputPaths = new List<string>();
		private static string outputPath;
		private static LibraryConfiguration settings;

		private static void Main(string[] args)
		{
			ParseArgs(args);
			if (inputPaths.Count == 0)
			{
				System.Console.WriteLine("Please pass in an asset");
				System.Console.ReadLine();
				return;
			}

			if (string.IsNullOrEmpty(outputPath))
			{
				System.Console.WriteLine("Please pass in an output location");
				System.Console.ReadLine();
				return;
			}

			try
			{
				settings = new();
				settings.ResetToDefaultValues();
				settings.LogConfigurationValues();
				GameData gameData = LoadAndProcess(inputPaths);
				PrepareExportDirectory(outputPath);
				Ripper.ExportProject(gameData, settings, outputPath, Ripper.GetDefaultPostExporters(), null);
			}
			catch
			{
				System.Console.WriteLine("An extraction error happened");
				System.Console.ReadLine();
			}
		}

		private static GameData LoadAndProcess(IReadOnlyList<string> paths)
		{
			GameData gameData = Load(paths);
			Process(gameData);
			return gameData;
		}

		private static GameData Load(IReadOnlyList<string> paths)
		{
			return Ripper.Load(paths, settings);
		}

		private static void Process(GameData gameData)
		{
			Ripper.Process(gameData, Ripper.GetDefaultProcessors(settings));
		}

		private static void PrepareExportDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				Logger.Info(LogCategory.Export, "Clearing export directory...");
				Directory.Delete(path, true);
			}
		}

		private static List<string> _args;

		private static void ParseArgs(string[] args)
		{
			_args = args.ToList();
			bool firstFlag = false;
			bool outputFlag = false;
			foreach (var item in _args)
			{
				System.Console.WriteLine(item);
				if (item.Trim().StartsWith("-"))
				{
					System.Console.WriteLine("First flag set");
					firstFlag = true;
				}
				if (!firstFlag)
				{
					System.Console.WriteLine($"input:{item}");
					inputPaths.Add(item);
				}
				if (outputFlag)
				{
					if (item.Trim() != "" && !item.StartsWith("-"))
					{
						System.Console.WriteLine($"output:{item}");
						outputPath = item;
					}
				}
				if (item.Trim().StartsWith("-o"))
				{
					System.Console.WriteLine("Second flag set");
					outputFlag = true;
				}
			}
		}
	}
}
