using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.PathIdMapping;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using AssetRipper.Processing.AnimatorControllers;
using AssetRipper.Processing.Assemblies;
using AssetRipper.Processing.AudioMixers;
using AssetRipper.Processing.Editor;
using AssetRipper.Processing.PrefabOutlining;
using AssetRipper.Processing.Scenes;
using AssetRipper.Processing.Textures;

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
			if(inputPaths.Count == 0)
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
                Ripper.ExportProject(gameData, settings, outputPath, GetDefaultPostExporters(), GetBeforeExport());
                
            } catch {
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
            Ripper.Process(gameData, GetDefaultProcessors(settings));
        }

        public static IEnumerable<IAssetProcessor> GetDefaultProcessors(LibraryConfiguration settings)
        {
            if (settings.ScriptContentLevel == ScriptContentLevel.Level1)
            {
                yield return new MethodStubbingProcessor();
            }
            yield return new SceneDefinitionProcessor();
            yield return new MainAssetProcessor();
            yield return new LightingDataProcessor();
            yield return new AnimatorControllerProcessor();
            yield return new AudioMixerProcessor();
            yield return new EditorFormatProcessor(settings.BundledAssetsExportMode);
            //Static mesh separation goes here
            if (settings.EnablePrefabOutlining)
            {
                yield return new PrefabOutliningProcessor();
            }
            yield return new PrefabProcessor();
            yield return new SpriteProcessor();
        }

        protected static Action<ProjectExporter>? GetBeforeExport()
        {
            return null;
        }

        public static IEnumerable<IPostExporter> GetDefaultPostExporters()
        {
            yield return new ProjectVersionPostExporter();
            yield return new PackageManifestPostExporter();
            yield return new StreamingAssetsPostExporter();
            yield return new DllPostExporter();
            yield return new PathIdMapExporter();
        }

        protected static IEnumerable<IPostExporter> GetPostExporters()
        {
            return GetDefaultPostExporters();
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
                if(!firstFlag)
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
