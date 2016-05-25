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
	
	public class QSaveGame : QuickStart	{

		private static ConfigNode saveGame;

		public static string vesselName;
		public static string vesselType;

		public static string Folder = KSPUtil.ApplicationRootPath + "saves";
		public static string File = "persistent";
		public static string Ext = ".sfs";

		public static string saveFile {
			get {
				return string.Format ("{0}/{1}{2}", saveDir, File, Ext);
			}
		}

		public static string saveDir {
			get {
				return string.Format ("{0}/{1}", Folder, LastUsed);
			}
		}

		public static string[] gameBlackList = {
			"scenarios",
			"training"
		};

		private static int indexSave = -1;
		[KSPField(isPersistant = true)]	private static string lastUsed;
		public static string LastUsed {
			get {
				if (lastUsed == null) {
					DateTime _lastWriteTime = DateTime.MinValue;
					string _lastDirectoryUsed = string.Empty;
					DirectoryInfo[] _directories = new DirectoryInfo (Folder).GetDirectories ();
					for (int _i = 0; _i < _directories.Length; _i++) {
						DirectoryInfo _directory = _directories [_i];
						FileInfo[] _files = _directory.GetFiles ();
						FileInfo _file = Array.Find (_files, f => f.Name == File + Ext);
						if (_file != null) {
							if (_file.LastWriteTime > _lastWriteTime) {
								_lastWriteTime = _file.LastWriteTime;
								_lastDirectoryUsed = _directory.Name;
								indexSave = _i;
							}
						}
					}
					if (string.IsNullOrEmpty (_lastDirectoryUsed) || gameBlackList.Contains (_lastDirectoryUsed)) {
						_lastDirectoryUsed = string.Empty;
					}
					lastUsed = _lastDirectoryUsed;
					UpdateSave (true);
					Log ("Last savegame used found: " + lastUsed, "QSaveGame");
				}
				return lastUsed;
			}
		}

		public static void Next() {
			DirectoryInfo[] _directories = new DirectoryInfo (Folder).GetDirectories ();
			for (int _i = 0; _i < _directories.Length; _i++) {
				indexSave++;
				if (indexSave >= _directories.Length) {
					indexSave = 0;
				}
				if (!gameBlackList.Contains (_directories [indexSave].Name)) {
					break;
				}
			}
			lastUsed = _directories [indexSave].Name;
			UpdateSave (true);
			Log ("Next savegame found: " + lastUsed, "QSaveGame");
		}

		private static bool vesselIsOK(ConfigNode node) {
			return node.GetValue("type") != VesselType.Debris.ToString() && node.GetValue("type") != VesselType.Flag.ToString() && node.GetValue("type") != VesselType.SpaceObject.ToString() && node.GetValue("type") != VesselType.Unknown.ToString();
		}

		private static bool vesselIsOK(ProtoVessel pv) {
			return pv.vesselType != VesselType.Debris && pv.vesselType != VesselType.Flag && pv.vesselType != VesselType.SpaceObject && pv.vesselType != VesselType.Unknown;
		}

		private static void UpdateSave(bool force = false) {
			if (saveGame == null || force) {
				saveGame = ConfigNode.Load (saveFile);
			}
			if (saveGame != null) {
				if (saveGame.HasNode ("GAME")) {
					if (hasVesselNode) {
						ConfigNode[] _vessels = saveGame.GetNode ("GAME").GetNode ("FLIGHTSTATE").GetNodes ("VESSEL");
						var _vesselsSort = from _v in _vessels
							orderby double.Parse(_v.GetValue("lct"))
							select _v;
						ConfigNode _lastVessel = _vesselsSort.Last (_v => vesselIsOK (_v));
						if (_lastVessel != null) {
							QuickStart_Persistent.vesselID = _lastVessel.GetValue ("pid");
							vesselName = _lastVessel.GetValue ("name");
							vesselType = _lastVessel.GetValue ("type");
							Log (string.Format ("lastVessel: {0}({1})[{2}]", vesselName, vesselType, QuickStart_Persistent.vesselID), "QSaveGame");
						} else {
							Log ("No lastVessel found", "QSaveGame");
						}
					}
					ConfigNode[] _nodes = saveGame.GetNode ("GAME").GetNodes ("SCENARIO");
					ConfigNode _node = Array.Find (_nodes, n => n.GetValue("name") == "QPersistent");
					if (_node != null) {
						if (hasVesselNode) {
							if (_node.HasValue ("vesselID")) {
								string _vesselName;
								string _vesselType;
								if (Exists (_node.GetValue ("vesselID"), out _vesselName, out _vesselType)) {
									QuickStart_Persistent.vesselID = _node.GetValue ("vesselID");
									vesselName = _vesselName;
									vesselType = _vesselType;
									Log (string.Format ("currentVessel: {0}({1})[{2}]", vesselName, vesselType, QuickStart_Persistent.vesselID), "QSaveGame");
								} else {
									Log ("currentVessel not exist", "QSaveGame");
								}
							} else {
								Log ("No currentVessel found", "QSaveGame");
							}
						} else {
							Log ("There's no vessel on this savegame", "QSaveGame");
						}
					} else {
						Log ("No Scenario found", "QSaveGame");
					}
				}
			}
			Log ("Savegame loaded: " + LastUsed, "QSaveGame");
		}

		private static bool Exists (string vesselID, out string vesselName, out string vesselType) {
			vesselName = string.Empty;
			vesselType = string.Empty;
			if (hasVesselNode) {
				ConfigNode[] _nodes = saveGame.GetNode ("GAME").GetNode ("FLIGHTSTATE").GetNodes ("VESSEL");
				ConfigNode _node = Array.Find (_nodes, n => new Guid (n.GetValue ("pid")) == new Guid (vesselID));
				if (_node != null) {
					vesselName = _node.GetValue ("name");
					vesselType = _node.GetValue ("type");
					return true;
				}
			}
			return false;
		}
		private static bool hasVesselNode {
			get {
				if (saveGame != null) {
					if (saveGame.HasNode ("GAME")) {
						if (saveGame.GetNode ("GAME").HasNode ("FLIGHTSTATE")) {
							if (saveGame.GetNode ("GAME").GetNode ("FLIGHTSTATE").HasNode ("VESSEL")) {
								return true;
							}
						}
					}
				}
				return false;
			}
		}
	}
}

