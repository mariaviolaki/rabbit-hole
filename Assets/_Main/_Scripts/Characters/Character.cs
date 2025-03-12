namespace Characters
{
	public abstract class Character
	{
		public CharacterData Data { get; private set; }

		public Character(string name, CharacterDirectorySO directory, GameOptionsSO options)
		{
			Data = directory.GetCharacterData(name, options);
		}
	}
}
