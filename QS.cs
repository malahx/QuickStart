/* 
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace QuickStart {

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public partial class QuickStart {

		private string lastSaveGameUsed;

		public string saveFolder = KSPUtil.ApplicationRootPath + "saves";
		public string saveFile = "persistent";
		public string saveExt = ".sfs";

		public string[] gameBlackList = {
			"scenarios",
			"training"
		};

		private Rect RectGUI = new Rect((Screen.width - 400) / 2, Screen.height - 100, 400, 100);

		public static QuickStart Instance {
			get;
			private set;
		}

		private string LastSaveGameUsed {
			get {
				DateTime _lastWriteTime = DateTime.MinValue;
				string _lastDirectoryUsed = string.Empty;
				DirectoryInfo[] _directories = new DirectoryInfo (saveFolder).GetDirectories ();
				foreach (DirectoryInfo _directory in _directories) {
					FileInfo[] _files = _directory.GetFiles ();
					FileInfo _file = Array.Find (_files, f => f.Name == saveFile + saveExt);
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
				if (gameBlackList.Contains(_lastDirectoryUsed)) {
					return string.Empty;
				}
				return _lastDirectoryUsed;
			}
		}

		private void Awake() {
			if (Instance != null) {
				Warning ("There's already an Instance");
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
			Log ("Destroy QuickStart Instance");
		}

		private IEnumerator QStart() {
			if (string.IsNullOrEmpty (lastSaveGameUsed)) {
				Destroy (this);
				yield break;
			}
			while (MainMenu.FindObjectOfType (typeof(MainMenu)) == null) {
				yield return 0;
			}
			yield return new WaitForEndOfFrame();
			Log ("MainMenu Loaded");
			if (QSettings.Instance.Enabled) {
				Warning ("The last game found: " + lastSaveGameUsed);
				HighLogic.CurrentGame = GamePersistence.LoadGame (saveFile, lastSaveGameUsed, true, false);
				if (HighLogic.CurrentGame != null) {
					HighLogic.SaveFolder = lastSaveGameUsed;
					if (GamePersistence.UpdateScenarioModules (HighLogic.CurrentGame)) {
						GamePersistence.SaveGame (HighLogic.CurrentGame, saveFile, HighLogic.SaveFolder, SaveMode.OVERWRITE);
					}
					Log ("Goto SpaceCenter");
					HighLogic.CurrentGame.startScene = GameScenes.SPACECENTER;
					HighLogic.CurrentGame.Start ();
					InputLockManager.ClearControlLocks ();
					Destroy (this);
					yield break;
				}
				Warning ("Can't load the last save game");
			}
			Destroy (this);
		}

		private void OnGUI() {
			if (string.IsNullOrEmpty (lastSaveGameUsed) || HighLogic.LoadedScene != GameScenes.LOADING) {
				return;
			}
			GUI.skin = HighLogic.Skin;
			GUILayout.BeginArea (RectGUI);
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			bool _enabled = GUILayout.Toggle (QSettings.Instance.Enabled, "Enable QuickStart to the last game: " + (lastSaveGameUsed != null ? lastSaveGameUsed : "Not found the last game"), GUILayout.Width (400));
			if (_enabled != QSettings.Instance.Enabled) {
				QSettings.Instance.Enabled = _enabled;
				QSettings.Instance.Save ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			GUILayout.EndArea ();
		}
	}
}