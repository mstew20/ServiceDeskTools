using ServiceDeskToolsApp.Models;

namespace ServiceDeskToolsApp.Events;
internal record UpdateDomainListEvent(AvailableDomain Domain, UpdateAction Action);

internal enum UpdateAction
{
	Add,
	Delete
}