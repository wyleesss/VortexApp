using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

//using VortexApp.UI.Core;
//using VortexApp.UI.MVVM.Model;
//using Client.Services.Network;
//using Client.Services.Network.Utilits;
using VortexApp.UI.Core;
using VortexApp.UI.MVVM.Model;

namespace VortexApp.UI.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        //private ChatClient _client;

        public ObservableCollection<ContactModel> Contacts { get; set; }
        public ObservableCollection<RequestModel> Requests { get; set; }

        public UserModel User { get; set; }

        public RelayCommand SendCommand { get; set; }

        public ContactModel SelectedContact { get; set; }
        public ContactModel? CallingContact { get; set; }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public int RequestsCount => Requests.Count;

        public MainViewModel()
        {
            //_client = new(10000, "127.0.0.1", "pidr228@gmail.com", "pidr228", "127.0.0.1");
            //_client.Init();
            //Thread.Sleep(500);

            Contacts = new();
            Requests = new();

            CallingContact = new()
            {
                Username = "AAA",
                ImageSource = "./UI/Resources/DefaultUserLogo.png"
            };

            User = new();
            User.Username = "Username";
            User.UserID = "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4";
            User.ImageSource = "./UI/Resources/DefaultUserLogo.png";

            SendCommand = new RelayCommand(o =>
            { 
                if (Message == string.Empty || Message == null)
                {
                    return;
                }

                SelectedContact?.Messages.Add(new MessageModel
                {
                    Username = User.Username + " (You)",
                    Time = DateTime.Now,
                    ImageSource = "./UI/Resources/DefaultUserLogo.png",
                    Message = Message,
                    FirstMessage = true
                });

                //_client.SendMessage(Message, SelectedContact.UserID);

                Message = "";
            });

            Requests.CollectionChanged += (s, e) => OnPropertyChanged("RequestsCount");

            Contacts.Add(new ContactModel()
            {
                Username = "Testing!",
                UserID = "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4",
                ImageSource = "./UI/Resources/DefaultUserLogo.png",
                Messages = new ObservableCollection<MessageModel>()
            });

            Contacts.Add(new ContactModel()
            {
                Username = "why",
                UserID = "B6BF-329BF39FA1E4-F9168C5E-CEB2-4faa",
                ImageSource = "./UI/Resources/DefaultUserLogo.png",
                Messages = new ObservableCollection<MessageModel>()
            });

            for (int i = 0; i < 10; i++)
            {
                Contacts[0].Messages.Add(new MessageModel()
                {
                    Username = "Testing!",
                    ImageSource = "./UI/Resources/DefaultUserLogo.png",
                    Message = "test!",
                    Time = DateTime.Now,
                    IsNativeOrigin = true,
                    FirstMessage = true
                });
            }


            Requests.Add(new RequestModel()
            {
                Username = "TestRequestNameWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
                UserID = "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4",
                ImageSource = "./UI/Resources/DefaultUserLogo.png"
            });

            Requests.Add(new RequestModel()
            {
                Username = "TestRequest",
                UserID = "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4",
                ImageSource = "./UI/Resources/DefaultUserLogo.png"
            }); ;
            Requests.Add(new RequestModel()
            {
                Username = "TestRequest",
                UserID = "F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4",
                ImageSource = "./UI/Resources/DefaultUserLogo.png"
            });
        }

        public void SendFile(Microsoft.Win32.OpenFileDialog openFileDialog)
        {
            string selectedFilePath = openFileDialog.FileName;
            string solutionFolderPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string destinationFolderPath = Path.Combine(solutionFolderPath, "Services//FileSend//FileBuff");

            if (Directory.Exists(destinationFolderPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(destinationFolderPath);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
            }
            else
            {
                Directory.CreateDirectory(destinationFolderPath);
            }

            string destinationFilePath = Path.Combine(destinationFolderPath, Path.GetFileName(selectedFilePath));
            File.Copy(selectedFilePath, destinationFilePath);

            SelectedContact.Messages.Add(new MessageModel
            {
                Message = "file: " + openFileDialog.SafeFileName,
                Username = User.Username + " (You)",
                Time = DateTime.Now,
                ImageSource = "./UI/Resources/DefaultUserLogo.png",
                Color = "#FFC2C2C2",
                IsFocusable = false,
                IsClickable = true,
                Cursor = "Arrow",
                FontWeight = "Normal",
                FirstMessage = true
            });
        }

        public void DownloadFile(VistaFolderBrowserDialog openFolderDialog)
        {
            string destinationFilePath = openFolderDialog.SelectedPath;
            string solutionFolderPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string selectedFolderPath = Path.Combine(solutionFolderPath, "Services//FileSend//FileBuff//");

            selectedFolderPath += Directory.GetFiles(selectedFolderPath)
                                           .Select(Path.GetFileName)
                                           .ToArray()
                                           .ElementAt(0);

            string fileName = Path.GetFileName(selectedFolderPath);

            destinationFilePath = Path.Combine(destinationFilePath, fileName);

            File.Copy(selectedFolderPath, destinationFilePath, true);
        }
    }
}
