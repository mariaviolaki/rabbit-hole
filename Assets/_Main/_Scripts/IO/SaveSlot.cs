using Gameplay;
using History;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IO
{
	[System.Serializable]
	public class SaveSlot
	{
		public int slotNumber;
		public long dateTicks;
		public CharacterRoute route;
		public string sceneTitle;
		public List<HistoryState> historyStates;

		[NonSerialized] public Texture2D screenshot;

		public SaveSlot(int slotNumber, long dateTicks, CharacterRoute route, string sceneTitle, List<HistoryState> historyStates, Texture2D screenshot)
		{
			this.slotNumber = slotNumber;
			this.dateTicks = dateTicks;
			this.route = route;
			this.sceneTitle = sceneTitle;
			this.historyStates = historyStates;
			this.screenshot = screenshot;
		}
	}
}
