// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
// 
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
// 

﻿using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Core.Device;

namespace Core
{

	public class DelmeConfig : DeviceBase
	{
		private Dictionary<string,dynamic> m_constants;
		private static DelmeConfig m_instance;
		private static readonly object m_instanceLock = new object ();
		private static readonly object m_dataLoadLock = new object ();
		private IList<string> m_dataLoaded;

		private DelmeConfig () : base("config")
		{

			string aa = "";

			aa.ToLower ();

			ReloadConstants ();

		}


		public ConstClass Consts = new ConstClass();

		public class ConstClass {
		
			public int SomeVal() {

				return DelmeConfig.Instance.GetValue<int> ("SomeVal");
			}
		}

		/// <summary>
		/// Resets configuration settings for constants and forces them to be reloaded (lazily).
		/// </summary>
		public void ReloadConstants() {
			m_constants = new Dictionary<string,dynamic> ();
			m_dataLoaded = new List<string> ();
		}

		/// <summary>
		/// Adds a value to the Const dictionary.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		protected void AddConstant(string key, dynamic value) {
			if (m_constants.ContainsKey (key)) {
				//TODO: print error
			} else {

				m_constants.Add (key, value);
			}

		}

		/// <summary>
		/// Returns a static instance of the settings. Creates an instance if needed.
		/// </summary>
		/// <value>The instance.</value>
		public static DelmeConfig Instance {
			get {
				lock (m_instanceLock) {
					if (DelmeConfig.m_instance == null) {
						DelmeConfig.m_instance = new DelmeConfig ();
					}

					return m_instance;
				}
			}
		}

		/// <summary>
		/// Checks if the data for key have been loaded and loads the data at runtime if required.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="configFileName">Config file name.</param>
		public void CheckAndLoadData(string key, string configFileName) {

			lock (m_dataLoadLock) {

				if (!DelmeConfig.Instance.m_dataLoaded.Contains (key)) {
					DelmeConfig.LoadConstData (configFileName);

					DelmeConfig.Instance.m_dataLoaded.Add (key);
				}
			}
		}

		/// <summary>
		/// Returns a value of type T using key K or the default value for T
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetValue<T>(string key) {

			if (DelmeConfig.Instance.m_constants.ContainsKey (key)) {
				return (T) DelmeConfig.Instance.m_constants [key];
			}

			return default(T);

		}

		/// <summary>
		/// Loads and returns an XmlDocument.
		/// </summary>
		/// <returns>The config xml.</returns>
		/// <param name="file">File.</param>
		private static XmlDocument GetConfigXml(string file) {

			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = false;
			try { doc.Load(file); }
			catch (System.IO.FileNotFoundException)
			{}

			return doc;
		}

		/// <summary>
		/// Loads configuration file data from parameter
		/// </summary>
		/// <param name="configFileName">Config file name.</param>
		public static void LoadConstData(string configFileName) {

			foreach (XmlNode pathsNode in GetConfigXml(configFileName).SelectNodes("/Configuration/Consts")) {
				foreach (XmlNode node in pathsNode.ChildNodes) { 
					DelmeConfig.Instance.AddConstant (node.Name, node.Attributes["type"] != null ? ParseType(node.Attributes["type"].Value, node.Value) : node.Value  );
				}

			}
		}

		/// <summary>
		/// Returns a dynamic parsing of value using description as type identifier.
		/// </summary>
		/// <returns>The type.</returns>
		/// <param name="description">Description.</param>
		/// <param name="value">Value.</param>
		private static dynamic ParseType(string description, string value) {
			switch (description) {
			case "int":
				return int.Parse (value);
			default:
				return typeof(string);
			}
		}

	}



	public static class ConstClassExtension {

		public static string SomeVal2(this DelmeConfig.ConstClass self) {

			DelmeConfig.Instance.CheckAndLoadData ("ConstXtension", "constant/path..");

			return DelmeConfig.Instance.GetValue<string> ("SomeVal2");
		}


	}


}
	

