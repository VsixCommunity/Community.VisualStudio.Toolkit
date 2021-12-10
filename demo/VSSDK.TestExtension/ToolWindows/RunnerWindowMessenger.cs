using System;

namespace TestExtension
{
    /// <summary>
    /// This service demonstrates a very simple way for a toolbar to communicate 
    /// with a tool window. There are many ways that this can be done, and this
    /// service is by no means "best practice".
    /// 
    /// In fact, there is nothing about this service that restricts its use 
    /// to toolbars and tool windows - it's just raising events. Any sort of
    /// "message passing" can also be used to communicate with the tool window.
    /// 
    public class RunnerWindowMessenger
    {
        public void Send(string message)
        {
            // The tooolbar button will call this method.
            // The tool window has added an event handler
            MessageReceived?.Invoke(this, message);
        }

        public event EventHandler<string> MessageReceived;
    }
}
