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
using System.Data;
using R2Core.Common;
using System.Linq;

namespace R2Core.PushNotifications {

    public class PushNotificationDBAdapter : SQLDBAdapter, IPushNotificationDBAdapter {

        public PushNotificationDBAdapter(ISQLDatabase database) : base (database) { }

        protected override IDictionary<string, string> GetColumns() {
            return new Dictionary<string, string> {
                { "token", "TEXT NOT NULL" },
                { "type", "INTEGER NOT NULL" },
                { "identity", "TEXT NOT NULL" },
                { "target_group", "TEXT NOT NULL" },
                { "description", "TEXT" }
            };
        }

        protected override string GetTableName() => $"{Database.Identifier}_storage";

        public void Save(PushNotificationRegistryItem item) {

            if (Get(item.IdentityName, item.Group).Any(i => i.Token == item.Token)) { return; }

            IList<dynamic> values = new List<dynamic> {
                item.Token,
                $"{(int)item.ClientType}",
                item.IdentityName,
                item.Group,
                item.Description ?? "",
            };

            string sql = InsertSQL(values);

            Database.Insert(sql);

        }

        public void Remove(string token, string group = null) {

            string sql = DeleteSQL($@"token = ""{token}""" + (group != null ? $@" AND target_group = ""{group}""" : ""));

            Database.Query(sql);

        }

        public IEnumerable<PushNotificationRegistryItem>Get(string identity, string group = null) {

            string sql = SelectSQL($@"identity = ""{identity}""" + (group != null ? $@" AND target_group = ""{group}""" : ""));
            DataSet result = Database.Select(sql);

            foreach (DataRow row in result.Tables[0].Rows) {

                yield return row.CreateItem();

            }

        }

    }

    internal static class DataRowExtensions {

        public static PushNotificationRegistryItem CreateItem(this DataRow self) {

            return new PushNotificationRegistryItem {

                Token = self["token"].ToString(),
                IdentityName = self["identity"].ToString(),
                Group = self["target_group"].ToString(),
                ClientType = (PushNotificationClientType)int.Parse(self["type"].ToString()),
                Description = self["description"].ToString()

            };

        }

    }

}
