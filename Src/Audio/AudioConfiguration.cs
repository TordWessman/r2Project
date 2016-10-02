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
using Core;

namespace Audio {

		public static class PathsAudioExtensions {

			public static string Audio (this Settings.IdentifiersClass self, string fileName = null) {

				if (fileName == null) {
					return Settings.BasePath + "/Resources/Audio";
				}
				return Settings.BasePath + "/Resources/Audio" + System.IO.Path.DirectorySeparatorChar + fileName;
			}

			public static string Language (this Settings.IdentifiersClass self, string fileName = null) {

				if (fileName == null) {
					return Settings.BasePath + "/Resources/Language";
				}
				return Settings.BasePath + "/Resources/Language" + System.IO.Path.DirectorySeparatorChar + fileName;
			}

			public static string hub4wsj_sc_8k (this Settings.IdentifiersClass self, string fileName = null) {

				if (fileName == null) {
					return Settings.BasePath + "/Resources/Language/hub4wsj_sc_8k";
				}
				return Settings.BasePath + "/Resources/Language/hub4wsj_sc_8k" + System.IO.Path.DirectorySeparatorChar + fileName;
			}

			public static string chomskyAIML (this Settings.IdentifiersClass self, string fileName = null) {

				if (fileName == null) {
					return Settings.BasePath + "/Resources/Language/chomskyAIML";
				}
				return Settings.BasePath + "/Resources/Language/chomskyAIML" + System.IO.Path.DirectorySeparatorChar + fileName;
			}

			public static string chatbot (this Settings.IdentifiersClass self, string fileName = null) {

				if (fileName == null) {
					return Settings.BasePath + "/Resources/Language/chatbot";
				}
				return Settings.BasePath + "/Resources/Language/chatbot" + System.IO.Path.DirectorySeparatorChar + fileName;
			}

			public static string aimlConfig (this Settings.IdentifiersClass self, string fileName = null) {

				if (fileName == null) {
					return Settings.BasePath + "/Resources/Language/aimlConfig";
				}
				return Settings.BasePath + "/Resources/Language/aimlConfig" + System.IO.Path.DirectorySeparatorChar + fileName;
			}


		}


		public static class IdentifiersAudioExtensions {


		}


}


