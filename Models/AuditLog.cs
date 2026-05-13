namespace JobPortalCORE.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Kis User/Admin ne change kiya (Identity wala ID)
        public string TableName { get; set; } // Kis table mein change hua (e.g., "Employees")
        public string Action { get; set; } // Action kya tha (INSERT, UPDATE, ya DELETE)
        public string OldValues { get; set; } // Update se pehle purana data kya tha
        public string NewValues { get; set; } // Update ke baad naya data kya hai
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Kab hua (Server Time)
    }
}