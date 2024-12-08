using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels;

public class BitlockerSearchViewModel : Screen
{
	private string _keyId;
	private Bitlocker _selectedItem;
	private string _searchTerm;
	private bool _isSearching;

	public string KeyID
	{
		get { return _keyId; }
		set
		{
			_keyId = value;
			NotifyOfPropertyChange();
		}
	}
	public string SearchTerm
	{
		get { return _searchTerm; }
		set
		{
			_searchTerm = value;
			NotifyOfPropertyChange();
		}
	}
	public Bitlocker SelectedItem
	{
		get { return _selectedItem; }
		set
		{
			_selectedItem = value;
			NotifyOfPropertyChange();
		}
	}
	public bool IsSearching
	{
		get { return _isSearching; }
		set
		{
			_isSearching = value;
			NotifyOfPropertyChange();
		}
	}
	public BindingList<Bitlocker> Keys { get; set; } = new();
	public string Text { get; set; }
	public Func<string, List<Bitlocker>> SearchFunction { get; set; }

	public BitlockerSearchViewModel(string display, string name, Func<string, List<Bitlocker>> search)
	{
		DisplayName = display;
		Text = name;
		SearchFunction = search;
	}

	public async Task Search()
	{
		IsSearching = true;
		Keys.Clear();
		try
		{
			var keys = await Task.Run(() => SearchFunction.Invoke(SearchTerm));
			foreach (var key in keys)
			{
				Keys.Add(key);
			}
			SelectedItem = Keys[0];
		}
		catch{}

		IsSearching = false;
	}

	protected override void OnViewLoaded(object view)
	{
		((dynamic)view).SearchTerm.Focus();
		base.OnViewLoaded(view);
	}
}
