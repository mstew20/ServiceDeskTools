using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp.ViewModels;
public class PingViewModel : Screen
{
    private readonly Ping _ping;
    private bool _isStopRequested;
    private string _addressOrComputerName;
    private string _address = "0.0.0.0";
    private int _packetsRecieved;
    private int _packetsSent;
    private bool _isContinuous;
    private int _packetsToSend = 4;
    private long _averageRoundTrip;
    private long _totalTrip;
    private bool _canPing = true;

    public string AddressOrComputerName
    {
        get { return _addressOrComputerName; }
        set
        {
            _addressOrComputerName = value;
            NotifyOfPropertyChange();
        }
    }
    public string Address
    {
        get { return _address; }
        set
        {
            _address = value;
            NotifyOfPropertyChange();
        }
    }
    public int PacketsRecieved
    {
        get { return _packetsRecieved; }
        set
        {
            _packetsRecieved = value;
            NotifyOfPropertyChange();
        }
    }
    public int PacketsSent
    {
        get { return _packetsSent; }
        set
        {
            _packetsSent = value;
            NotifyOfPropertyChange();
        }
    }
    public bool IsContinous
    {
        get { return _isContinuous; }
        set
        {
            _isContinuous = value;
            NotifyOfPropertyChange();
        }
    }
    public int PacketsToSend
    {
        get { return _packetsToSend; }
        set
        {
            _packetsToSend = value;
            NotifyOfPropertyChange();
        }
    }
    public long AverageRoundTrip
    {
        get { return _averageRoundTrip; }
        set
        {
            _averageRoundTrip = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(RoundTrip));
        }
    }
    public long PacketsLost => PacketsSent - PacketsRecieved;
    public string RoundTrip => $"{AverageRoundTrip} ms";
    public string Lost => $"{PacketsLost} ({100 * PacketsLost / (PacketsSent == 0 ? 1 : PacketsSent)})%";
    public bool CanPing
    {
        get => _canPing; set
        {
            _canPing = value;
            NotifyOfPropertyChange();
        }
    }

    public ICommand StopCommand { get; }

    public PingViewModel()
    {
        _ping = new Ping();
        StopCommand = new RelayCommand(Stop);
    }

    public async Task Ping()
    {
        CanPing = false;
        _isStopRequested = false;
        _totalTrip = 0;
        PacketsRecieved = 0;
        PacketsSent = 0;
        Address = "0.0.0.0";
        AverageRoundTrip = 0;
        NotifyOfPropertyChange(nameof(PacketsLost));
        NotifyOfPropertyChange(nameof(Lost));

        if (string.IsNullOrWhiteSpace(AddressOrComputerName))
        {
            return;
        }

        while (PacketsSent < PacketsToSend || (IsContinous && !_isStopRequested))
        {
            PacketsSent++;
            var reply = await _ping.SendPingAsync(AddressOrComputerName);
            if (reply.Status == IPStatus.Success)
            {
                PacketsRecieved++;

                _totalTrip += reply.RoundtripTime;
                AverageRoundTrip = _totalTrip / PacketsRecieved;
                Address = reply.Address.ToString();
            }
            NotifyOfPropertyChange(nameof(PacketsLost));
            NotifyOfPropertyChange(nameof(Lost));
            await Task.Delay(500);
        }
        CanPing = true;
    }
    private void Stop()
    {
        _isStopRequested = true;
    }
}
