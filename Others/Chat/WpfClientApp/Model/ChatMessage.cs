using System.Collections.ObjectModel;

namespace WpfClientApp.Model
{
    public struct Message
    {
        public int SourceUserId { get; private set; }
        public string Content { get; private set; }

        public Message(int source, string content)
        {
            SourceUserId = source;
            Content = content;
        }
    }

    public class ChatMessages
    {
        public int DestUserId { get; private set; }

        public ObservableCollection<Message> Messages { get; private set; } = new ObservableCollection<Message>();

        public ChatMessages(int dest)
        {
            DestUserId = dest;
        }
    }
}
