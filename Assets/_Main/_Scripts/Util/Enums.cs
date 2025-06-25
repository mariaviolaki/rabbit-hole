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
}

namespace Dialogue
{
	public enum PromptPosition
	{
		EndOfText, TextboxBottomRight
	}

	public enum DialogueReadMode
	{
		None, Forward, Auto, Skip
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
}

namespace GameIO
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
}

namespace Commands
{
	public enum CommandSkipType
	{
		None, Immediate, Transition
	}
}

namespace Variables
{
	public enum ScriptVariableType
	{
		None, Tag, Variable
	}

	public enum ScriptVariableDataType
	{
		String, Int, Float, Bool
	}
}
