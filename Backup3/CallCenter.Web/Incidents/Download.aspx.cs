using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace CallCenter.Web.Incidents
{
    public partial class Download : System.Web.UI.Page
    {
        private string _cs;

        protected void Page_Load(object sender, EventArgs e)
        {
            // requiere login por web.config
            _cs = System.Configuration.ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;

            Guid id;
            if (!Guid.TryParse(Request.QueryString["id"], out id))
            {
                Response.StatusCode = 400;
                Response.End();
                return;
            }

            string fileName, storedName, contentType;
            Guid incidentId;

            using (var cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
SELECT TOP 1 IncidentId, FileName, StoredName, ContentType
FROM dbo.IncidentAttachments WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read())
                        {
                            Response.StatusCode = 404;
                            Response.End();
                            return;
                        }
                        incidentId = rd.GetGuid(0);
                        fileName = Convert.ToString(rd["FileName"]);
                        storedName = Convert.ToString(rd["StoredName"]);
                        contentType = Convert.ToString(rd["ContentType"]);
                    }
                }
            }

            // (opcional) autorización extra: verificar que el usuario tenga permiso de ver esta incidencia

            string fullPath = Server.MapPath("~/App_Data/Uploads/" + storedName);
            if (!File.Exists(fullPath))
            {
                Response.StatusCode = 404;
                Response.End();
                return;
            }

            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "application/octet-stream";

            Response.Clear();
            Response.ContentType = contentType;
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlPathEncode(fileName) + "\"");
            Response.TransmitFile(fullPath);
            Response.End();
        }
    }
}
