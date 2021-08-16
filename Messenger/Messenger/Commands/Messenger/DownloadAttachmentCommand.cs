using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers.MessageHelpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
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
        private readonly MessageViewModel _viewModel;
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public DownloadAttachmentCommand(MessageViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Opens the file open picker and sets the model for AttachmentsBlobName, if any selected
        /// </summary>
        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is Attachment;

            if (!executable) return;

            try
            {
                Attachment target = parameter as Attachment;
                string destinationDirectory = UserDataPaths.GetDefault().Downloads;

                bool success = await FileSharingService.Download(target.ToBlobName(), destinationDirectory);

                if (success)
                {
                    await ResultConfirmationDialog
                        .Set(true, $"Download successful (Path: {destinationDirectory}")
                        .ShowAsync();

                    return;
                }

                _logger.Information($"Download complete. {attachment.FileName}.{attachment.FileType}");
            }
            catch (Exception e)
            {
                _logger.Information($"Error while fetching files from the device: {e.Message}");
                await ResultConfirmationDialog
                        .Set(false, $"Download failed. Try again.")
                        .ShowAsync();
            }
        }
    }
}
