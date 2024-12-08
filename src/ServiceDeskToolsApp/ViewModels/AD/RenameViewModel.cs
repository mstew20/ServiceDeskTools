using Caliburn.Micro;
using EzActiveDirectory;
using EzActiveDirectory.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
internal class RenameViewModel : Screen
{
    private string _firstName;
    private string _lastName;
    private string _displayNameUser;
    private string _fullName;
    private string _employeeId;
    private ActiveDirectory _ad;

    public ActiveDirectoryUser User { get; set; }
    public string FirstName
    {
        get { return _firstName; }
        set
        {
            _firstName = value;
            NotifyOfPropertyChange();
        }
    }
    public string LastName
    {
        get { return _lastName; }
        set
        {
            _lastName = value;
            NotifyOfPropertyChange();
        }
    }
    public string DisplayNameUser
    {
        get => _displayNameUser;
        set
        {
            _displayNameUser = value;
            NotifyOfPropertyChange();
        }
    }
    public string FullName
    {
        get => _fullName;
        set
        {
            _fullName = value;
            NotifyOfPropertyChange();
        }
    }
    public string EmployeeId
    {
        get => _employeeId;
        set
        {
            _employeeId = value;
            NotifyOfPropertyChange();
        }
    }

    public ICommand UpdateUserCommand { get; }
    public ICommand CloseCommand { get; }

    public RenameViewModel()
    {
        UpdateUserCommand = new RelayCommand(ApplyChanges);
        CloseCommand = new RelayCommand(async () => await TryCloseAsync());
    }

    public void UpdateUserInfo(ActiveDirectoryUser user, ActiveDirectory ad)
    {
        User = user;
        FirstName = user.FirstName;
        LastName = user.LastName;
        DisplayNameUser = user.DisplayName;
        FullName = user.FullName;
        EmployeeId = user.EmployeeId;
        _ad = ad;
    }

    private void ApplyChanges()
    {
        if (FirstName != User.FirstName)
        {
            _ad.Users.SaveProperty(User.Path, Property.FirstName, FirstName);
        }

        if (LastName != User.LastName)
        {
            _ad.Users.SaveProperty(User.Path, Property.LastName, LastName);
        }

        if (DisplayNameUser != User.DisplayName)
        {
            _ad.Users.SaveProperty(User.Path, Property.DisplayName, DisplayNameUser);
        }

        if (FullName != User.FullName)
        {
            _ad.Users.SaveProperty(User.Path, Property.Name, FullName);
        }

        if (EmployeeId != User.EmployeeId)
        {
            _ad.Users.SaveProperty(User.Path, Property.EmployeeId, EmployeeId);
        }

        TryCloseAsync();
    }
}
