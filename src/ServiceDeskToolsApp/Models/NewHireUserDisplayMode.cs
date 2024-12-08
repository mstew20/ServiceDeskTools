using Caliburn.Micro;

namespace ServiceDeskToolsApp.Models
{
    public class NewHireUserDisplayMode : PropertyChangedBase
    {
        private string _userName = "";
        private string _password = "";
        private string _email = "";

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeID { get; set; }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                NotifyOfPropertyChange();
            }
        }
        public string TempPassword
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyOfPropertyChange();
            }
        }
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                NotifyOfPropertyChange();
            }
        }

        public string Path { get; internal set; }
    }
}
