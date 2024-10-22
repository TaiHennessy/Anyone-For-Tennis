namespace AnyoneForTennis.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

         public string ExceptionMessage { get; set; }    // For customising the simulated error's message when the exception is caught
    }
}
