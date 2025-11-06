using System;
using System.Collections.Generic;
using CallCenter.Domain.Entities;

namespace CallCenter.Domain.Abstractions
{
    public interface IIncidentRepository
    {
        Guid Insert(Incident inc);
        IList<KeyValuePair<int, string>> GetIncidentTypes();
        IList<KeyValuePair<int, string>> GetPriorities();
        IList<KeyValuePair<int, string>> GetCustomers();
        string GetCustomerEmail(int customerId);
        Guid GetUserIdByUsername(string username);

        // Acciones (si las definiste)
        bool MarkInAnalysis(Guid incidentId);
        bool Reassign(Guid incidentId, Guid newUserId);
        bool Resolve(Guid incidentId, string note);
        bool Close(Guid incidentId, string closeComment);

        // Soporte
        IList<KeyValuePair<Guid, string>> GetAssignableUsers();
    }
}
