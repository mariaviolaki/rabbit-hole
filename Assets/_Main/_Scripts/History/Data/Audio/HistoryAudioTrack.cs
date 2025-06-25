using Audio;

namespace History
{
	[System.Serializable]
	public class HistoryAudioTrack
	{
		public AudioType audioType;
		public int layerNum;
		public string name;
		public float volume;
		public float pitch;
		public bool isLooping;
	}
}
