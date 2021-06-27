using Messenger.Core.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.IO;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Messenger.Commands.Messenger
{
    public class DownloadAttachmentCommand : ICommand
    {
        private ChatHubService Hub => Singleton<ChatHubService>.Instance;
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is Attachment;

            if (!executable)
            {
                _logger.Information($"Invalid attachment model.");
                return;
            }

            try
            {
                Attachment attachment = (Attachment)parameter;
                var dialog = new FolderPicker();
                // ..\Downloads

                var folder = await dialog.PickSingleFolderAsync();

                if (folder == null)
                {
                    return;
                }

                string path = Path.GetDirectoryName(folder.Path);

                bool isSuccess = await Hub.DownloadAttachment(attachment, path);

                if (!isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not download the attachment.")
                        .ShowAsync();

                    return;
                }

                _logger.Information($"Download complete. {attachment.FileName}.{attachment.FileType}");
            }
            catch (Exception e)
            {
                _logger.Fatal($"Error while downloading attachments: {e.Message}");
            }
        }
    }
}
