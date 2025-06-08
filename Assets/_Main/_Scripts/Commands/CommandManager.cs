using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Audio;
using Characters;
using Dialogue;
using Graphics;
using UnityEngine;

namespace Commands
{
	public class CommandManager : MonoBehaviour
	{
		class CommandInputData
		{
			public string Directory { get; private set; }
			public string Command { get; private set; }
			public string[] Arguments { get; private set; }

			public CommandInputData(string directoryName, string commandName, string[] arguments)
			{
				Directory = directoryName;
				Command = commandName;
				Arguments = arguments;
			}
		}

		// Command directory categories
		public static readonly string MainDirectoryName = "Main";
		public static readonly string DialogueDirectoryName = "Dialogue";
		public static readonly string VisualsDirectoryName = "Visuals";
		public static readonly string AudioDirectoryName = "Audio";
		public static readonly string CharacterDirectoryName = "Characters";
		public static readonly string GraphicsCharacterDirectoryName = "GraphicsCharacters";
		public static readonly string SpriteCharacterDirectoryName = "SpriteCharacters";
		public static readonly string Model3DCharacterDirectoryName = "Model3DCharacters";
		const char CommandDirectoryIdentifier = '.';

		CharacterManager characterManager;
		GraphicsGroupManager graphicsGroupManager;
		AudioManager audioManager;
		DialogueSystem dialogueSystem;

		// The grouping of all dialogue commands available to be run (divided into categories)
		Dictionary<string, CommandDirectory> commandDirectories = new Dictionary<string, CommandDirectory>();

		// Keep track of all the processes that may run simultaneously
		readonly Dictionary<Guid, CommandProcess> processes = new Dictionary<Guid, CommandProcess>();
		readonly HashSet<string> unskippableProcessNames = new HashSet<string>();

		public bool IsIdle => processes.Count == 0;
		public CharacterManager GetCharacterManager() => characterManager;
		public GraphicsGroupManager GetGraphicsGroupManager() => graphicsGroupManager;
		public AudioManager GetAudioManager() => audioManager;
		public DialogueSystem GetDialogueSystem() => dialogueSystem;

		void Start()
		{
			characterManager = FindObjectOfType<CharacterManager>();
			graphicsGroupManager = FindObjectOfType<GraphicsGroupManager>();
			audioManager = FindObjectOfType<AudioManager>();
			dialogueSystem = FindObjectOfType<DialogueSystem>();
			InitDirectory();
		}

		public CommandProcess Execute(string fullCommandName, params string[] inputArguments)
		{
			CommandInputData inputData = GetCommandInputData(fullCommandName, inputArguments);
			if (inputData == null)
			{
				Debug.LogWarning($"Skipped invalid command: '{fullCommandName}'");
				return null;
			}

			CommandDirectory directory = commandDirectories.ContainsKey(inputData.Directory) ? commandDirectories[inputData.Directory] : null;
			if (directory == null || !directory.HasCommand(inputData.Command))
			{
				Debug.LogWarning($"Command '{inputData.Command}' not found in directory '{inputData.Directory}'. Command skipped.");
				return null;
			}

			Delegate command = directory.GetCommand(inputData.Command);
			Delegate skipCommand = directory.GetSkipCommand(inputData.Command);
			if (command == null) return null;

			if (command.Method.ReturnType == typeof(void))
			{
				ExecuteAction(command, inputData.Arguments);
				return null;
			}
			else if (command.Method.ReturnType == typeof(IEnumerator) || command.Method.ReturnType == typeof(Task))
			{
				CommandProcess process = ExecuteProcess(command, skipCommand, inputData.Command, inputData.Arguments);
				return process;
			}

			return null;
		}

		public void SkipCommands()
		{
			// Iterate over a copy of the list to avoid conflicts in case the processes change in the meantime
			foreach (CommandProcess process in processes.Values.ToList())
			{
				process.Stop();
			}
		}

		public CommandDirectory GetDirectory(string directoryName)
		{
			if (!commandDirectories.ContainsKey(directoryName))
				commandDirectories.Add(directoryName, new CommandDirectory());

			return commandDirectories[directoryName];
		}

		void DeleteProcess(Guid processId)
		{
			processes.Remove(processId);
		}

		void ExecuteAction(Delegate command, string[] arguments)
		{
			if (command is Action)
				command.DynamicInvoke();
			else if (command is Action<string>)
				command.DynamicInvoke(arguments[0]);
			else if (command is Action<string[]>)
				command.DynamicInvoke((object)arguments);
		}

		CommandProcess ExecuteProcess(Delegate command, Delegate skipCommand, string commandName, string[] arguments)
		{
			Guid processId = Guid.NewGuid();
			bool isUnskippable = unskippableProcessNames.Contains(commandName);

			CommandProcess commandProcess = new CommandProcess(commandName, arguments, command, skipCommand, this, isUnskippable);
			commandProcess.OnFullyCompleted += () => DeleteProcess(processId);
			commandProcess.Start();
			processes.Add(processId, commandProcess);

			return commandProcess;
		}

		CommandInputData GetCommandInputData(string fullCommandName, string[] arguments)
		{
			(string directoryName, string commandName) = ParseCommandInput(fullCommandName);

			// If the directory name is valid, get the command from this directory
			if (commandDirectories.ContainsKey(directoryName))
				return new CommandInputData(directoryName, commandName, arguments);

			// If not directory was found, assume that a character name was provided
			string characterShortName = directoryName;
			if (!characterManager.HasCharacter(characterShortName))
			{
				Debug.LogWarning($"Invalid Command Directory Entered: '{directoryName}'. Command '{fullCommandName}' skipped.");
				return null;
			}

			directoryName = GetCharacterDirectoryName(characterShortName, commandName);

			// Inject the character's short name into the arguments so that the command can find the character
			string[] characterArguments = new string[arguments.Length + 1];
			characterArguments[0] = characterShortName;
			Array.Copy(arguments, 0, characterArguments, 1, arguments.Length);

			return new CommandInputData(directoryName, commandName, characterArguments);
		}

		(string, string) ParseCommandInput(string fullCommandName)
		{
			// If no directory was specified, get the command from the main directory
			int commandIndex = fullCommandName.LastIndexOf(CommandDirectoryIdentifier);
			if (commandIndex == -1)
				return (MainDirectoryName, fullCommandName);

			string directoryName = fullCommandName.Substring(0, commandIndex);
			string commandName = fullCommandName.Substring(commandIndex + 1);

			return (directoryName, commandName);
		}

		string GetCharacterDirectoryName(string characterShortName, string commandName)
		{
			CharacterType characterType = characterManager.GetCharacter(characterShortName).Data.Type;
			bool isGraphicsCharacter = characterType == CharacterType.Sprite || characterType == CharacterType.Model3D;

			if (!isGraphicsCharacter) return CharacterDirectoryName;

			if (characterType == CharacterType.Sprite && commandDirectories[SpriteCharacterDirectoryName].HasCommand(commandName))
				return SpriteCharacterDirectoryName;
			else if (characterType == CharacterType.Model3D && commandDirectories[Model3DCharacterDirectoryName].HasCommand(commandName))
				return Model3DCharacterDirectoryName;
			else if (commandDirectories[GraphicsCharacterDirectoryName].HasCommand(commandName))
				return GraphicsCharacterDirectoryName;
			
			return CharacterDirectoryName;
		}

		void InitDirectory()
		{
			Assembly projectAssembly = Assembly.GetExecutingAssembly();
			Type[] commandTypes = projectAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DialogueCommand))).ToArray();

			foreach (Type commandType in commandTypes)
			{
				// Search for all scripts inheriting from DialogueCommand and register their commands to the directory 
				MethodInfo registerMethod = commandType.GetMethod("Register", BindingFlags.Static | BindingFlags.Public);
				if (registerMethod != null)
					registerMethod.Invoke(null, new object[] { this });
				else
					Debug.LogError($"Register function not found in {commandType.Name}!");

				// If any unskippable processes are included, cache them in a hash set
				FieldInfo unskippableProcessesField = commandType.GetField("unskippableProcesses", BindingFlags.Static | BindingFlags.Public);
				if (unskippableProcessesField != null)
				{
					string[] unskippableProcessNames = (string[])unskippableProcessesField.GetValue(null);
					foreach (string processName in unskippableProcessNames)
					{
						this.unskippableProcessNames.Add(processName);
					}
				}
			}
		}
	}
}
