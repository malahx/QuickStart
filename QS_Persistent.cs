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
	public partial class QuickStart_Persistent {

		public static QuickStart_Persistent Instance {
			get;
			private set;
		}

		[KSPField(isPersistant = true)]	public static string vesselID = string.Empty;

		public static Guid VesselID {
			get {
				return new Guid (vesselID);
			}
		}

		public static readonly string shipFilename = "Auto-Saved Ship";

		public static string shipPath {
			get {
				return string.Format ("{0}/Ships/{1}/{2}.craft", QSaveGame.saveDir, ShipConstruction.GetShipsSubfolderFor ((EditorFacility)QSettings.Instance.editorFacility), shipFilename);
			}
		}

		public override void OnAwake() {
			if (QSpaceCenter.Instance != null) {
				QuickStart.Log ("QSpaceCenter is loaded, abort!", "QPersistent");
				Destroy (this);
				return;
			}
			if (Instance != null) {
				QuickStart.Warning ("There's already an Instance", "QPersistent");
				Destroy (this);
				return;
			}
			Instance = this;
			GameEvents.onFlightReady.Add (OnFlightReady);
			GameEvents.onVesselChange.Add (OnVesselChange);
			QSettings.Instance.gameScene = (int)HighLogic.LoadedScene;
			if (HighLogic.LoadedSceneIsEditor) {
				QSettings.Instance.editorFacility = (int)EditorDriver.editorFacility;
			}
			QSettings.Instance.Save ();
			QuickStart.Log ("OnAwake", "QPersistent");
		}

		private void Start() {
			if (HighLogic.LoadedSceneIsEditor && QSettings.Instance.enableEditorAutoSaveShip) {
				StartCoroutine (autoSaveShip ());
			}
			QuickStart.Log ("Start", "QPersistent");
		}

		IEnumerator autoSaveShip() {
			QuickStart.Log ("autoSaveShip: start", "QPersistent");
			while (HighLogic.LoadedSceneIsEditor && QSettings.Instance.enableEditorAutoSaveShip) {
				yield return new WaitForSeconds (QSettings.Instance.editorTimeToSave);
				ShipConstruction.SaveShip(shipFilename);
				QuickStart.Log ("autoSaveShip: save", "QPersistent");
			}
			QuickStart.Log ("autoSaveShip: end", "QPersistent");
		}

		private void OnDestroy() {
			GameEvents.onFlightReady.Remove (OnFlightReady);
			GameEvents.onVesselChange.Remove (OnVesselChange);
			QuickStart.Log ("OnDestroy", "QPersistent");
		}

		public override void OnLoad(ConfigNode node) {
			try {
				if (node != null) {
					if (node.HasValue ("vesselID")) {
						vesselID = node.GetValue ("vesselID");
					}
				}
				QuickStart.Log ("OnLoad", "QPersistent");
			} catch (Exception e) {
				QuickStart.Warning ("Can't load: {0} " + e, "QPersistent");
			}
		}

		public override void OnSave(ConfigNode node) {
			try {
				if (!string.IsNullOrEmpty (vesselID)) {
					node.AddValue ("vesselID", vesselID);
				}
				QuickStart.Log ("OnSave", "QPersistent");
			} catch (Exception e) {
				QuickStart.Warning ("Can't save: {0} " + e, "QPersistent");
			}
		}

		private void OnFlightReady() {
			vesselID = FlightGlobals.ActiveVessel.id.ToString();
			QuickStart.Log ("OnFlightReady", "QPersistent");
		}

		private void OnVesselChange(Vessel vessel) {
			vesselID = vessel.id.ToString();
			QuickStart.Log ("OnVesselChange", "QPersistent");
		}
	}
}

