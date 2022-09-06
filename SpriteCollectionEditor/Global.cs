using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TML.SpriteCollectionEditor {
	public struct PlainConfigData {
		public string ResourcePath { get; init; }
	}
	public class ConfigData : INotifyPropertyChanged {
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		string resourcePath = "";
		public string ResourcePath {
			get => resourcePath;
			set {
				resourcePath = value;
				OnPropertyChanged(nameof(ResourcePath));
			}
		}
	}
	public static class Global {
		static Global() {
			Config.PropertyChanged += (o, e) => HasResourcePathChanged = true;
		}
		public static ConfigData Config { get; } = new ConfigData();

		public const string ConfigPath = "./Config.json";
		public static void LoadConfig() {
			try {
				var text=File.ReadAllText(ConfigPath);
				var data = Json.DeserializeStruct<PlainConfigData>(text);
				Config.ResourcePath = data.ResourcePath;
			} catch(Exception e) {
				//MessageBox.Show(string.Format("Failed to load config: {0}\n\nUsing default config.", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				ResetConfig();
			}
		}
		public static void SaveConfig() {
			PlainConfigData data = new PlainConfigData() {
				ResourcePath = Config.ResourcePath,
			};
			string text = Json.SerializeStruct(data, true);
			File.WriteAllText(ConfigPath, text);
		}
		public static void ResetConfig() {
			Config.ResourcePath = "";
		}

		public static bool HasResourcePathChanged { get; private set; } = true;
		public static void AcknowledgeTexturePath() {
			HasResourcePathChanged = false;
		}

		public static bool FormatPathForRes(string path, [NotNullWhen(true)] out string? result) {
			var rel = Path.GetRelativePath(Config.ResourcePath, path);
			if (rel.StartsWith('.') || Path.IsPathRooted(rel)) {
				result = null;
				return false;
			}
			result = "res://" + rel.Replace('\\', '/');
			return true;
		}
	}
}
