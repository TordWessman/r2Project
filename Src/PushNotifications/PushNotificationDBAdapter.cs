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
using System.Collections.Generic;
using R2Core.Common;

namespace R2Core.PushNotifications {

    public interface IPushNotificationStorageDBAdapter : IDBAdapter { }

    public class PushNotificationDBAdapter : SQLDBAdapter, IPushNotificationStorageDBAdapter {

        public PushNotificationDBAdapter(ISQLDatabase database) : base (database) { }

        protected override IDictionary<string, string> GetColumns() => new Dictionary<string, string> {
                { "token", "TEXT NOT NULL" },
                { "type", "INTEGER NOT NULL" },
                { "group", "TEXT NOT NULL" }
            };

        protected override IEnumerable<string> GetPrimaryKeys() {
            return new string[] { };
        }

        protected override string GetTableName() => $"{Database.Identifier}_storage";
    }
}
