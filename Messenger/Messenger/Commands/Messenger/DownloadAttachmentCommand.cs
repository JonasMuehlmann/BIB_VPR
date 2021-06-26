using Messenger.Core.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Views.DialogBoxes;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using System;
using System.IO;
using System.Windows.Input;
using Windows.Storage;

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
                var dialog = new CommonOpenFileDialog();
                // ..\Downloads
                dialog.EnsureValidNames = false;
                dialog.EnsureFileExists = false;
                dialog.EnsurePathExists = true;
                dialog.InitialDirectory = ApplicationData.Current.LocalFolder.Path;
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                {
                    return;
                }

                string path = Path.GetDirectoryName(dialog.FileName);

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
