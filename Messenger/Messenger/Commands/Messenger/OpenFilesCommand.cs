using Messenger.Core.Helpers;
using Messenger.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Messenger.Commands.Messenger
{
    public class OpenFilesCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public OpenFilesCommand(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.FileTypeFilter.Add("*");
                IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();

                if (files.Count > 0)
                {
                    var attachmentPaths = files.Select(f => f.Path).ToList();

                    _viewModel.SelectedFiles = files;
                    _viewModel.MessageToSend.AttachmentsBlobName = attachmentPaths;
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while fetching files from the device: {e.Message}");
            }
        }
    }
}
