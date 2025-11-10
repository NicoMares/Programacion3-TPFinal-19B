using System;

namespace CallCenter.Domain.Entities
{
    public class Incident
    {
        public Guid Id { get; set; }
        public int CustomerId { get; set; }
        public int IncidentTypeId { get; set; }
        public int PriorityId { get; set; }
        public string Problem { get; set; }
        public string Status { get; set; }         
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid AssignedToUserId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string CustomerEmail { get; set; }    
    }
}
