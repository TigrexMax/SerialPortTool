using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Check.SPort.Utilities
{
    public static class SnackbarService
    {
        public static ISnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

        public static void ShowMessage(string message)
        {
            SnackbarMessageQueue.Enqueue(message);
        }

        public static void ShowMessage(string message, string actionContent, Action action)
        {
            SnackbarMessageQueue.Enqueue(message, actionContent, action);
        }
    }
}
