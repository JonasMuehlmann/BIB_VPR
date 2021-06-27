﻿using Messenger.Core.Helpers;
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
                    var attachmentPaths = files.Select(f => f.Path).ToList();

                    // Sets the models in the view model
                    _viewModel.SelectedFiles = files;
                    _viewModel.MessageToSend.UploadFilePaths = attachmentPaths;
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while fetching files from the device: {e.Message}");
            }
        }
    }
}
