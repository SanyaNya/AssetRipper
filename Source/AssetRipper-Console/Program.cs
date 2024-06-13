using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using System.IO;

namespace AssetRipper.Console
{
	internal class Program
	{
		private static List<string> inputPaths = new List<string>();
		private static string outputPath;
		private static LibraryConfiguration settings = LoadSettings();
		private static ExportHandler exportHandler = new(settings);
		public static ExportHandler ExportHandler
		{
			private get
			{
				return exportHandler;
			}
			set
			{
				ArgumentNullException.ThrowIfNull(value);
				value.ThrowIfSettingsDontMatch(settings);
				exportHandler = value;
			}
		}


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
				GameData gameData = ExportHandler.LoadAndProcess(inputPaths);
				PrepareExportDirectory(outputPath);
				ExportHandler.Export(gameData, outputPath);
			}
			catch (Exception ex) 
			{
				System.Console.WriteLine($"An extraction error happened{ex.Message}");
				System.Console.ReadLine();
			}
		}

		private static LibraryConfiguration LoadSettings()
		{
			LibraryConfiguration settings = new();
			settings.LoadFromDefaultPath();
			return settings;
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
