using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TML.SpriteCollectionEditor {
	public struct SpriteGroupData {
		public static explicit operator SpriteGroupData(SpriteGroup group) {
			var data= new SpriteGroupData() {
				TexturePaths = new List<string>(),
				Speed = group.Speed,
				StartPlaying = group.StartPlaying,
				StartIndex = group.StartIndex,
				Looped = group.Looped,
				FlipX = group.FlipX,
				FlipY = group.FlipY,
				OriginFactor = new Vector2(group.OriginFactorX, group.OriginFactorY),
				OriginOffset = new Vector2(group.OriginOffsetX, group.OriginOffsetY),
				NextGroup = group.NextGroupEnabled ? group.NextGroup : null,
			};
			foreach(var path in group.TexturePaths) {
				data.TexturePaths.Add(path.Path);
			}
			return data;
		}

		public List<string> TexturePaths { get; set; }
		public float Speed { get; set; }
		public bool StartPlaying { get; set; }
		public float StartIndex { get; set; }
		public bool Looped { get; set; }
		public bool FlipX { get; set; }
		public bool FlipY { get; set; }
		public Vector2 OriginFactor { get; set; }
		public Vector2 OriginOffset { get; set; }
		public string? NextGroup { get; set; }
	}

	public class SpriteGroup:INotifyPropertyChanged {
		public static explicit operator SpriteGroup(SpriteGroupData data) {
			var group = new SpriteGroup() {
				Speed = data.Speed,
				StartPlaying = data.StartPlaying,
				StartIndex = data.StartIndex,
				Looped = data.Looped,
				FlipX = data.FlipX,
				FlipY = data.FlipY,
				OriginFactorX = data.OriginFactor.x,
				OriginFactorY = data.OriginFactor.y,
				OriginOffsetX = data.OriginOffset.x,
				OriginOffsetY = data.OriginOffset.y,
				NextGroupEnabled = data.NextGroup != null,
				NextGroup = data.NextGroup ?? ""
			};
			foreach(var path in data.TexturePaths) {
				group.TexturePaths.Add(new TexturePath(path));
			}
			return group;
		}
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string propertyName) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		string id = string.Empty;
		public string Id {
			get => id;
			set {
				id = value;
				OnPropertyChanged(nameof(Id));
			}
		}

		public string OriginalId { get; set; } = string.Empty;

		public class TexturePath {
			public TexturePath(string path) {
				Path = path;
			}
			public string Path { get; set; }
		}
		public ObservableCollection<TexturePath> TexturePaths { get; } = new();

		float speed = 6;
		public float Speed {
			get => speed;
			set {
				speed = value;
				OnPropertyChanged(nameof(Speed));
			}
		}
		bool startPlaying = true;
		public bool StartPlaying {
			get => startPlaying;
			set {
				startPlaying = value;
				OnPropertyChanged(nameof(Speed));
			}
		}
		float startIndex = 0;
		public float StartIndex {
			get => startIndex;
			set {
				startIndex = value;
				OnPropertyChanged(nameof(StartIndex));
			}
		}
		bool looped = true;
		public bool Looped {
			get => looped;
			set {
				looped = value;
				OnPropertyChanged(nameof(Looped));
			}
		}
		bool flipX = false;
		public bool FlipX {
			get => flipX;
			set {
				flipX = value;
				OnPropertyChanged(nameof(FlipX));
			}
		}
		bool flipY = false;
		public bool FlipY {
			get => flipY;
			set {
				flipY = value;
				OnPropertyChanged(nameof(FlipY));
			}
		}
		float originFactorX = 0.5f;
		public float OriginFactorX {
			get => originFactorX;
			set {
				originFactorX = value;
				OnPropertyChanged(nameof(OriginFactorX));
			}
		}
		float originFactorY = 0.5f;
		public float OriginFactorY {
			get => originFactorY;
			set {
				originFactorY = value;
				OnPropertyChanged(nameof(OriginFactorY));
			}
		}
		float originOffsetX = 0;
		public float OriginOffsetX {
			get => originOffsetX;
			set {
				originOffsetX = value;
				OnPropertyChanged(nameof(OriginOffsetX));
			}
		}
		float originOffsetY = 0;
		public float OriginOffsetY {
			get => originOffsetY;
			set {
				originOffsetY = value;
				OnPropertyChanged(nameof(OriginOffsetY));
			}
		}
		bool nextGroupEnabled = false;
		public bool NextGroupEnabled {
			get => nextGroupEnabled;
			set {
				nextGroupEnabled = value;
				OnPropertyChanged(nameof(NextGroupEnabled));
			}
		}
		string nextGroup = string.Empty;
		public string NextGroup {
			get => nextGroup;
			set {
				nextGroup = value;
				OnPropertyChanged(nameof(NextGroup));
			}
		}
	}
}
