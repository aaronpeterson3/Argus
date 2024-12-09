namespace Argus.Abstractions.Models
{
    public class PasswordHashConfig
    {
        public int IterationCount { get; set; } = 10000;
        public int NumBytesRequested { get; set; } = 256 / 8;
    }
}