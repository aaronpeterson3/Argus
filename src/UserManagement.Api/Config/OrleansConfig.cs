namespace UserManagement.Api;

public class OrleansConfig
{
    public string ClusterId { get; set; } = "development";
    public string ServiceId { get; set; } = "UserManagementApp";
    public int SiloPort { get; set; } = 11111;
    public int GatewayPort { get; set; } = 30000;
    public PasswordHashConfig PasswordHashConfig { get; set; } = new();
}

public class PasswordHashConfig
{
    public int IterationCount { get; set; } = 100000;
    public int NumBytesRequested { get; set; } = 32;
}