using Messenger.Core.Helpers;
using Messenger.ViewModels.Pages;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Messenger.Commands.Messenger
{
    /// <summary>
    /// Attach the selected file to the message to be sent
    /// </summary>
    public class AttachFileCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public AttachFileCommand(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Prevents illegal access outside of the view model
        /// </summary>
        public bool CanExecute(object parameter)
        {
            bool canExecute = _viewModel != null
                && _viewModel.MessageToSend != null;

            return canExecute;
        }

        /// <summary>
        /// Opens the file open picker and sets the model for AttachmentsBlobName, if any selected
        /// </summary>
        public async void Execute(object parameter)
        {
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.FileTypeFilter.Add("*");

                // Opens the file picker
                IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();

                if (files.Count > 0)
                {

                    _viewModel.MessageToSend.UploadFileData = new List<Core.Models.UploadData>();
                    foreach (StorageFile file in files)
                    {
                        var str = new Core.Models.UploadData((await file.OpenReadAsync()).AsStream(),file.Path);
                        _viewModel.MessageToSend.UploadFileData.Add(str);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while fetching files from the device: {e.Message}");
            }
        }
    }
}
