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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuickStart {

	public partial class QLoading {

		private GUIStyle Button;

		private Rect RectGUI {
			get {
				return new Rect (0, Screen.height - 200, Screen.width, 200);
			}
		}

		public static QLoading Instance {
			get;
			private set;
		}

		private void Awake() {
			if (Instance != null) {
				QuickStart.Warning ("There's already an Instance", "QLoading");
				Destroy (this);
				return;
			}
			Instance = this;
			Button = new GUIStyle(HighLogic.Skin.button);
			Button.contentOffset = new Vector2(2,2);
			Button.alignment = TextAnchor.MiddleCenter;
			QuickStart.Log ("Awake", "QLoading");
		}

		private void Start() {
			QuickStart.Log ("Start", "QLoading");
		}

		private void OnDestroy() {
			QSettings.Instance.Save ();
			QuickStart.Log ("OnDestroy", "QLoading");
		}

		private void OnGUI() {
			if (HighLogic.LoadedScene != GameScenes.LOADING) {
				return;
			}
			if (string.IsNullOrEmpty (QSaveGame.LastUsed)) {
				return;
			}
			GUI.skin = HighLogic.Skin;
			GUILayout.BeginArea (RectGUI);
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (QSettings.Instance.Enabled) {
				GUILayout.Label (string.Format ("[{0}] {1}", QuickStart.MOD, (!string.IsNullOrEmpty (QSaveGame.LastUsed) ? "Last game found: <color=white><b>" + QSaveGame.LastUsed + "</b></color>" : "<b><color=#000000>No last game found</color></b>")));
				if (GUILayout.Button ("►", Button, GUILayout.Width (20), GUILayout.Height (20))) {
					QSaveGame.Next ();
				}
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			if (!string.IsNullOrEmpty (QSaveGame.LastUsed)) {
				GUILayout.BeginHorizontal ();
				QSettings.Instance.Enabled = GUILayout.Toggle (QSettings.Instance.Enabled, "Enable " + QuickStart.MOD, GUILayout.Width (250));
				if (QSettings.Instance.Enabled) {
					GUILayout.FlexibleSpace ();
					if (GUILayout.Toggle (QSettings.Instance.gameScene == (int)GameScenes.SPACECENTER, "Space Center", GUILayout.Width (250))) {
						if (QSettings.Instance.gameScene != (int)GameScenes.SPACECENTER) {
							QSettings.Instance.gameScene = (int)GameScenes.SPACECENTER;
						}
					}
					GUILayout.FlexibleSpace ();
					if (GUILayout.Toggle (QSettings.Instance.editorFacility == (int)EditorFacility.VAB && QSettings.Instance.gameScene == (int)GameScenes.EDITOR, "Vehicle Assembly Building", GUILayout.Width (250))) {
						if (QSettings.Instance.gameScene != (int)GameScenes.EDITOR || QSettings.Instance.editorFacility != (int)EditorFacility.VAB) {
							QSettings.Instance.gameScene = (int)GameScenes.EDITOR;
							QSettings.Instance.editorFacility = (int)EditorFacility.VAB;
						}
					}
					GUILayout.FlexibleSpace ();
					if (GUILayout.Toggle (QSettings.Instance.editorFacility == (int)EditorFacility.SPH && QSettings.Instance.gameScene == (int)GameScenes.EDITOR, "Space Plane Hangar", GUILayout.Width (250))) {
						if (QSettings.Instance.gameScene != (int)GameScenes.EDITOR || QSettings.Instance.editorFacility == (int)EditorFacility.SPH) {
							QSettings.Instance.gameScene = (int)GameScenes.EDITOR;
							QSettings.Instance.editorFacility = (int)EditorFacility.SPH;
						}
					}
					GUILayout.FlexibleSpace ();
					if (GUILayout.Toggle (QSettings.Instance.gameScene == (int)GameScenes.TRACKSTATION, "Tracking Station", GUILayout.Width (250))) {
						if (QSettings.Instance.gameScene != (int)GameScenes.TRACKSTATION) {
							QSettings.Instance.gameScene = (int)GameScenes.TRACKSTATION;
						}
					}
					GUILayout.FlexibleSpace ();
					GUI.enabled = !string.IsNullOrEmpty (QuickStart_Persistent.vesselID);
					if (GUILayout.Toggle (QSettings.Instance.gameScene == (int)GameScenes.FLIGHT, (!string.IsNullOrEmpty (QSaveGame.vesselName) ? string.Format("Last Vessel: {0}({1})", QSaveGame.vesselName, QSaveGame.vesselType) : "No vessel found"), GUILayout.Width (300))) {
						if (QSettings.Instance.gameScene != (int)GameScenes.FLIGHT) {
							QSettings.Instance.gameScene = (int)GameScenes.FLIGHT;
						}
					}
				}
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndVertical ();
			GUILayout.EndArea ();
		}
	}
}