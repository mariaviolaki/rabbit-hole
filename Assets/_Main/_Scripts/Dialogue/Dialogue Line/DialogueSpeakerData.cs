using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Dialogue
{
	public class DialogueSpeakerData
	{
		public enum Layer { None, Face, Body };

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
		string castName;
		Vector2 pos;
		Dictionary<Layer, string> layers;

		public string Name { get { return name; } }
		public string CastName { get { return castName; } }
		public Vector2 Pos { get { return pos; } }
		public Dictionary<Layer, string> Layers { get { return layers; } }
		public string DisplayName { get { return CastName == string.Empty ? Name : CastName; } }

		public DialogueSpeakerData(string rawSpeaker)
		{
			ParseSpeakerData(rawSpeaker);
		}

		void ParseSpeakerData(string rawSpeaker)
		{
			Regex delimiterRegex = new Regex(DataDelimiterPattern);
			MatchCollection delimiterMatches = delimiterRegex.Matches(rawSpeaker);

			name = "";
			castName = "";
			pos = new Vector2();
			layers = new Dictionary<Layer, string>();

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
				castName = value;
			}
			else if (match.Value == PosDelimiter)
			{
				string[] posAxes = value.Split(PosAxisDelimiter, System.StringSplitOptions.RemoveEmptyEntries);

				// TryParse automatically trims before parsing
				float.TryParse(posAxes[0], out pos.x);
				if (posAxes.Length > 1)
					float.TryParse(posAxes[1], out pos.y);
			}
			else if (match.Value == LayerStartDelimiter)
			{
				// Remove the end bracket enclosing the value
				value = value.Split(LayerEndDelimiter, System.StringSplitOptions.RemoveEmptyEntries)[0];

				string[] layerStrings = value.Split(LayerTypeDelimiter, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (string layerString in layerStrings)
				{
					string[] layerData = layerString.Split(LayerValueDelimiter, System.StringSplitOptions.RemoveEmptyEntries);
					layers[GetLayerFromText(layerData[0].Trim())] = layerData[1].Trim();
				}
			}
		}

		Layer GetLayerFromText(string layerText)
		{
			switch (layerText)
			{
				case "face": return Layer.Face;
				case "body": return Layer.Body;
				default: return Layer.None;
			}
		}
	}
}