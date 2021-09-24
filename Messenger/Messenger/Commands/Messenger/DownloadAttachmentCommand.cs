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
    /// <summary>
    /// Download selected attachment to local storage
    /// </summary>
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

                var folderPicker = new FolderPicker();
                folderPicker.SuggestedStartLocation = PickerLocationId.Downloads;
                folderPicker.FileTypeFilter.Add("*");

                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    // Application now has read/write access to all contents in the picked folder
                    // (including other sub-folder contents)
                    Windows.Storage.AccessCache.StorageApplicationPermissions.
                    FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                    MemoryStream outP = await FileSharingService.Download(target.ToBlobName());
                    SaveToStorage(outP, target.FileName+"."+target.FileType, folder);
                    await ResultConfirmationDialog
                       .Set(true, $"Download successful (Path: {destinationDirectory}")
                       .ShowAsync();

                    return;
                }

            }
            catch (Exception e)
            {
                _logger.Information($"Error while fetching files from the device: {e.Message}");
                await ResultConfirmationDialog
                        .Set(false, $"Download failed. Try again.")
                        .ShowAsync();
            }
        }
        
        /// <summary>
        /// Safes an attachment to the local storage
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filename"></param>
        /// <param name="folder"></param>
        /// <return></return>
        public async void SaveToStorage(MemoryStream stream, string filename, StorageFolder folder)
        {
            //var localFolder = ApplicationData.Current.LocalFolder;
            var storageFile = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            using (Stream x = await storageFile.OpenStreamForWriteAsync())
            {
                x.Seek(0, SeekOrigin.Begin);
                stream.WriteTo(x);
            }
        }
    }
}
