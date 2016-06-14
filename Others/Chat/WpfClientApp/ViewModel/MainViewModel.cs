using Client;
using GalaSoft.MvvmLight;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfClientApp.Model;

namespace WpfClientApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public int UserId { get; }
        private ChatClient _client;

        public ObservableCollection<ChatMessages> Messages { get; set; } = new ObservableCollection<ChatMessages>();
        public string MessageToSend { get; set; }
        public int CurrentIndex { get;
            set; }

        private object _syncObject = new object();

        public ICommand LogOnCommand
        {
            get; private set;
        }

        public ICommand SendCommand
        {
            get; private set;
        }

        public MainViewModel(ChatClient client, LogOnRequestResult result)
        {
            _client = client;
            UserId = client.UserId;

            foreach (var u in result.OnlineUsers)
                if (u != UserId)
                    Messages.Add(new ChatMessages(u));

            _client.OnGotMessage += (o, e) =>
            {
                Action del = () =>
                {
                    foreach (var item in Messages)
                    {
                        if (item.DestUserId == e.SourceId)
                            item.Messages.Add(new Message(e.SourceId, e.ChatContent));
                    }
                };
                    
                Application.Current.Dispatcher.BeginInvoke(del);
            };

            _client.OnGotNewBody += (o, e) =>
            {
                Action del = () =>
                Messages.Add(new ChatMessages(e.NewUserId));
                Application.Current.Dispatcher.BeginInvoke(del);
            };

            SendCommand = new DelegateCommand(() =>
            {
                var current = Messages[CurrentIndex];
                _client.Chat(current.DestUserId, MessageToSend);
                current.Messages.Add(new Message(UserId, MessageToSend));
            },
                () => MessageToSend != string.Empty);
        }
    }
}