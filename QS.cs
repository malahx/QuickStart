﻿/* 
QuickStart
Copyright 2016 Malah

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System;
using System.Reflection;
using UnityEngine;

namespace QuickStart {
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public partial class QLoading : MonoBehaviour {}

	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public partial class QMainMenu : MonoBehaviour {}

	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public partial class QSpaceCenter : MonoBehaviour {}

	[KSPAddon(KSPAddon.Startup.Flight, true)]
	public partial class QFlight : MonoBehaviour {}

	[KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
	public partial class QuickStart_Persistent : ScenarioModule {}

	public class QuickStart {

		public readonly static string VERSION = Assembly.GetAssembly(typeof(QuickStart)).GetName().Version.Major + "." + Assembly.GetAssembly(typeof(QuickStart)).GetName().Version.Minor + Assembly.GetAssembly(typeof(QuickStart)).GetName().Version.Build;
		public readonly static string MOD = Assembly.GetAssembly(typeof(QuickStart)).GetName().Name;

		internal static void Log(string String, string Title = null, bool force = false) {
			if (!force) {
				if (!QSettings.Instance.Debug) {
					return;
				}
			}
			if (Title == null) {
				Title = MOD;
			} else {
				Title = string.Format ("{0}({1})", MOD, Title);
			}
			Debug.Log (string.Format ("{0}[{1}]: {2}", Title, VERSION, String));
		}
		internal static void Warning(string String, string Title = null) {
			if (Title == null) {
				Title = MOD;
			} else {
				Title = string.Format ("{0}({1})", MOD, Title);
			}
			Debug.LogWarning (string.Format ("{0}[{1}]: {2}", Title, VERSION, String));
		}
	}
}