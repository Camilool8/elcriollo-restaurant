namespace ElCriollo.API.Models.DTOs.Request
{
    public class ActualizarOrdenRequest
    {
        public int OrdenID { get; set; }
        public string? Observaciones { get; set; }
        public List<ItemOrdenRequest> Items { get; set; } = new();
    }
} 