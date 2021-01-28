using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Data.SQLite;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DragDrop
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public SQLiteConnection my;
        public RSACryptoServiceProvider csp;
        public RSAParameters _privateKey;
        public MainPage()
        {
            
            string connStr = "Data Source=storing.sqlite3";
            this.InitializeComponent();
            my = new SQLiteConnection(connStr);
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var storageFile = items[0] as StorageFile;
                    var contentType = storageFile.ContentType;
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    if (contentType == "image/jpg" || contentType == "image/png" || contentType == "image/jpeg")
                    {
                        StorageFile newFile = await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                        var bitmapImg = new BitmapImage();
                        bitmapImg.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                        imgMain.Source = bitmapImg;
                        var x = File.ReadAllBytes(newFile.Path);
                        x = Encrypt(x);
                        using (var cmd = new SQLiteCommand())
                        {
                            cmd.CommandText = "INSERT INTO StoreData('Name','image') VALUES(@Name, @img)";
                            my.Open();
                            cmd.Parameters.Add("@img",System.Data.DbType.Binary,x.Length);
                            cmd.Parameters.AddWithValue("@img", x);
                            cmd.Parameters.AddWithValue("@Name", descriptionBox.Text);
                        }
                    }
                    else if (contentType == "music/mp3" || contentType == "music/mpe3")
                    {
                        StorageFile storage = await storageFile.CopyAsync(folder,storageFile.Name,NameCollisionOption.GenerateUniqueName);
                        //mediaPlayer.SetSource(await storageFile.OpenAsync(st)));
                        //mediaPlayer.Play();
                    }
                }
            }
        }
        private byte[] Encrypt(byte[] data)
        {
            csp  = new RSACryptoServiceProvider();
            _privateKey = csp.ExportParameters(true);

            return HashAndSign(data, _privateKey);
        }
        private byte[] HashAndSign(byte[] data, RSAParameters _privateKey)
        {
            
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(_privateKey);
            return csp.Encrypt(data, true);
        }
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            // TO specifie which operation are allowed
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            // TO display the data dragged
            e.DragUIOverride.Caption = "Drop here to view image";
            e.DragUIOverride.IsGlyphVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsCaptionVisible = true;
        }
    }
}
