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

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using R2Core;

namespace R2Core.Audio.ASR
{
	/// <summary>
	/// Aiml to sphinx converter used to 
	/// convert *.aiml patterns into a list of sentences
	/// and an IModelCreator to create a set of models
	/// used by the CMU Sphinx engine.
	/// </summary>
	public class AimlToSphinxConverter
	{
		private IModelCreator m_modelCreator;
		private IList<string> m_data;
		
		public AimlToSphinxConverter (IModelCreator languageUpdater)
		{
			m_modelCreator = languageUpdater;
			Reset ();
		}
		
		public void Reset ()
		{
			m_data = new List<string> ();
		}
		
		public void ParseFiles (string path, string fileName = null, int maxCount = int.MaxValue)
		{
			int count = 0;
			foreach (string file in Directory.GetFiles(path, fileName)) {
				
				if (ParseFile (file)) {
					return;
				}
				if (count++ >= maxCount) {
					return;
				}
			}
			
			
		}
		
		public void CreateLanguageFiles (string corpusFileName, string wordsFileName)
		{
			m_modelCreator.CreateLanguageFiles (m_data, corpusFileName, wordsFileName);
		}
		
		public bool ParseFile (string fileName)
		{
			Console.WriteLine ("\nReading: " + fileName);
			XmlDocument doc = new XmlDocument ();
			
			doc.LoadXml (File.ReadAllText (fileName));

			
			foreach (XmlNode element in doc.DocumentElement.SelectNodes("//pattern")) {
				
				string res = Clean (element.InnerText);
				if (Console.KeyAvailable)  {
					
					ConsoleKeyInfo key = Console.ReadKey (true); 
					
					switch (key.Key) {  
						case ConsoleKey.Escape:  
							return true;  
						default:  
							break;  
					}  

				}
				
				if (!m_data.Contains (res)) {
					m_data.Add (res);
					Console.Write (".");
				} else {
					Console.Write ("!");
				}
				
			}
			
			return false;
		}
		
		
		/// <summary>
		/// Clean the specified input. Removes non alpha-numerics
		/// trims, lowercases and replaces numbers with string
		/// representations.
		/// </summary>
		/// <param name='input'>
		/// Input.
		/// </param>
		public string Clean (string input)
		{

			Regex rgx = new Regex ("[^a-zA-Z0-9 -]");
			string replaced = rgx.Replace (input, "").ToUpper ().Trim ();
			rgx = new Regex (@"\d+");
			
			foreach (Match m in rgx.Matches(replaced)) {
				try {
					replaced = replaced.Replace (m.Groups [0].Value, NumberToWords (int.Parse (m.Groups [0].Value)));
				} catch (OverflowException ex) {
					Log.w ("BOOOM: " + ex.Message + " " + m.Groups [0].Value);
					replaced = replaced.Replace (m.Groups [0].Value, NumberToWords (42));

				}
			
			}
			
			return replaced.Replace("-"," ");
		}
		
		
		
//http://stackoverflow.com/questions/2729752/converting-numbers-in-to-words-c-sharp
public string NumberToWords(int number)
{
    if (number == 0)
        return "zero";

    if (number < 0)
        return "minus " + NumberToWords(Math.Abs(number));

    string words = "";

    if ((number / 1000000) > 0)
    {
        words += NumberToWords(number / 1000000) + " million ";
        number %= 1000000;
    }

    if ((number / 1000) > 0)
    {
        words += NumberToWords(number / 1000) + " thousand ";
        number %= 1000;
    }

    if ((number / 100) > 0)
    {
        words += NumberToWords(number / 100) + " hundred ";
        number %= 100;
    }

    if (number > 0)
    {
        if (words != "")
            words += "and ";

        var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

        if (number < 20)
            words += unitsMap[number];
        else
        {
            words += tensMap[number / 10];
            if ((number % 10) > 0)
                words += "-" + unitsMap[number % 10];
        }
    }

    return words;
}


		
		
	}
}

