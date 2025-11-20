using System;

namespace DataAccess.Models
{
    public class FileForVisit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VisitId { get; set; }
        public DoctorVisit Visit { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;

        public byte[] FileData { get; set; }
    }
}