using EzActiveDirectory.Models;
using System.Text.Json.Serialization;

namespace ServiceDeskToolsApp.Models
{
    public class AvailableDomain
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string LdapPath { get; set; }
        public UserCredentials Credentials { get; set; } = new();
        public string DefaultDomain { get; set; }
        [JsonIgnore]
        public string[] DomainControllers { get; set; } = [];
    }
}