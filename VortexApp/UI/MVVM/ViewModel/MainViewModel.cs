using Client.Services.Network;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;

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
        public ChatClient client;

        public ObservableCollection<ContactModel> Contacts { get; set; }
        public ObservableCollection<RequestModel> Requests { get; set; }

        private UserModel _userM;
        public UserModel User
        {
            get { return _userM; }
            set
            {
                if (_userM != value)
                {
                    _userM = value;
                    OnPropertyChanged("User");
                }

            }
        }

        public RelayCommand SendCommand { get; set; }

        private ContactModel _selcont;
        public ContactModel SelectedContact
        {
            get { return _selcont; }
            set
            {
                if (_selcont != value)
                {
                    _selcont = value;
                    OnPropertyChanged("SelectedContact");
                }
            }
        }

        private ContactModel _selreq;
        public ContactModel SelectedRequest
        {
            get { return _selreq; }
            set
            {
                if (_selreq != value)
                {
                    _selreq = value;
                    OnPropertyChanged("SelectedRequest");
                }
            }
        }

        public ContactModel? CallingContact { get; set; }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public int RequestsCount => Requests.Count;
        public int ContactsCount => Contacts.Count;

        public MainViewModel()
        {
            Contacts = new();
            Requests = new();

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
                client.SendMessage(Message, SelectedContact.UserID);
                Message = "";
            });

            Requests.CollectionChanged += (s, e) => OnPropertyChanged("RequestsCount");
            Contacts.CollectionChanged += (s, e) => OnPropertyChanged("ContactsCount");
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

            client.TrySendFile(openFileDialog.SafeFileName, SelectedContact.UserID);

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

        public void DownloadFile(VistaFolderBrowserDialog openFolderDialog, string filename)
        {
            string destinationFilePath = openFolderDialog.SelectedPath;
            if (!client.GetFile(filename, destinationFilePath))
            {
                Thread.Sleep(700);
                var acceptFileThread = new Thread(() =>
                {
                    client.GetFile(filename, destinationFilePath);
                });
                acceptFileThread.Start();
            }
        }

        public void ApplyRequest(string userID)
        {
            client.AcceptFriendRequest(Guid.Parse(userID));
            foreach (var req in Requests)
            {
                if (req.UserID == userID)
                {
                    Requests.Remove(req);
                    return;
                }
            }
        }

        public void DeclineRequest(string userID)
        {
            client.DeclineFriendRequest(Guid.Parse(userID));
            foreach (var req in Requests)
            {
                if (req.UserID == userID)
                {
                    Requests.Remove(req);
                    return;
                }
            }
        }
    }
}
