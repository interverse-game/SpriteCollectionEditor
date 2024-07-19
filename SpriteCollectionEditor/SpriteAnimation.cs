using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TML.Engine;

namespace TML.SpriteCollectionEditor {
	public readonly record struct SpriteAnimationData(
		string[] TexturePaths,
		float Speed,
		bool StartPlaying,
		float StartIndex,
		float IndexAfterLoop,
		bool Looped,
		bool PrecacheTextures,
		Vector2 Scale,
		Vector2 OriginFactor,
		Vector2 OriginOffset,
		string NextAnimation
	);

	public class SpriteAnimation:INotifyPropertyChanged {
		public static SpriteAnimation FromData(SpriteAnimationData data) {
			var group = new SpriteAnimation() {
				Speed = data.Speed,
				StartPlaying = data.StartPlaying,
				StartIndex = data.StartIndex,
				IndexAfterLoop = data.IndexAfterLoop,
				Looped = data.Looped,
				PrecacheTextures = data.PrecacheTextures,
				ScaleX = data.Scale.x,
				ScaleY = data.Scale.y,
				OriginFactorX = data.OriginFactor.x,
				OriginFactorY = data.OriginFactor.y,
				OriginOffsetX = data.OriginOffset.x,
				OriginOffsetY = data.OriginOffset.y,
				NextAnimation = data.NextAnimation
			};
			foreach(var path in data.TexturePaths) {
				group.TexturePaths.Add(new TexturePath(path));
			}
			return group;
		}
		public SpriteAnimationData ToData() {
			return new SpriteAnimationData {
				TexturePaths = TexturePaths.Select(o => o.Path).ToArray(),
				Speed = Speed,
				StartPlaying = StartPlaying,
				StartIndex = StartIndex,
				IndexAfterLoop = IndexAfterLoop,
				Looped = Looped,
				PrecacheTextures = PrecacheTextures,
				Scale = new(ScaleX, ScaleY),
				OriginFactor = new(OriginFactorX, OriginFactorY),
				OriginOffset = new(OriginFactorX, OriginFactorY),
				NextAnimation = NextAnimation
			};
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
		float indexAfterLoop = 0;
		public float IndexAfterLoop {
			get => indexAfterLoop;
			set {
				indexAfterLoop = value;
				OnPropertyChanged(nameof(IndexAfterLoop));
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
		bool precacheTextures = true;
		public bool PrecacheTextures {
			get => precacheTextures;
			set {
				precacheTextures = value;
				OnPropertyChanged(nameof(PrecacheTextures));
			}
		}
		float scaleX = 1f;
		public float ScaleX {
			get => scaleX;
			set {
				scaleX = value;
				OnPropertyChanged(nameof(ScaleX));
			}
		}
		float scaleY = 1f;
		public float ScaleY {
			get => scaleY;
			set {
				scaleY = value;
				OnPropertyChanged(nameof(ScaleY));
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
		string nextAnimation = string.Empty;
		public string NextAnimation {
			get => nextAnimation;
			set {
				nextAnimation = value;
				OnPropertyChanged(nameof(NextAnimation));
			}
		}
	}
}
