using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;

namespace TML.SpriteCollectionEditor {
	/// <summary>
	/// TextureListEditor.xaml 的交互逻辑
	/// </summary>
	public partial class ConfigDialog : Window {
		public ConfigDialog() {
			Global.LoadConfig();
			Config = new ConfigData() {
				ResourcePath = Global.Config.ResourcePath
			};

			InitializeComponent();
		}

		public ConfigData Config { get; }

		VistaFolderBrowserDialog folderDialog = new VistaFolderBrowserDialog();

		public RelayCommand<ConfigDialog> BrowseCommand { get; } = new RelayCommand<ConfigDialog>(
			(o) => {
				Debug.Assert(o != null);
				if (o.folderDialog.ShowDialog(o)??false) {
					o.Config.ResourcePath = o.folderDialog.SelectedPath;
				}
			}
		);
		public RelayCommand<ConfigDialog> ConfirmCommand { get; } = new RelayCommand<ConfigDialog>(
			(o) => {
				Debug.Assert(o != null);
				Global.Config.ResourcePath=o.Config.ResourcePath;
				Global.SaveConfig();
				o.Close();
			}
		);
		public RelayCommand<ConfigDialog> CancelCommand { get; } = new RelayCommand<ConfigDialog>(
			(o) => {
				Debug.Assert(o != null);
				o.Close();
			}
		);
	}
}
