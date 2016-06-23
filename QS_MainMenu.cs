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
using UnityEngine;

namespace QuickStart {

	public partial class QMainMenu {

		public bool Ready = false;
		public static QMainMenu Instance {
			get;
			private set;
		}

		private void Awake() {
			if (Instance != null) {
				QuickStart.Warning ("There's already an Instance", "QMainMenu");
				Destroy (this);
				return;
			}
			Instance = this;
			QuickStart.Log ("Awake", "QMainMenu");
		}

		private void Start() {
			if (!QSettings.Instance.Enabled) {
				QuickStart.Log ("No need to keep it loaded.", "QMainMenu");
				DestroyThis ();
				return;
			}
			StartCoroutine (QStart ());
			QuickStart.Log ("Start", "QMainMenu");
		}

		private IEnumerator QStart() {
			if (string.IsNullOrEmpty (QSaveGame.LastUsed)) {
				QuickStart.Warning ("Last savegame not found!", "QMainMenu");
				DestroyThis ();
				yield break;
			}
			if (!QSettings.Instance.Enabled) {
				QuickStart.Log ("QuickStart is disabled!", "QMainMenu");
				DestroyThis ();
				yield break;
			}
			while (!Ready) {
				yield return 0;
			}
			yield return new WaitForEndOfFrame ();
			yield return new WaitForSeconds (QSettings.Instance.WaitLoading);
			yield return new WaitForEndOfFrame ();
			QuickStart.Log ("MainMenu Loaded", "QMainMenu");
			QuickStart.Warning ("The last game found: " + QSaveGame.LastUsed, "QMainMenu");
			HighLogic.CurrentGame = GamePersistence.LoadGame (QSaveGame.File, QSaveGame.LastUsed, true, false);
			if (HighLogic.CurrentGame != null) {
				HighLogic.SaveFolder = QSaveGame.LastUsed;
				if (GamePersistence.UpdateScenarioModules (HighLogic.CurrentGame)) {
					GamePersistence.SaveGame (HighLogic.CurrentGame, QSaveGame.File, HighLogic.SaveFolder, SaveMode.OVERWRITE);
				}
				QuickStart.Log ("Goto SpaceCenter", "QMainMenu");
				HighLogic.CurrentGame.startScene = GameScenes.SPACECENTER;
				HighLogic.CurrentGame.Start ();
				InputLockManager.ClearControlLocks ();
				Destroy (this);
				yield break;
			}
			QuickStart.Warning ("Can't load the last save game", "QMainMenu");
			DestroyThis ();
			yield break;
		}

		private void LateUpdate() {
			if (!Ready) {
				QuickStart.Log ("Ready", "QMainMenu");
				Ready = true;
			}
		}

		private void DestroyThis() {
			QuickStart.Log ("DestroyThis", "QMainMenu");
			QuickStart_Persistent.vesselID = string.Empty;
			Destroy (this);
		}

		private void OnDestroy() {
			QuickStart.Log ("OnDestroy", "QMainMenu");
		}
	}
}