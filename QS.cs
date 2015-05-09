/* 
QuickStart
Copyright 2015 Malah

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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QuickStart {

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class QuickStart : Quick {

		private string lastSaveGameUsed;

		public string SaveFolder = KSPUtil.ApplicationRootPath + "saves";

		private Rect RectGUI = new Rect((Screen.width - 400) / 2, (Screen.height - 80), 400, 50);

		public static QuickStart Instance {
			get;
			private set;
		}

		private string LastSaveGameUsed {
			get {
				DateTime _lastWriteTime = DateTime.MinValue;
				string _lastDirectoryUsed = string.Empty;
				DirectoryInfo[] _directories = new DirectoryInfo (SaveFolder).GetDirectories ();
				foreach (DirectoryInfo _directory in _directories) {
					FileInfo[] _files = _directory.GetFiles ();
					FileInfo _file = Array.Find (_files, f => f.Name == "persistent.sfs");
					if (_file != null) {
						if (_file.LastWriteTime > _lastWriteTime) {
							_lastWriteTime = _file.LastWriteTime;
							_lastDirectoryUsed = _directory.Name;
						}
					}
				}
				if (string.IsNullOrEmpty(_lastDirectoryUsed)) {
					return string.Empty;
				}
				if (_lastDirectoryUsed == "scenarios" || _lastDirectoryUsed == "training") {
					return string.Empty;
				}
				return _lastDirectoryUsed;
			}
		}

		private void Awake() {
			if (Instance != null) {
				Quick.Warning ("There's already an Instance");
				Destroy (this);
				return;
			}
			Instance = this;
			DontDestroyOnLoad (Instance);
			lastSaveGameUsed = LastSaveGameUsed;
			StartCoroutine (QStart ());
		}
			
		private void Start() {
			QSettings.Instance.Load ();
		}

		private void OnDestroy() {
			Quick.Log ("Bye, have fun");
		}

		private IEnumerator QStart() {
			while (MainMenu.FindObjectOfType(typeof(MainMenu)) == null) {
				yield return 0;
			}
			Quick.Log ("MainMenu Loaded");
			if (QSettings.Instance.Enabled) {
				if (!string.IsNullOrEmpty(lastSaveGameUsed)) {
					Quick.Warning ("The last game found: " + lastSaveGameUsed);
					HighLogic.CurrentGame = GamePersistence.LoadGame ("persistent", lastSaveGameUsed, true, false);
					if (HighLogic.CurrentGame != null) {
						HighLogic.SaveFolder = lastSaveGameUsed;
						if (GamePersistence.UpdateScenarioModules (HighLogic.CurrentGame)) {
							GamePersistence.SaveGame (HighLogic.CurrentGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
						}
						HighLogic.CurrentGame.Start ();
						Destroy (this);
						yield break;
					} 
				}
				Quick.Warning ("Can't load the last save game");
			}
			Destroy (this);
		}

		private void OnGUI() {
			if (HighLogic.LoadedSceneIsGame) {
				Destroy (this);
				return;
			}
			if (string.IsNullOrEmpty (lastSaveGameUsed) || HighLogic.LoadedScene != GameScenes.LOADING) {
				return;
			}
			GUI.skin = HighLogic.Skin;
			GUILayout.BeginArea (RectGUI);
			bool _Enabled = GUILayout.Toggle (QSettings.Instance.Enabled, "Enable QuickStart to the last game: " + (lastSaveGameUsed != null ? lastSaveGameUsed : "Not found the last game"));
			if (_Enabled != QSettings.Instance.Enabled) {
				QSettings.Instance.Enabled = _Enabled;
				QSettings.Instance.Save ();
			}
			GUILayout.EndArea ();
		}
	}
}