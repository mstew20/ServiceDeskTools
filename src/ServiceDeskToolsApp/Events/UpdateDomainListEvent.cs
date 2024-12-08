using ServiceDeskToolsApp.Models;

namespace ServiceDeskToolsApp.Events;
internal class UpdateDomainListEvent
{
	public AvailableDomain NewDomain { get; }
	public UpdateDomainListEvent(AvailableDomain newDomain)
	{
		NewDomain = newDomain;
	}
}
