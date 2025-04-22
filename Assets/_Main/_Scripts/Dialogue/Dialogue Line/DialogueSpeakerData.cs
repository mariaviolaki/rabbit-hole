using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Characters;

namespace Dialogue
{
	public class DialogueSpeakerData
	{
		const string CastDelimiter = " as ";
		const string PosDelimiter = " at ";
		const string LayerStartDelimiter = " [";
		const string LayerEndDelimiter = "]";
		const string RegexLayerDelimiter = @" \[";

		const string PosAxisDelimiter = ":";
		const string LayerTypeDelimiter = ",";
		const string LayerValueDelimiter = ":";
		readonly string DataDelimiterPattern = @$"{CastDelimiter}|{PosDelimiter}|{RegexLayerDelimiter}";

		string name;
		string displayName;
		float xPos = float.NaN;
		float yPos = float.NaN;
		Dictionary<SpriteLayerType, string> layers;

		public string Name { get { return name; } }
		public string DisplayName { get { return displayName; } }
		public float XPos { get { return xPos; } }
		public float YPos { get { return yPos; } }
		public Dictionary<SpriteLayerType, string> Layers { get { return layers; } }

		public DialogueSpeakerData(string rawSpeaker)
		{
			ParseSpeakerData(rawSpeaker);
		}

		void ParseSpeakerData(string rawSpeaker)
		{
			Regex delimiterRegex = new Regex(DataDelimiterPattern);
			MatchCollection delimiterMatches = delimiterRegex.Matches(rawSpeaker);

			name = "";
			displayName = "";
			layers = new Dictionary<SpriteLayerType, string>();

			// There should always be a speaker name - the rest are optional
			if (delimiterMatches.Count == 0)
			{
				name = rawSpeaker.Trim();
				return;
			}

			// The speaker name is always listed before all the other params
			name = rawSpeaker.Substring(0, delimiterMatches[0].Index);

			for (int i = 0; i < delimiterMatches.Count; i++)
			{
				Match match = delimiterMatches[i];
				Match nextMatch = i + 1 == delimiterMatches.Count ? null : delimiterMatches[i + 1];

				ParseDataBetweenMatches(rawSpeaker, match, nextMatch);
			}
		}

		void ParseDataBetweenMatches(string rawSpeaker, Match match, Match nextMatch)
		{
			int valueStart = match.Index + match.Length;
			int valueLength = nextMatch == null ? rawSpeaker.Length - valueStart : nextMatch.Index - valueStart;
			string value = rawSpeaker.Substring(valueStart, valueLength);

			if (match.Value == CastDelimiter)
			{
				displayName = value;
			}
			else if (match.Value == PosDelimiter)
			{
				string[] posAxes = value.Split(PosAxisDelimiter, System.StringSplitOptions.RemoveEmptyEntries);

				// TryParse automatically trims before parsing
				float.TryParse(posAxes[0], out xPos);
				if (posAxes.Length > 1)
					float.TryParse(posAxes[1], out yPos);
			}
			else if (match.Value == LayerStartDelimiter)
			{
				layers = new Dictionary<SpriteLayerType, string>();

				// Remove the end bracket enclosing the value
				value = value.Split(LayerEndDelimiter)[0];

				string[] layerStrings = value.Split(LayerTypeDelimiter);
				foreach (string layerString in layerStrings)
				{
					if (layerString.Contains(LayerValueDelimiter))
					{
						// Sprite characters use multiple layers
						string[] layerData = layerString.Split(LayerValueDelimiter, System.StringSplitOptions.RemoveEmptyEntries);
						layers[GetLayerFromText(layerData[0].Trim())] = layerData[1].Trim();
					}
					else
					{
						// Model3D characters have no layers
						layers[SpriteLayerType.None] = layerString.Trim();
					}
				}
			}
		}

		SpriteLayerType GetLayerFromText(string layerText)
		{
			if (Enum.TryParse(typeof(SpriteLayerType), layerText, ignoreCase: true, out object layer))
				return (SpriteLayerType)layer;

			return SpriteLayerType.None;
		}
	}
}
