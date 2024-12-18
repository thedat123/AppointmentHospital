namespace AppointmentHospital.Configuration.EmailConfiguaration
{
    public class EmailConfiguration
    {
        public string Host { get; set; }    
        public string Password { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public bool EnableSSL { get; set; }
    }
}
