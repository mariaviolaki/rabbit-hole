using System.Collections.Generic;
using System.IO;

namespace Dialogue
{
	public class DialogueFile
	{
		List<string> lines = new List<string>();

		public string FileName { get; private set; }
		public List<string> Lines { get { return lines; } }

		public DialogueFile(string fileName, string fileContents)
		{
			FileName = fileName;

			using StringReader sr = new StringReader(fileContents);
			string line;
			while ((line = sr.ReadLine()) != null)
			{
				if (line == string.Empty) continue;

				lines.Add(line);
			}
		}
	}
}