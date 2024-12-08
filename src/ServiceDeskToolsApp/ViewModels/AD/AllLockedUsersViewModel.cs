using EzActiveDirectory;
using EzActiveDirectory.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ServiceDeskToolsApp.ViewModels;
public class AllLockedUsersViewModel
{
    public BindingList<ActiveDirectoryUser> Users { get; set; }

    internal void UpdateUsers(List<ActiveDirectoryUser> users)
    {
        Users = new();
        foreach (ActiveDirectoryUser user in users)
        {
            Users.Add(user);
        }
    }
}
