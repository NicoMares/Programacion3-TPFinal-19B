using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using CallCenter.Domain.Abstractions;
using CallCenter.Domain.Entities;


namespace CallCenter.Business.Repositories
{
    public class IncidentRepository
    {
        private readonly string _cs;
        public IncidentRepository(string cs = null)
        {
            _cs = cs ?? ConfigurationManager.ConnectionStrings["CallCenterDb"].ConnectionString;
        }

        // ======= NUEVO: usuarios asignables (activos y no borrados) =======
        public IList<KeyValuePair<Guid, string>> GetAssignableUsers()
        {
            List<KeyValuePair<Guid, string>> list = new List<KeyValuePair<Guid, string>>();
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id, Username FROM dbo.Users WHERE IsDeleted=0 AND IsBlocked=0 ORDER BY Username;", cn))
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        list.Add(new KeyValuePair<Guid, string>(rd.GetGuid(0), rd.GetString(1)));
                }
            }
            return list;
        }

        // ======= NUEVO: marcar En Análisis =======
        public bool MarkInAnalysis(Guid incidentId)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE dbo.Incidents SET Status=N'En Análisis' WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = incidentId;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ======= NUEVO: reasignar (pone estado Asignado) =======
        public bool Reassign(Guid incidentId, Guid newUserId)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET AssignedToUserId=@uid, Status=N'Asignado'
WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = incidentId;
                    cmd.Parameters.Add("@uid", SqlDbType.UniqueIdentifier).Value = newUserId;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ======= NUEVO: resolver (nota y fecha, estado Resuelto) =======
        public bool Resolve(Guid incidentId, string note)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET Status=N'Resuelto', ResolvedAt=SYSUTCDATETIME(), ResolutionNote=@n
WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = incidentId;
                    cmd.Parameters.Add("@n", SqlDbType.NVarChar, 1000).Value = (object)note ?? DBNull.Value;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ======= NUEVO: cerrar (comentario obligatorio, fecha, estado Cerrado) =======
        public bool Close(Guid incidentId, string closeComment)
        {
            using (SqlConnection cn = new SqlConnection(_cs))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
UPDATE dbo.Incidents
SET Status=N'Cerrado', ClosedAt=SYSUTCDATETIME(), ClosedComment=@c
WHERE Id=@id;", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = incidentId;
                    cmd.Parameters.Add("@c", SqlDbType.NVarChar, 1000).Value = closeComment;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
