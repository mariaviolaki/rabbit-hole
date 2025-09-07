using Audio;
using Dialogue;
using Gameplay;
using History;
using IO;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
	PlayerSettings settings;
	PlayerProgress progress;
	AudioManager audioManager;
	GameOptionsSO gameOptions;

	HashSet<string> readLines;

	public string LastAutosaveNodeId { get; set; } = null;
	public bool HasPendingSettings { get; set; } = false;
	public bool HasPendingProgress { get; set; } = false;
	public PlayerSettings Settings => settings;
	public PlayerProgress Progress => progress;

	public string PlayerName => progress.playerName;
	public int SaveMenuPage => progress.saveMenuPage;

	public float AudioVolume => settings.audioVolume;
	public float AmbientAudioVolume => settings.ambientAudioVolume;
	public float MusicAudioVolume => settings.musicAudioVolume;
	public float SFXAudioVolume => settings.sfxAudioVolume;
	public float VoiceAudioVolume => settings.voiceAudioVolume;
	public DialogueSkipMode SkipMode => settings.skipMode;
	public float TextSpeed => settings.textSpeed;
	public float AutoSpeed => settings.autoSpeed;
	public ScreenMode GameScreenMode => settings.screenMode;
	public int GraphicsQuality => settings.graphicsQuality;
	public int ResolutionWidth => settings.resolutionWidth;
	public int ResolutionHeight => settings.resolutionHeight;

	public bool HasReadLine(string lineId) => readLines.Contains(lineId);

	public void Initialize(PlayerSettings settings, PlayerProgress progress, AudioManager audioManager, GameOptionsSO gameOptions)
	{
		this.settings = settings;
		this.progress = progress;
		this.audioManager = audioManager;
		this.gameOptions = gameOptions;

		if (progress.readLines == null)
			readLines = new HashSet<string>();
		else
			readLines = new HashSet<string>(progress.readLines);
	}

	public void ResetReadLines()
	{
		readLines.Clear();
		progress.readLines.Clear();
	}

	public void ResetVolume() => SetVolume(gameOptions.Audio.MasterVolume);
	public void ResetAmbientVolume() => SetAmbientVolume(gameOptions.Audio.AmbientVolume);
	public void ResetMusicVolume() => SetMusicVolume(gameOptions.Audio.MusicVolume);
	public void ResetSFXVolume() => SetSFXVolume(gameOptions.Audio.SFXVolume);
	public void ResetVoiceVolume() => SetVoiceVolume(gameOptions.Audio.VoiceVolume);
	public void ResetSkipMode() => SetSkipMode(gameOptions.Dialogue.SkipMode);
	public void ResetTextSpeed() => SetTextSpeed(gameOptions.Dialogue.TextSpeed);
	public void ResetAutoSpeed() => SetAutoSpeed(gameOptions.Dialogue.AutoSpeed);
	public void ResetScreenMode() => SetScreenMode(gameOptions.General.GameScreenMode);
	public void ResetGraphicsQuality() => SetGraphicsQuality((int)gameOptions.General.GraphicsQuality);
	public void ResetResolution() => SetResolution(gameOptions.General.ResolutionWidth, gameOptions.General.ResolutionHeight);

	public void AddReadLine(string lineId)
	{
		if (string.IsNullOrWhiteSpace(lineId) || readLines.Contains(lineId)) return;

		readLines.Add(lineId);
		progress.readLines.Add(lineId);
		HasPendingProgress = true;
	}

	public void SetPlayerName(string playerName)
	{
		if (progress.playerName == playerName) return;

		progress.playerName = playerName;
		HasPendingProgress = true;
	}

	public void SetSaveMenuPage(int pageNumber)
	{
		if (progress.saveMenuPage == pageNumber) return;

		progress.saveMenuPage = pageNumber;
		HasPendingProgress = true;
	}

	public void SetVolume(float volume)
	{
		audioManager.SetVolume(Audio.AudioType.None, volume);
		settings.audioVolume = volume;
		HasPendingSettings = true;
	}

	public void SetAmbientVolume(float volume)
	{
		audioManager.SetVolume(Audio.AudioType.Ambient, volume);
		settings.ambientAudioVolume = volume;
		HasPendingSettings = true;
	}

	public void SetMusicVolume(float volume)
	{
		audioManager.SetVolume(Audio.AudioType.Music, volume);
		settings.musicAudioVolume = volume;
		HasPendingSettings = true;
	}

	public void SetSFXVolume(float volume)
	{
		audioManager.SetVolume(Audio.AudioType.SFX, volume);
		settings.sfxAudioVolume = volume;
		HasPendingSettings = true;
	}

	public void SetVoiceVolume(float volume)
	{
		audioManager.SetVolume(Audio.AudioType.Voice, volume);
		settings.voiceAudioVolume = volume;
		HasPendingSettings = true;
	}

	public void SetSkipMode(DialogueSkipMode skipMode)
	{
		settings.skipMode = skipMode;
		HasPendingSettings = true;
	}

	public void SetTextSpeed(float speed)
	{
		settings.textSpeed = speed;
		HasPendingSettings = true;
	}

	public void SetAutoSpeed(float speed)
	{
		settings.autoSpeed = speed;
		HasPendingSettings = true;
	}

	public void SetScreenMode(ScreenMode screenMode)
	{
		Screen.fullScreen = screenMode == ScreenMode.Fullscreen;
		settings.screenMode = screenMode;
		HasPendingSettings = true;
	}

	public void SetGraphicsQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
		settings.graphicsQuality = qualityIndex;
		HasPendingSettings = true;
	}

	public void SetResolution(int width, int height)
	{
		Screen.SetResolution(width, height, settings.screenMode == ScreenMode.Fullscreen);
		settings.resolutionWidth = width;
		settings.resolutionHeight = height;
		HasPendingSettings = true;
	}
}
