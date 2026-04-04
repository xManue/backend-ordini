using System;

namespace Backend.Core.Models
{
    public class TableModel
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = Guid.NewGuid().ToString("N");
        public bool IsActive { get; set; } = true;
    }
}
