namespace Characters
{
	public enum CharacterType
	{
		Text, Sprite, Model3D
	}

	public enum SpriteLayerType
	{
		None, Body, Face
	}

	public enum TransitionStatus
	{
		Started, Skipped, Completed
	}
}

namespace Dialogue
{
	public enum DialogueNodeType
	{
		None, Jump, Dialogue, Command, Input, Choice, ChoiceBranch, Condition, ConditionBranch, Assignment
	}

	public enum PromptPosition
	{
		EndOfText, TextboxBottomRight
	}

	public enum DialogueReadMode
	{
		None, Forward, Auto, Skip
	}

	public enum DialogueSkipMode
	{
		Read, Unread, AfterChoices
	}

	public enum TextBuildMode
	{
		Instant, Typed, InstantFade, TypedFade
	}

	public enum SegmentStartMode
	{
		None, InputClear, InputAppend, AutoClear, AutoAppend
	}
}

namespace Audio
{
	public enum AudioType
	{
		None, Ambient, Music, SFX, Voice
	}
}

namespace Visuals
{
	public enum VisualType
	{
		None, Background, Foreground, Cinematic
	}

	public enum GraphicsQuality
	{
		VeryLow = 0, Low = 1, Medium = 2, High = 3, VeryHigh = 4, Ultra = 5
	}
}

namespace IO
{
	public enum InputActionType
	{
		Advance, Auto, Skip, SkipHold, SkipHoldEnd
	}

	public enum InputActionDuration
	{
		Toggle, Hold, End
	}

	public enum FileExtension
	{
		None, mp4, mp3, ogg
	}

	public enum AssetType
	{
		Video, Audio, Dialogue, Image, CharacterAtlas, Model3DPrefab
	}

	public enum SaveMenuMode
	{
		Save, Load
	}

	public enum ScreenMode
	{
		None, Windowed, Fullscreen
	}
}

namespace Variables
{
	public enum DataTypeEnum
	{
		None, String, Int, Float, Bool
	}
}

namespace UI
{
	public enum MenuType
	{
		None, SideMenu, Save, Load, Log, Relationships, Settings, Title
	}

	public enum SettingsSection
	{
		None, Display, Text, Audio
	}
}

namespace Gameplay
{
	public enum CharacterRoute
	{
		Common, Void, Zero, Marsh
	}
}
