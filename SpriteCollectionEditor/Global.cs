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
		public string LocaleId { get; set; }
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

		public Localization() {
			locales.Add("en", new LocaleSet {
				MenuFile = "_File",
				MenuFileNew = "_New",
				MenuFileOpen = "_Open...",
				MenuFileSaveAs = "_Save As...",
				MenuFileExit = "_Exit",
				MenuConfigure = "_Configure",
				MenuAbout = "_About",
				GroupAdd = "+ Group",
				ContextRemove = "Remove",
				GroupId = "Id",
				GroupTextures = "Textures",
				GroupSpeed = "Speed (FPS)",
				GroupStartIndex = "Start Index",
				GroupFlags = "Flags",
				GroupStartPlaying = "Start Playing",
				GroupLooped = "Looped",
				GroupFlipX = "Flip X",
				GroupFlipY = "Flip Y",
				GroupOriginFactor = "Origin Factor",
				GroupOriginOffset = "Origin Offset",
				GroupNextGroup = "Next Group",
				GroupEditingSingle = "Editing group \"{0}\"",
				GroupEditingMultiple = "Editing {0} groups",
				OpenCollectionError = "Failed to open sprite collection: {0}",
				SaveCollectionError = "Failed to save sprite collection: {0}",
				AddTexturePathError = "The file(s) you've selected are not in the resource path!",
				AddTextureConfigError = "Resource path invalid!\nPlease re-configure it in the following dialog.",
				Confirm = "Confirm",
				Cancel = "Cancel",
				ConfigResourcePath = "Resource Path",
				ConfigBrowse = "Browse...",
				ConfigLanguage = "Language"
			});

			locales.Add("zh-cn", new LocaleSet {
				MenuFile = "文件(_F)",
				MenuFileNew = "新建(_N)",
				MenuFileOpen = "打开(_O)...",
				MenuFileSaveAs = "另存为(_S)...",
				MenuFileExit = "退出(_E)",
				MenuConfigure = "配置(_C)",
				MenuAbout = "关于(_A)",
				GroupAdd = "+ 贴图组",
				ContextRemove = "移除",
				GroupId = "Id",
				GroupTextures = "贴图",
				GroupSpeed = "帧速率（FPS）",
				GroupStartIndex = "起始帧序号",
				GroupFlags = "选项",
				GroupStartPlaying = "开始时播放",
				GroupLooped = "循环播放",
				GroupFlipX = "翻转 - 左右",
				GroupFlipY = "翻转 - 上下",
				GroupOriginFactor = "原点比例",
				GroupOriginOffset = "原点偏移",
				GroupNextGroup = "接续组",
				GroupEditingSingle = "正在编辑贴图组 {0}",
				GroupEditingMultiple = "正在多选编辑 {0} 个贴图组",
				OpenCollectionError = "无法打开贴图集：{0}",
				SaveCollectionError = "无法保存贴图集：{0}",
				AddTexturePathError = "贴图文件需要位于资源路径内才能添加！",
				AddTextureConfigError = "资源路径无效！请在稍后弹出的对话框中配置。",
				Confirm = "确定",
				Cancel = "取消",
				ConfigResourcePath = "资源路径",
				ConfigBrowse = "浏览...",
				ConfigLanguage = "语言"
			});

			SetLocale("en");
		}
		readonly Dictionary<string, LocaleSet> locales = new Dictionary<string, LocaleSet>();
		public IEnumerable<string> LocaleIds => locales.Keys;

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
			Config.PropertyChanged += (o, e) => {
				if (e.PropertyName == "ResourcePath") {
					HasResourcePathChanged = true;
				}
				if (e.PropertyName == "LocaleId") {
					if (!Localization.SetLocale(Config.LocaleId)) {
						Config.LocaleId = "en";
					}
				}
			};
		}

		public static Localization Localization { get; } = new Localization();

		public static ConfigData Config { get; } = new ConfigData();

		public const string ConfigPath = "./Config.json";
		public static void LoadConfig() {
			try {
				var text=File.ReadAllText(ConfigPath);
				var data = Json.DeserializeStruct<PlainConfigData>(text);
				Config.ResourcePath = data.ResourcePath;
				Config.LocaleId = data.LocaleId;
			} catch(Exception e) {
				//MessageBox.Show(string.Format("Failed to load config: {0}\n\nUsing default config.", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				ResetConfig();
			}
		}
		public static void SaveConfig() {
			PlainConfigData data = new PlainConfigData() {
				ResourcePath = Config.ResourcePath,
				LocaleId = Config.LocaleId,
			};
			string text = Json.SerializeStruct(data, true);
			File.WriteAllText(ConfigPath, text);
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
			result = "res://" + rel.Replace('\\', '/');
			return true;
		}

	}
}
