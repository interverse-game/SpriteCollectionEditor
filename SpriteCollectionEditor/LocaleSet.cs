using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TML.SpriteCollectionEditor {
	public record class LocaleSet(
		string MenuFile,
		string MenuFileNew,
		string MenuFileOpen,
		string MenuFileSaveAs,
		string MenuFileExit,
		string MenuAbout,
		string AnimationAdd,
		string ContextRemove,
		string AnimationId,
		string AnimationIdHelp,
		string AnimationTexturePaths,
		string AnimationTexturePathsAddHelp,
		string AnimationTexturePathsEditHelp,
		string AnimationSpeed,
		string AnimationSpeedHelp,
		string AnimationStartIndex,
		string AnimationStartIndexHelp,
		string AnimationIndexAfterLoop,
		string AnimationIndexAfterLoopHelp,
		string AnimationFlags,
		string AnimationStartPlaying,
		string AnimationStartPlayingHelp,
		string AnimationLooped,
		string AnimationPrecacheTextures,
		string AnimationPrecacheTexturesHelp,
		string AnimationScale,
		string AnimationOrigin,
		string AnimationOriginHelp,
		string AnimationNextAnimation,
		string AnimationNextAnimationHelp,
		string OpenCollectionError,
		string SaveCollectionError,
		string AddTexturePathError,
		string AddTextureConfigError,
		string ConfigAddsResPrefix,
		string ConfigAddsResPrefixHelp,
		string ConfigResourcePath,
		string ConfigResourcePathHelp,
		string ConfigBrowse,
		string ConfigLanguage,
		string Confirm,
		string Cancel,
		string EditTextureListHelp
	);
}
