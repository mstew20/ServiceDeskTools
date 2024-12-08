using System.Collections.Generic;

namespace ServiceDeskToolsApp.Models
{
    public class AvailableDomainSet
    {

        public void Initialize(List<AvailableDomain> domains)
        {
            Domains = domains;
        }

        public List<AvailableDomain> Domains { get; set; }
    }
}