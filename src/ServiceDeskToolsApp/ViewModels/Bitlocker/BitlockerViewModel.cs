using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels;
public class BitlockerViewModel : Screen
{
	private readonly ActiveDirectory _ad;

	public List<BitlockerSearchViewModel> BitLockerItems { get; set; }

	public BitlockerViewModel(ActiveDirectory ad)
	{
		_ad = ad;

		BitlockerSearchViewModel byID = new("Recovery Key ID", "Key ID", GetKeysByID);
		BitlockerSearchViewModel byComp = new("Computer Name", "Computer Name", GetKeysByComp);

		BitLockerItems = [byID, byComp];
	}

	private List<Bitlocker> GetKeysByComp(string value)
	{
		return _ad.Bitlocker.GetByComputerName(value);
	}
	private List<Bitlocker> GetKeysByID(string value)
	{
		return _ad.Bitlocker.GetByID(value);
	}
}
