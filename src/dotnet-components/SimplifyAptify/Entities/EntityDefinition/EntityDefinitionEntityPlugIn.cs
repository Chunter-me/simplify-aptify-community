using Aptify.Framework.BusinessLogic.GenericEntity;
using Aptify.Framework.ExceptionManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimplifyAptify.Entities.EntityDefinition
{
    public class EntityDefinitionEntityPlugIn : AptifyGenericEntity
    {
        public override bool Save(bool AllowGUI, ref string ErrorString, string TransactionID)
        {
            try
            {
                var fields = new Dictionary<string, string>();
                var links = new List<string>();

                // Get Entity Fields and Help (Support) Links
                using (var reader = DataAction.ExecuteDataReaderParametrized(
                    "spEntityDefinitionApp_sa",
                    System.Data.CommandType.StoredProcedure,
                    new System.Data.IDataParameter[]
                    {
                        DataAction.GetDataParameter("@AppContent", System.Data.SqlDbType.NVarChar, "Generator"),
                        DataAction.GetDataParameter("@RecordID", System.Data.SqlDbType.Int, GetValue("EntityID"))
                    }
                ))
                {
                    // Entities
                    if (reader.Read() && string.IsNullOrWhiteSpace(GetValue("Definition")?.ToString() ?? string.Empty))
                    {
                        SetValue("Definition", reader.GetString(reader.GetOrdinal("Description")));
                    }

                    reader.NextResult();

                    // Fields
                    while (reader.Read())
                    {
                        fields.Add(
                            reader.GetString(reader.GetOrdinal("Name")),
                            reader.GetString(reader.GetOrdinal("Description"))
                        );
                    }

                    reader.NextResult();

                    // Support Links
                    while (reader.Read())
                    {
                        links.Add(reader.GetString(reader.GetOrdinal("Url")));
                    }
                }

                // Entity Fields
                var fieldsSubType = SubTypes["EntityDefinitionField_sa"];
                if (fields.Count > 0)
                {
                    // Add Fields and/or Update Description if Blank
                    foreach (var field in fields)
                    {
                        var fieldGe = fieldsSubType.Find("Field", field.Key) ?? fieldsSubType.Add();

                        if (string.IsNullOrWhiteSpace(fieldGe.GetValue("Field")?.ToString() ?? string.Empty))
                        {
                            fieldGe.SetValue("Field", field.Key);
                        }

                        if (string.IsNullOrWhiteSpace(fieldGe.GetValue("Description")?.ToString() ?? string.Empty))
                        {
                            fieldGe.SetValue("Description", field.Value);
                        }
                    }

                    // Remove Fields no longer exist on Entity
                    for (int i = 0; i < fieldsSubType.Count; i++)
                    {
                        var fieldGe = fieldsSubType[i];
                        if (fieldGe != null)
                        {
                            var fieldName = fieldGe.GetValue("Field")?.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(fieldName) && !fields.ContainsKey(fieldName))
                            {
                                fieldsSubType.Remove(fieldGe);
                            }
                        }
                    }
                }

                // Support Links
                var supportLinksSubType = SubTypes["EntityDefinitionSupportLink_sa"];
                if (links.Count > 0)
                {
                    foreach (var url in links)
                    {
                        var linkGe = supportLinksSubType.Find("Url", url) ?? supportLinksSubType.Add();

                        if (string.IsNullOrWhiteSpace(linkGe.GetValue("Url")?.ToString() ?? string.Empty))
                        {
                            linkGe.SetValue("Url", url);
                        }

                        if (string.IsNullOrWhiteSpace(linkGe.GetValue("Title")?.ToString() ?? string.Empty))
                        {
                            var title = url;

                            if (title.Contains("kb.aptify.com") || title.Contains("aptifykb.atlassian.net"))
                            {
                                title = title.Split('/').Last().Replace("+", " ");
                            }

                            linkGe.SetValue("Title", title);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
            }

            return base.Save(AllowGUI, ref ErrorString, TransactionID);
        }
    }
}