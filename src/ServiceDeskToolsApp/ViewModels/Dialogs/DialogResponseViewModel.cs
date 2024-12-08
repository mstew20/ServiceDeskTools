using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskToolsApp.ViewModels
{
    public class DialogResponseViewModel : Screen
    {
        private bool _response;
        private string _message;

        public bool Response
        {
            get { return _response; }
            set
            {
                _response = value;
                NotifyOfPropertyChange(() => Response);
            }
        }
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }


        public async Task Yes()
        {
            Response = true;
            await Close();
        }

        public async Task No()
        {
            Response = false;
            await Close();
        }

        private async Task Close()
        {
            await TryCloseAsync(Response);
        }
    }
}
