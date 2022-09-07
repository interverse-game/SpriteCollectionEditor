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

		public GroupManager Groups { get; } = new GroupManager();

		SpriteGroup? currentGroup;
		public SpriteGroup? CurrentGroup {
			get => currentGroup;
			private set {
				currentGroup = value;
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
			group.Id = group.OriginalId;
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

				Dictionary<string, SpriteGroupData>? data;
				try {
					var text = File.ReadAllText(o.openCollectionDialog.FileName);
					data = Json.DeserializeClass<Dictionary<string, SpriteGroupData>>(text);
				} catch (Exception e) {
					MessageBox.Show(o, string.Format("Failed to open Sprite Collection: {0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}

				o.InputGroupId = "";
				o.Groups.Clear();

				if (data != null) {
					foreach (var pair in data) {
						o.Groups.Add(pair.Key, (SpriteGroup)pair.Value);
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

				Dictionary<string, SpriteGroupData> data = new();
				foreach (var pair in o.Groups.Groups) {
					data.Add(pair.Key, (SpriteGroupData)pair.Value);
				}

				try {
					var text = Json.SerializeClass(data, true);
					File.WriteAllText(o.saveCollectionDialog.FileName, text);
				} catch (Exception e) {
					MessageBox.Show(o, string.Format("Failed to save Sprite Collection: {0}", e.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}
		);
		public RelayCommand<MainWindow> ConfigCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				var window = new ConfigDialog {
					Owner = o
				};
				window.ShowDialog();
			}
		);
		public RelayCommand<MainWindow> AboutCommand { get; } = new((o) => {
			Debug.Assert(o != null);
			MessageBox.Show(o, "Sprite Collection Editor v1.1.0\nby TML233", "About", MessageBoxButton.OK, MessageBoxImage.Information);
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
			},
			(o) => {
				Debug.Assert(o != null);
				var text = o.InputGroupId.Trim();
				return !string.IsNullOrEmpty(text) && !o.Groups.IsExists(text);
			}
		);
		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (sender is not ListView list) {
				return;
			}
			if (list.SelectedItem == null) {
				SetGroup(null);
				return;
			}

			var id = list.SelectedItem as string;
			Debug.Assert(id != null);

			SetGroup(id);
		}
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
					string? id = list.SelectedItems[i] as string;
					Debug.Assert(id != null);
					removing[i] = id;
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
					MessageBox.Show(o, "Resource path invalid or not exists!\nPlease re-configurate it in the following dialog.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
					var dialog = new ConfigDialog() {
						Owner = o
					};
					dialog.ShowDialog();
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
						MessageBox.Show(o, "The file(s) you've chosen are not in the resource path!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
						break;
					}
					o.CurrentGroup.TexturePaths.Add(new SpriteGroup.TexturePath(result));
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
					o.CurrentGroup.TexturePaths.Add(new SpriteGroup.TexturePath(tpath));
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

				var removing = new SpriteGroup.TexturePath[count];
				for(int i = 0; i < count; i += 1) {
					var item = list.SelectedItems[i] as SpriteGroup.TexturePath;
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


		Dictionary<string, SpriteGroup> groups = new();
		public IReadOnlyDictionary<string, SpriteGroup> Groups => groups;
		public IEnumerable<string> Ids => groups.Keys.OrderBy(x => x);
		void OnGroupsChanged() {
			OnPropertyChanged(nameof(Ids));
		}

		public bool Add(string id, SpriteGroup? group) {
			if (IsExists(id)) {
				return false;
			}
			group ??= new SpriteGroup();
			group.Id = id;
			group.OriginalId = id;
			groups.Add(id, group);

			OnGroupsChanged();
			return true;
		}
		public bool IsExists(string id) {
			return groups.ContainsKey(id);
		}
		public bool TryGet(string id, [NotNullWhen(true)] out SpriteGroup? result) {
			return groups.TryGetValue(id, out result);
		}
		public bool Remove(string id) {
			var r = groups.Remove(id);
			if (r) {
				OnGroupsChanged();
			}
			return r;
		}
		public bool Rename(string from,string to) {
			if(!TryGet(from,out var group)) {
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

	public class PercentageConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return string.Format("{0:0.##}", ((float)value) * 100);
		}

		public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is string s) {
				if (float.TryParse(s, out var v)) {
					return v / 100f;
				}
			}
			return null;
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