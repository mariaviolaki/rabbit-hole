using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Audio;
using Characters;
using Dialogue;
using Visuals;
using UnityEngine;

namespace Commands
{
	public class CommandManager : MonoBehaviour
	{
		class CommandInputData
		{
			readonly string bank;
			readonly string command;
			readonly DialogueCommandArguments arguments;

			public string Bank => bank;
			public string Command => command;
			public DialogueCommandArguments Arguments => arguments;

			public CommandInputData(string bankName, string commandName, DialogueCommandArguments arguments)
			{
				this.bank = bankName;
				this.command = commandName;
				this.arguments = arguments;
			}
		}

		// Command bank categories
		public static readonly string MainBankName = "Main";
		public static readonly string DialogueBankName = "Dialogue";
		public static readonly string VisualsBankName = "Visuals";
		public static readonly string AudioBankName = "Audio";
		public static readonly string CharacterBankName = "Characters";
		public static readonly string GraphicsCharacterBankName = "GraphicsCharacters";
		public static readonly string SpriteCharacterBankName = "SpriteCharacters";
		public static readonly string Model3DCharacterBankName = "Model3DCharacters";
		const char CommandBankIdentifier = '.';

		[SerializeField] DialogueSystem dialogueSystem;
		CharacterManager characterManager;
		VisualGroupManager visualGroupManager;
		AudioManager audioManager;

		// The grouping of all dialogue commands available to be run (divided into categories)
		Dictionary<string, CommandBank> commandDirectories = new();

		// Keep track of all the processes that may run simultaneously
		readonly Dictionary<Guid, CommandProcess> processes = new();

		public CharacterManager Characters => characterManager;
		public VisualGroupManager Visuals => visualGroupManager;
		public AudioManager Audio => audioManager;
		public DialogueSystem Dialogue => dialogueSystem;

		void Start()
		{
			characterManager = dialogueSystem.Characters;
			visualGroupManager = dialogueSystem.Visuals;
			audioManager = dialogueSystem.Audio;
			InitBank();
		}

		public bool IsIdle()
		{
			bool isIdle = true;
			List<Guid> removedProcessIds = new();

			foreach (CommandProcess process in processes.Values)
			{
				bool isBlockingProcess = process.IsBlocking && !process.IsCompleted;

				if (isBlockingProcess)
					isIdle = false;
				else if (process.IsCompleted)
					removedProcessIds.Add(process.Id);
			}

			RemoveProcesses(removedProcessIds);
			return isIdle;
		}

		public CommandProcess Execute(string fullCommandName, DialogueCommandArguments inputArguments)
		{
			CommandInputData inputData = GetCommandInputData(fullCommandName, inputArguments);
			if (inputData == null)
			{
				Debug.LogWarning($"Skipped invalid command: '{fullCommandName}'");
				return null;
			}

			CommandBank bank = commandDirectories.ContainsKey(inputData.Bank) ? commandDirectories[inputData.Bank] : null;
			if (bank == null || !bank.HasCommand(inputData.Command))
			{
				Debug.LogWarning($"Command '{inputData.Command}' not found in bank '{inputData.Bank}'. Command skipped.");
				return null;
			}

			Delegate command = bank.GetCommand(inputData.Command);
			CommandSkipType skipType = bank.GetSkipType(inputData.Command);
			if (command == null) return null;

			if (command.Method.ReturnType == typeof(void))
			{
				command.DynamicInvoke(inputData.Arguments);
				return null;
			}
			else if (command.Method.ReturnType == typeof(Coroutine))
			{
				CommandProcess process = ExecuteProcess(command, inputData.Command, inputData.Arguments, skipType);
				return process;
			}

			return null;
		}

		public void SkipCommands()
		{
			List<Guid> removedProcessIds = new();

			foreach (CommandProcess process in processes.Values)
			{
				if (process.IsCompleted)
					removedProcessIds.Add(process.Id);
				else if (!process.IsCompleted && !process.IsBlocking)
					process.Stop();
			}

			RemoveProcesses(removedProcessIds);
		}

		public CommandBank GetBank(string bankName)
		{
			if (!commandDirectories.ContainsKey(bankName))
				commandDirectories.Add(bankName, new CommandBank());

			return commandDirectories[bankName];
		}

		CommandProcess ExecuteProcess(Delegate command, string commandName, DialogueCommandArguments arguments, CommandSkipType skipType)
		{
			Guid processId = Guid.NewGuid();

			CommandProcess commandProcess = new(processId, commandName, arguments, command, this, skipType);
			processes.Add(processId, commandProcess);
			commandProcess.Start();

			return commandProcess;
		}

		void RemoveProcesses(List<Guid> processIds)
		{
			foreach (Guid processId in processIds)
			{
				processes.Remove(processId);
			}
		}

		CommandInputData GetCommandInputData(string fullCommandName, DialogueCommandArguments arguments)
		{
			(string bankName, string commandName) = ParseCommandInput(fullCommandName);

			// If the bank name is valid, get the command from this bank
			if (commandDirectories.ContainsKey(bankName))
				return new CommandInputData(bankName, commandName, arguments);

			// If not bank was found, assume that a character name was provided
			string characterShortName = bankName;
			if (!characterManager.HasCharacter(characterShortName))
			{
				Debug.LogWarning($"Invalid Command Bank Entered: '{bankName}'. Command '{fullCommandName}' skipped.");
				return null;
			}

			bankName = GetCharacterBankName(characterShortName, commandName);

			// Inject the character's short name into the arguments so that the command can find the character
			arguments.AddIndexedArgument(0, characterShortName);

			return new CommandInputData(bankName, commandName, arguments);
		}

		(string, string) ParseCommandInput(string fullCommandName)
		{
			// If no bank was specified, get the command from the main bank
			int commandIndex = fullCommandName.LastIndexOf(CommandBankIdentifier);
			if (commandIndex == -1)
				return (MainBankName, fullCommandName);

			string bankName = fullCommandName.Substring(0, commandIndex);
			string commandName = fullCommandName.Substring(commandIndex + 1);

			return (bankName, commandName);
		}

		string GetCharacterBankName(string characterShortName, string commandName)
		{
			CharacterType characterType = characterManager.GetCharacter(characterShortName).Data.Type;
			bool isGraphicsCharacter = characterType == CharacterType.Sprite || characterType == CharacterType.Model3D;

			if (!isGraphicsCharacter) return CharacterBankName;

			if (characterType == CharacterType.Sprite && commandDirectories[SpriteCharacterBankName].HasCommand(commandName))
				return SpriteCharacterBankName;
			else if (characterType == CharacterType.Model3D && commandDirectories[Model3DCharacterBankName].HasCommand(commandName))
				return Model3DCharacterBankName;
			else if (commandDirectories[GraphicsCharacterBankName].HasCommand(commandName))
				return GraphicsCharacterBankName;
			
			return CharacterBankName;
		}

		void InitBank()
		{
			Assembly projectAssembly = Assembly.GetExecutingAssembly();
			Type[] commandTypes = projectAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(DialogueCommand))).ToArray();

			foreach (Type commandType in commandTypes)
			{
				// Search for all scripts inheriting from DialogueCommand and register their commands to the bank 
				MethodInfo registerMethod = commandType.GetMethod("Register", BindingFlags.Static | BindingFlags.Public);
				if (registerMethod != null)
					registerMethod.Invoke(null, new object[] { this });
				else
					Debug.LogError($"Register function not found in {commandType.Name}!");
			}
		}
	}
}
