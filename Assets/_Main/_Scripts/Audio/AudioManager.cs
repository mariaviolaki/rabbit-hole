using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	[SerializeField] FileManagerSO fileManager;
	[SerializeField] AudioMixerGroup musicMixer;
	[SerializeField] AudioMixerGroup ambientMixer;
	[SerializeField] AudioMixerGroup sfxMixer;
	[SerializeField] AudioMixerGroup voiceMixer;

	const string AmbientRootName = "Ambient";
	const string MusicRootName = "Music";
	const string SFXRootName = "SFX";
	const string VoiceRootName = "Voice";

	Transform ambientRoot;
	Transform musicRoot;
	Transform sfxRoot;
	Transform voiceRoot;

	void Awake()
	{
		ambientRoot = new GameObject(AmbientRootName).transform;
		ambientRoot.SetParent(transform);
		musicRoot = new GameObject(MusicRootName).transform;
		musicRoot.SetParent(transform);
		sfxRoot = new GameObject(SFXRootName).transform;
		sfxRoot.SetParent(transform);
		voiceRoot = new GameObject(VoiceRootName).transform;
		voiceRoot.SetParent(transform);
	}
	
	public async Task PlayAmbient(string name, float volume = 0.5f, float pitch = 1f, bool isLooping = true)
	{
		AudioClip audioClip = await fileManager.LoadAmbientAudio(name);
		PlayAudio(audioClip, ambientRoot, ambientMixer, volume, pitch, isLooping);
	}

	public async Task PlayMusic(string name, float volume = 0.5f, float pitch = 1f, bool isLooping = true)
	{
		AudioClip audioClip = await fileManager.LoadMusicAudio(name);
		PlayAudio(audioClip, musicRoot, musicMixer, volume, pitch, isLooping);
	}

	public async Task PlaySFX(string name, float volume = 0.5f, float pitch = 1f, bool isLooping = false)
	{
		AudioClip audioClip = await fileManager.LoadSFXAudio(name);
		PlayAudio(audioClip, sfxRoot, sfxMixer, volume, pitch, isLooping);
	}

	public async Task PlayVoice(string name, float volume = 0.5f, float pitch = 1f, bool isLooping = false)
	{
		AudioClip audioClip = await fileManager.LoadVoiceAudio(name);
		PlayAudio(audioClip, voiceRoot, voiceMixer, volume, pitch, isLooping);
	}

	public void StopAmbient(string name)
	{
		StopAudio(name, ambientRoot);
	}

	public void StopMusic(string name)
	{
		StopAudio(name, musicRoot);
	}

	public void StopSFX(string name)
	{
		StopAudio(name, sfxRoot);
	}

	public void StopVoice(string name)
	{
		StopAudio(name, voiceRoot);
	}

	public void PlayAudio(AudioClip audioClip, Transform audioRoot, AudioMixerGroup mixerGroup, float volume, float pitch, bool isLooping)
	{
		AudioSource audioSource = new GameObject(audioClip.name).AddComponent<AudioSource>();
		audioSource.transform.SetParent(audioRoot);
		audioSource.outputAudioMixerGroup = mixerGroup;
		audioSource.clip = audioClip;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.loop = isLooping;

		audioSource.Play();

		if (!isLooping)
		{
			float duration = audioClip.length / pitch;
			Destroy(audioSource.gameObject, duration + 1);
		}
	}

	public void StopAudio(string name, Transform audioRoot)
	{
		foreach (AudioSource audioSource in audioRoot.GetComponentsInChildren<AudioSource>())
		{
			if (audioSource.clip.name == name)
			{
				fileManager.UnloadMusicAudio(name);
				Destroy(audioSource.gameObject);
				return;
			}
		}
	}
}
