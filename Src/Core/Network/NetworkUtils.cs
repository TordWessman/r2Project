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
using Core.Network.Data;


namespace Core.Network
{
	public class NetworkUtils
	{
		private NetworkUtils ()
		{
		}
		/*
		public static byte [] RemovePackageTypeHeader (byte[] input)
		{
			byte [] output = new byte[input.Length - RawPackageFactory.PACKAGE_TYPE_SIZE];
			
			Array.Copy (input, RawPackageFactory.PACKAGE_TYPE_SIZE, output, 0, output.Length);
			
			return output;
		}*/
		
		public static PackageTypes GetBasePackageType (byte[]input)
		{

			if (input.Length > RawPackageFactory.PACKAGE_TYPE_POSITION) {
			
				return (PackageTypes) input[RawPackageFactory.PACKAGE_TYPE_POSITION];

			}

			return PackageTypes.Unknown;

		}
		
		public static DataPackageType GetSubPackageType (byte[]input)
		{

			if (input.Length > RawPackageFactory.PACKAGE_SUB_TYPE_POSITION) {

				return (DataPackageType) input[RawPackageFactory.PACKAGE_SUB_TYPE_POSITION];

			}

			return DataPackageType.Unknown;

		}

	}

}