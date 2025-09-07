using System;
using System.Collections.Generic;

namespace Variables
{
	public class DefaultVariables
	{
		public const string PlayerNameVariable = "playerName";
		public const string DefaultPlayerName = "Player";

		public const string RouteVariable = "route";
		public const string DefaultRoute = "Common";

		public const string HonestyVariable = "honesty";
		public const string DefaultHonesty = "0";
		public const int MinHonesty = -5;
		public const int MaxHonesty = 5;

		public const string KindnessVariable = "kindness";
		public const string DefaultKindness = "0";
		public const int MinKindness = -5;
		public const int MaxKindness = 5;

		public const string CuriosityVariable = "curiosity";
		public const string DefaultCuriosity = "0";
		public const int MinCuriosity = -5;
		public const int MaxCuriosity = 5;

		static readonly Dictionary<string, string> variables = new(StringComparer.OrdinalIgnoreCase)
		{
			{ PlayerNameVariable, DefaultPlayerName },
			{ RouteVariable, DefaultRoute },
			{ HonestyVariable, DefaultHonesty },
			{ KindnessVariable, DefaultKindness },
			{ CuriosityVariable, DefaultCuriosity }
		};

		public static Dictionary<string, string> Variables => variables;
	}
}
