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
using System.Linq;
using UnityEngine;

namespace QuickStart {

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class QuickStart : Quick {

		private string lastSaveGameUsed;
		private string lastVessel;

		public string saveFolder = KSPUtil.ApplicationRootPath + "saves";
		public string saveFile = "persistent";
		public string saveExt = ".sfs";

		public string[] gameBlackList = {
			"scenarios",
			"training"
		};

		private Rect RectGUI = new Rect(0, (Screen.height - 100), Screen.width, 100);

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

		private ProtoVessel LastProtoVessel(Game game) {
			List<ProtoVessel> _pVessels = game.flightState.protoVessels;
			var _pVesselsSort = from _pv in _pVessels orderby _pv.launchTime select _pv;
			return _pVesselsSort.Last (_pv => pVesselIsOK (_pv));
		}
		private string LastVesselName(ConfigNode gameNode) {
			if (!gameNode.HasNode("GAME")) {
				return string.Empty;
			}
			if (!gameNode.GetNode("GAME").HasNode("FLIGHTSTATE")) {
				return string.Empty;
			}
			if (!gameNode.GetNode("GAME").GetNode("FLIGHTSTATE").HasNode("VESSEL")) {
				return string.Empty;
			}
			ConfigNode[] _vessels = gameNode.GetNode("GAME").GetNode("FLIGHTSTATE").GetNodes ("VESSEL");
			var _vesselsSort = from _v in _vessels orderby double.Parse(_v.GetValue("lct")) select _v;
			ConfigNode _lastVessel = _vesselsSort.Last (_v => vesselNodeIsOK (_v));
			if (_lastVessel == null) {
				return string.Empty;
			}
			return _lastVessel.GetValue ("name");
		}

		private bool pVesselIsOK(ProtoVessel pv) {
			return pv.vesselType != VesselType.Debris && pv.vesselType != VesselType.Flag && pv.vesselType != VesselType.SpaceObject && pv.vesselType != VesselType.Unknown;
		}
		private bool vesselNodeIsOK(ConfigNode pv) {
			return pv.GetValue("type") != VesselType.Debris.ToString() && pv.GetValue("type") != VesselType.Flag.ToString() && pv.GetValue("type") != VesselType.SpaceObject.ToString() && pv.GetValue("type") != VesselType.Unknown.ToString();
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
			if (!string.IsNullOrEmpty (lastSaveGameUsed)) {
				ConfigNode _saveGame = ConfigNode.Load (saveFolder + "/" + lastSaveGameUsed + "/" + saveFile + saveExt);
				if (_saveGame != null) {
					lastVessel = LastVesselName (_saveGame);
				}
			}
		}

		private void OnDestroy() {
			Quick.Log ("Destroy QuickStart Instance");
		}

		private IEnumerator QStart() {
			if (string.IsNullOrEmpty (lastSaveGameUsed)) {
				Destroy (this);
				yield break;
			}
			while (MainMenu.FindObjectOfType (typeof(MainMenu)) == null) {
				yield return 0;
			}
			Quick.Log ("MainMenu Loaded");
			if (QSettings.Instance.Enabled) {
				Quick.Warning ("The last game found: " + lastSaveGameUsed);
				HighLogic.CurrentGame = GamePersistence.LoadGame (saveFile, lastSaveGameUsed, true, false);
				if (HighLogic.CurrentGame != null) {
					HighLogic.SaveFolder = lastSaveGameUsed;
					if (GamePersistence.UpdateScenarioModules (HighLogic.CurrentGame)) {
						GamePersistence.SaveGame (HighLogic.CurrentGame, saveFile, HighLogic.SaveFolder, SaveMode.OVERWRITE);
					}
					ProtoVessel _pVessel = LastProtoVessel (HighLogic.CurrentGame);
					// HighLogic.CurrentGame.Start (); doesn't want to work with startScene set to FLIGHT or EDITOR ...
					if ((GameScenes)QSettings.Instance.GameScene != GameScenes.FLIGHT || _pVessel == null) {
						if ((GameScenes)QSettings.Instance.GameScene == GameScenes.FLIGHT) {
							Quick.Warning ("Can't find the last vessel");
						}
						HighLogic.CurrentGame.startScene = (GameScenes)QSettings.Instance.GameScene == GameScenes.EDITOR || (GameScenes)QSettings.Instance.GameScene == GameScenes.FLIGHT ? GameScenes.SPACECENTER : (GameScenes)QSettings.Instance.GameScene;
						/*if (HighLogic.CurrentGame.startScene == GameScenes.EDITOR) {
								HighLogic.CurrentGame.editorFacility = (QSettings.Instance.editorFacility ? EditorFacility.VAB : EditorFacility.SPH);
							}
							if (HighLogic.CurrentGame.startScene == GameScenes.FLIGHT) {
								HighLogic.CurrentGame.editorFacility = (QSettings.Instance.editorFacility ? EditorFacility.VAB : EditorFacility.SPH);
								HighLogic.CurrentGame.flightState.activeVesselIdx = HighLogic.CurrentGame.flightState.protoVessels.FindLastIndex (pv => pv == lastProtoVessel);
							}*/
						HighLogic.CurrentGame.Start ();
						if ((GameScenes)QSettings.Instance.GameScene == GameScenes.EDITOR) {
							while (SpaceCenterMain.FindObjectOfType (typeof(SpaceCenterMain)) == null) {
								yield return 0;
							}
							Quick.Log ("SpaceCenterMain Loaded");
							EditorDriver.StartEditor (QSettings.Instance.editorFacility ? EditorFacility.VAB : EditorFacility.SPH);
						}
					} else {
						string _saveGame = GamePersistence.SaveGame (HighLogic.CurrentGame, saveFile, HighLogic.SaveFolder, SaveMode.OVERWRITE);
						FlightDriver.StartAndFocusVessel (_saveGame, HighLogic.CurrentGame.flightState.protoVessels.FindLastIndex (pv => pv == _pVessel));
					}
					InputLockManager.ClearControlLocks ();
					Destroy (this);
					yield break;
				}
				Quick.Warning ("Can't load the last save game");
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
			bool _enabled = GUILayout.Toggle (QSettings.Instance.Enabled, "Enable QuickStart to the last game: " + (lastSaveGameUsed != null ? lastSaveGameUsed : "Not found the last game"), GUILayout.Width (300));
			if (_enabled != QSettings.Instance.Enabled) {
				QSettings.Instance.Enabled = _enabled;
				QSettings.Instance.Save ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginHorizontal ();
			if (QSettings.Instance.Enabled && QSettings.Instance.otherScenes) {
				GUILayout.FlexibleSpace ();
				if (GUILayout.Toggle (QSettings.Instance.GameScene == (int)GameScenes.SPACECENTER, "Space Center", GUILayout.Width (200))) {
					if (QSettings.Instance.GameScene != (int)GameScenes.SPACECENTER) {
						QSettings.Instance.GameScene = (int)GameScenes.SPACECENTER;
						QSettings.Instance.Save ();
					}
				}
				GUILayout.FlexibleSpace ();
				if (GUILayout.Toggle (QSettings.Instance.editorFacility && QSettings.Instance.GameScene == (int)GameScenes.EDITOR, "Vehicle Assembly Building", GUILayout.Width (200))) {
					if (QSettings.Instance.GameScene != (int)GameScenes.EDITOR || !QSettings.Instance.editorFacility) {
						QSettings.Instance.GameScene = (int)GameScenes.EDITOR;
						QSettings.Instance.editorFacility = true;
						QSettings.Instance.Save ();
					}
				}
				GUILayout.FlexibleSpace ();
				if (GUILayout.Toggle (!QSettings.Instance.editorFacility && QSettings.Instance.GameScene == (int)GameScenes.EDITOR, "Space Plane Hangar", GUILayout.Width (200))) {
					if (QSettings.Instance.GameScene != (int)GameScenes.EDITOR || QSettings.Instance.editorFacility) {
						QSettings.Instance.GameScene = (int)GameScenes.EDITOR;
						QSettings.Instance.editorFacility = false;
						QSettings.Instance.Save ();
					}
				}
				GUILayout.FlexibleSpace ();
				if (GUILayout.Toggle (QSettings.Instance.GameScene == (int)GameScenes.TRACKSTATION, "Tracking Station", GUILayout.Width (200))) {
					if (QSettings.Instance.GameScene != (int)GameScenes.TRACKSTATION) {
						QSettings.Instance.GameScene = (int)GameScenes.TRACKSTATION;
						QSettings.Instance.Save ();
					}
				}
				GUILayout.FlexibleSpace ();
				if (GUILayout.Toggle (QSettings.Instance.GameScene == (int)GameScenes.FLIGHT, "Last Vessel" + (!string.IsNullOrEmpty(lastVessel) ? ": " + lastVessel : "" ), GUILayout.Width (200))) {
					if (QSettings.Instance.GameScene != (int)GameScenes.FLIGHT) {
						QSettings.Instance.GameScene = (int)GameScenes.FLIGHT;
						QSettings.Instance.Save ();
					}
				}
				GUILayout.FlexibleSpace ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical ();
			GUILayout.EndArea ();
		}
	}
}