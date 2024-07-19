using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TML.SpriteCollectionEditor {
	public readonly record struct PlainConfigData(
		bool AddsResPrefix,
		string ResourcePath,
		string LocaleId
	);
	public class ConfigData : INotifyPropertyChanged {
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		bool addsResPrefix = true;
		public bool AddsResPrefix {
			get => addsResPrefix;
			set {
				addsResPrefix = value;
				OnPropertyChanged(nameof(AddsResPrefix));
			}
		}
		string resourcePath = "";
		public string ResourcePath {
			get => resourcePath;
			set {
				resourcePath = value;
				OnPropertyChanged(nameof(ResourcePath));
			}
		}

		string localeId = "";
		public string LocaleId {
			get => localeId;
			set {
				localeId = value;
				OnPropertyChanged(nameof(LocaleId));
			}
		}
	}
	public class Localization:INotifyPropertyChanged {
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		readonly Dictionary<string, LocaleSet> locales = new Dictionary<string, LocaleSet>();
		public IEnumerable<string> LocaleIds => locales.Keys;

		public void AddLocale(string id,LocaleSet locale)
		{
			locales.Add(id, locale);
		}
		public bool SetLocale(string id) {
			if (!locales.TryGetValue(id, out var locale)) {
				return false;
			}
			Strings = locale;
			return true;
		}
		LocaleSet strings = null!;
		public LocaleSet Strings {
			get => strings;
			private set {
				strings = value;
				OnPropertyChanged(nameof(Strings));
			}
		}
	}
	public static class Global {
		static Global() {
			string[] locales = ["en", "zh-cn"];
			foreach (var locale in locales) {
				try {
					var localeSet=JsonSerializer.Deserialize<LocaleSet>(File.ReadAllText($"./Languages/{locale}.json"));
					if (localeSet == null) throw new InvalidDataException("Language file is null!");
					Localization.AddLocale(locale, localeSet);
				} catch (Exception e) {
					MessageBox.Show($"Failed loading locale {locale}: {e}");
				}
			}
			Config.PropertyChanged += (o, e) => {
				if (e.PropertyName == "ResourcePath") {
					HasResourcePathChanged = true;
				}
				if (e.PropertyName == "LocaleId") {
					if (!Localization.SetLocale(Config.LocaleId)) {
						if (Config.LocaleId != "en") {
							Config.LocaleId = "en";
						}
					}
				}
				if (SaveConfigAfterChanged) {
					configSaveTimer.Stop();
					configSaveTimer.Interval = TimeSpan.FromSeconds(DelayBeforeConfigSave);
					configSaveTimer.Start();
				}
			};
			configSaveTimer.Tick += (_,_) => {
				configSaveTimer.Stop();
				SaveConfig();
			};
			SaveConfigAfterChanged = true;
		}

		public static Localization Localization { get; } = new Localization();

		public static ConfigData Config { get; } = new ConfigData();

		public const string ConfigPath = "./Config.json";
		static bool SaveConfigAfterChanged { get; set; } = false;
		public static float DelayBeforeConfigSave { get; set; } = 2;
		static DispatcherTimer configSaveTimer = new DispatcherTimer();
		public static void LoadConfig() {
			try {
				var text=File.ReadAllText(ConfigPath);
				var data = JsonSerializer.Deserialize<PlainConfigData>(text);
				Config.ResourcePath = data.ResourcePath;
				Config.LocaleId = data.LocaleId;
			} catch(Exception e) {
				//MessageBox.Show(string.Format("Failed to load config: {0}\n\nUsing default config.", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				ResetConfig();
			}
		}
		public static void SaveConfig() {
			PlainConfigData data = new PlainConfigData() {
				AddsResPrefix = Config.AddsResPrefix,
				ResourcePath = Config.ResourcePath,
				LocaleId = Config.LocaleId,
			};
			string text = JsonSerializer.Serialize(data);
			try {
				File.WriteAllText(ConfigPath, text);
			}catch(Exception e) {
				MessageBox.Show($"Failed to save config: {e}");
			}
		}
		public static void ResetConfig() {
			Config.ResourcePath = "";
			Config.LocaleId = "en";
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
			result = (Config.AddsResPrefix ? "res://" : "") + rel.Replace('\\', '/');
			return true;
		}

	}
}
