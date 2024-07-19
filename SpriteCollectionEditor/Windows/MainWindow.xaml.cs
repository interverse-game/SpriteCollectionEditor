using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace TML.SpriteCollectionEditor {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public MainWindow() {
			InitializeComponent();
			Global.LoadConfig();
			Groups.PropertyChanged += (from, e) => AddGroupCommand.RaiseCanExecuteChanged();
		}

		public ConfigData Config => Global.Config;
		readonly VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

		public RelayCommand<MainWindow> ConfigBrowseCommand { get; } = new RelayCommand<MainWindow>(
			(o) => {
				Debug.Assert(o != null);
				if (o.folderDialog.ShowDialog(o) ?? false) {
					o.Config.ResourcePath = o.folderDialog.SelectedPath;
				}
			}
		);


		public Localization Localization => Global.Localization;

		public GroupManager Groups { get; } = new GroupManager();

		SpriteAnimation? currentGroup;
		public SpriteAnimation? CurrentGroup {
			get => currentGroup;
			set {
				currentGroup = value;
				if (currentGroup != null) {
					currentGroup.Id = currentGroup.OriginalId;
				}
				OnPropertyChanged(nameof(CurrentGroup));
				OnPropertyChanged(nameof(GroupEditorVisiblity));
			}
		}
		public Visibility GroupEditorVisiblity => CurrentGroup != null? Visibility.Visible: Visibility.Collapsed;
		public void SetGroup(string? id) {
			if (id == null) {
				CurrentGroup = null;
				return;
			}

			if (!Groups.TryGet(id, out var group)) {
				Debug.Assert(false);
			}

			CurrentGroup = group;
		}

		public string inputGroupId = "";
		public string InputGroupId {
			get => inputGroupId;
			set {
				inputGroupId = value;
				OnPropertyChanged(nameof(InputGroupId));
				AddGroupCommand.RaiseCanExecuteChanged();
			}
		}

		public bool HasChanges { get; private set; }

		public VistaOpenFileDialog selectTextureDialog = new() {
			Filter = "Texture(*.png)|*.png",
			Multiselect = true,
			ValidateNames = true,
			CheckFileExists = true,
			CheckPathExists = true
		};
		public VistaOpenFileDialog openCollectionDialog = new() {
			Filter = "Sprite Collection File(*.json)|*.json",
			ValidateNames = true,
			CheckFileExists = true,
			CheckPathExists = true
		};
		public VistaSaveFileDialog saveCollectionDialog = new() {
			Filter = "Sprite Collection File(*.json)|*.json",
			ValidateNames = true,
			CheckPathExists = true,
			AddExtension = true,
			DefaultExt="json"
		};

		#region Menu Bar Commands
		public RelayCommand<MainWindow> FileNewCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				o.InputGroupId = "";
				o.Groups.Clear();
			}
		);
		public RelayCommand<MainWindow> FileOpenCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);

				if (!(o.openCollectionDialog.ShowDialog(o) ?? false)) {
					return;
				}

				Dictionary<string, SpriteAnimationData>? data;
				try {
					var text = File.ReadAllText(o.openCollectionDialog.FileName);
					data = JsonSerializer.Deserialize<Dictionary<string, SpriteAnimationData>>(text);
				} catch (Exception e) {
					MessageBox.Show(o, string.Format(o.Localization.Strings.OpenCollectionError, e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				o.InputGroupId = "";
				o.Groups.Clear();

				if (data != null) {
					foreach (var pair in data) {
						o.Groups.Add(pair.Key, SpriteAnimation.FromData(pair.Value));
					}
				}
			}
		);
		public RelayCommand<MainWindow> FileSaveCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);

				if (!(o.saveCollectionDialog.ShowDialog(o) ?? false)) {
					return;
				}

				// Lose focus to update bound value
				o.textboxGroupId.Focus();

				Dictionary<string, SpriteAnimationData> data = new();
				foreach (var pair in o.Groups.Groups) {
					data.Add(pair.Key, pair.Value.ToData());
				}

				try {
					var text = JsonSerializer.Serialize(data);
					File.WriteAllText(o.saveCollectionDialog.FileName, text);
				} catch (Exception e) {
					MessageBox.Show(o, string.Format(o.Localization.Strings.SaveCollectionError, e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
		);
		public RelayCommand<MainWindow> AboutCommand { get; } = new((o) => {
			Debug.Assert(o != null);
			MessageBox.Show(o, "Sprite Collection Editor v2.0.0\nby TML233", "About", MessageBoxButton.OK, MessageBoxImage.Information);
		});
		public RelayCommand<MainWindow> ExitCommand { get; } = new((o) => {
			Debug.Assert(o != null);
			o.Close();
		});
		#endregion



		#region Group Commands
		public RelayCommand<MainWindow> AddGroupCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				var text = o.InputGroupId.Trim();
				var succeeded = o.Groups.Add(text, null);
				Debug.Assert(succeeded);
				o.InputGroupId = string.Empty;
				o.SetGroup(text);
			},
			(o) => {
				Debug.Assert(o != null);
				var text = o.InputGroupId.Trim();
				return !string.IsNullOrEmpty(text) && !o.Groups.IsExists(text);
			}
		);
		public RelayCommand<MainWindow> GroupListRemoveCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				var list = o.groupList;
				var count = list.SelectedItems.Count;
				if (count == 0) {
					return;
				}

				var removing = new string[count];
				for (int i = 0; i < count; i += 1) {
					if (list.SelectedItems[i] is not KeyValuePair<string, SpriteAnimation> pair) {
						Debug.Fail("Type error");
						return;
					}
					removing[i] = pair.Key;
				}

				foreach (var id in removing) {
					Debug.Assert(o.Groups.IsExists(id));
					o.Groups.Remove(id);
				}
			}
		);
		#endregion

		#region Group Editor Commands
		public RelayCommand<MainWindow> ConfirmGroupIdCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);

				var group = o.CurrentGroup;
				Debug.Assert(group != null);

				var succeeded =o.Groups.Rename(group.OriginalId, group.Id);
				Debug.Assert(succeeded);
				o.SetGroup(group.Id);
			},
			(o) => {
				Debug.Assert(o != null);

				var group = o.CurrentGroup;
				Debug.Assert(group != null);

				return !Validation.GetHasError(o.textboxGroupId) && group.Id != group.OriginalId;
			}
		);

		public RelayCommand<MainWindow> AddTextureCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				Debug.Assert(o.CurrentGroup != null);

				var resPath = Global.Config.ResourcePath;
				if (!Directory.Exists(resPath)) {
					MessageBox.Show(o, o.Localization.Strings.AddTextureConfigError, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				if (Global.HasResourcePathChanged) {
					Global.AcknowledgeTexturePath();
					o.selectTextureDialog.FileName = resPath.Replace('/', '\\') + "\\";
					//o.selectTextureDialog.InitialDirectory = resPath.Replace('/', '\\');
				}

				if (!(o.selectTextureDialog.ShowDialog(o) ?? false)) {
					return;
				}

				foreach (var fpath in o.selectTextureDialog.FileNames) {
					if (!Global.FormatPathForRes(fpath, out var result)) {
						MessageBox.Show(o, o.Localization.Strings.AddTexturePathError, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
						break;
					}
					o.CurrentGroup.TexturePaths.Add(new SpriteAnimation.TexturePath(result));
				}
			}
		);

		StringBuilder textureListSB = new StringBuilder();
		public RelayCommand<MainWindow> EditTextureListCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				Debug.Assert(o.CurrentGroup != null);

				o.textureListSB.Clear();
				foreach(var path in o.CurrentGroup.TexturePaths) {
					o.textureListSB.AppendLine(path.Path);
				}

				var dialog = new TextureListDialog() {
					Owner = o,
					ListText = o.textureListSB.ToString()
				};
				if (dialog.ShowDialog() != true) {
					return;
				}

				string text = dialog.ListText.Trim().Replace("\r\n", "\n");
				string[] textures = text.Split('\n');

				o.CurrentGroup.TexturePaths.Clear();
				foreach (var path in textures) {
					var tpath = path.Trim();
					if (string.IsNullOrEmpty(tpath)) {
						continue;
					}
					o.CurrentGroup.TexturePaths.Add(new SpriteAnimation.TexturePath(tpath));
				}
			}
		);
		public RelayCommand<MainWindow> TextureListRemoveCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				Debug.Assert(o.CurrentGroup != null);
				
				var list = o.textureList;
				var count = list.SelectedItems.Count;
				if (count == 0) {
					return;
				}

				var removing = new SpriteAnimation.TexturePath[count];
				for(int i = 0; i < count; i += 1) {
					var item = list.SelectedItems[i] as SpriteAnimation.TexturePath;
					Debug.Assert(item != null);
					removing[i] = item;
				}

				foreach(var item in removing) {
					o.CurrentGroup.TexturePaths.Remove(item);
				}
			}
		);
		#endregion
	}

	public class GroupManager : INotifyPropertyChanged {
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		Dictionary<string, SpriteAnimation> groups = new();
		public IReadOnlyDictionary<string, SpriteAnimation> Groups => groups;
		public IEnumerable<KeyValuePair<string,SpriteAnimation>> OrderedGroups => groups.OrderBy(x => x.Key);
		void OnGroupsChanged() {
			OnPropertyChanged(nameof(OrderedGroups));
		}

		public bool Add(string id, SpriteAnimation? group) {
			if (IsExists(id)) {
				return false;
			}
			group ??= new SpriteAnimation();
			group.Id = id;
			group.OriginalId = id;
			groups.Add(id, group);

			OnGroupsChanged();
			return true;
		}
		public bool IsExists(string id) {
			return groups.ContainsKey(id);
		}
		public bool TryGet(string id, [NotNullWhen(true)] out SpriteAnimation? result) {
			return groups.TryGetValue(id, out result);
		}
		public bool Remove(string id) {
			var r = groups.Remove(id);
			if (r) {
				OnGroupsChanged();
			}
			return r;
		}
		public bool Rename(string from, string to) {
			if (!TryGet(from, out var group)) {
				return false;
			}
			if (IsExists(to)) {
				return false;
			}
			Remove(from);
			Add(to, group);
			OnGroupsChanged();
			return true;
		}
		public void Clear() {
			groups.Clear();
			OnGroupsChanged();
		}
	}

	public class GroupIdRule : ValidationRule {
		public GroupIdRuleWrapper? Wrapper { get; set; }

		public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
			string? id = value as string;
			Debug.Assert(id != null);
			var window = Wrapper?.Window;
			Debug.Assert(window != null);
			var group = window.CurrentGroup;
			Debug.Assert(group != null);

			if (id != group.OriginalId) {
				if (window.Groups.IsExists(id)) {
					return new ValidationResult(false, "Id already exists!");
				}
			}

			return ValidationResult.ValidResult;
		}
	}
	public class GroupIdRuleWrapper : DependencyObject {
		public static readonly DependencyProperty WindowProperty =
			 DependencyProperty.Register(nameof(Window), typeof(MainWindow),
			 typeof(GroupIdRuleWrapper), new FrameworkPropertyMetadata(null));

		public MainWindow? Window {
			get { return (MainWindow?)GetValue(WindowProperty); }
			set { SetValue(WindowProperty, value); }
		}
	}
	public class BindingProxy : System.Windows.Freezable {
		protected override Freezable CreateInstanceCore() {
			return new BindingProxy();
		}

		public object Data {
			get { return (object)GetValue(DataProperty); }
			set { SetValue(DataProperty, value); }
		}

		public static readonly DependencyProperty DataProperty =
			DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
	}
}