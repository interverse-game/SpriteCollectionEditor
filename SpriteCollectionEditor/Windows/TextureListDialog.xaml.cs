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

namespace TML.SpriteCollectionEditor {
	/// <summary>
	/// TextureListEditor.xaml 的交互逻辑
	/// </summary>
	public partial class TextureListDialog : Window, INotifyPropertyChanged {
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public TextureListDialog() {
			InitializeComponent();
		}

		public Localization Localization => Global.Localization;

		string listText = "";
		public string ListText {
			get=> listText;
			set {
				listText = value;
				OnPropertyChanged(nameof(ListText));
			}
		}

		public RelayCommand<TextureListDialog> ConfirmCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				o.DialogResult = true;
				o.Close();
			}
		);
		public RelayCommand<TextureListDialog> CancelCommand { get; } = new(
			(o) => {
				Debug.Assert(o != null);
				o.DialogResult = false;
				o.Close();
			}
		);
	}
}
