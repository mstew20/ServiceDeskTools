using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
public class GroupSearchViewModel : Screen
{
    private string _searchTerm;
    private readonly ActiveDirectory _ad;
    private ActiveDirectoryGroup _selectedGroup;

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            _searchTerm = value;
            NotifyOfPropertyChange();
        }
    }
    public BindingList<ActiveDirectoryGroup> SearchedGroups { get; set; } = new();
    public ActiveDirectoryGroup SelectedGroup
    {
        get { return _selectedGroup; }
        set
        {
            _selectedGroup = value;
            NotifyOfPropertyChange();
        }
    }

    public ICommand SearchCommand { get; }

    public GroupSearchViewModel(ActiveDirectory ad)
    {
        _ad = ad;

        SearchCommand = new RelayCommand(Search);
    }

    public void Search()
    {
        SearchedGroups.Clear();
        var groups = _ad.Groups.Find(SearchTerm);
        foreach (var group in groups)
        {
            SearchedGroups.Add(group);
        }
    }
}
