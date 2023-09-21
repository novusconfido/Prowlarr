using System.Collections.Generic;
using System.Data;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Datastore.Migration.Framework;
using static NzbDrone.Core.Datastore.Migration.redacted_api;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(39)]
    public class freeleech_token_options : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.WithConnection(MigrateOrpheusToTokenOptions);
        }

        private void MigrateOrpheusToTokenOptions(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT \"Id\", \"Settings\" FROM \"Indexers\" WHERE \"Implementation\" = 'Orpheus'";

                var updatedIndexers = new List<Indexer008>();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        var settings = reader.GetString(1);

                        if (!string.IsNullOrWhiteSpace(settings))
                        {
                            var jsonObject = Json.Deserialize<JObject>(settings);

                            if (jsonObject.ContainsKey("useFreeleechToken") && jsonObject.Value<JToken>("useFreeleechToken").Type == JTokenType.Boolean)
                            {
                                var optionValue = jsonObject.Value<bool>("useFreeleechToken") switch
                                {
                                    true => 1, // Preferred
                                    _ => 0 // Never
                                };

                                jsonObject.Remove("useFreeleechToken");
                                jsonObject.Add("useFreeleechToken", optionValue);
                            }

                            settings = jsonObject.ToJson();

                            updatedIndexers.Add(new Indexer008 { Id = id, Settings = settings });
                        }
                    }
                }

                var updateSql = "UPDATE \"Indexers\" SET \"Settings\" = @Settings WHERE \"Id\" = @Id";
                conn.Execute(updateSql, updatedIndexers, transaction: tran);
            }
        }
    }
}
