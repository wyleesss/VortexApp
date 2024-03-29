﻿using System.Collections.ObjectModel;
using System.Windows;
using VortexApp.UI.MVVM.Model;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp.UI.Events
{
    internal class UIEvents
    {
        public static void EndCallUI()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ((MainWindow)Application.Current.MainWindow).CallResponseWaiting.Visibility = Visibility.Hidden;
                ((MainViewModel)Application.Current.MainWindow.DataContext).client.EndCallAccept();
                foreach (var i in ((MainViewModel)Application.Current.MainWindow.DataContext).Contacts)
                {
                    if (i.UserID == ((MainViewModel)Application.Current.MainWindow.DataContext).CallingContact.UserID)
                    {
                        i.Messages.Add(new MessageModel
                        {
                            Username = "Call Status",
                            Time = DateTime.Now,
                            Message = "Call Ended - " + (DateTime.Now - ((MainWindow)Application.Current.MainWindow).callWindow.startTime).ToString(@"hh\:mm\:ss"),
                        });
                    }
                }
                ((MainViewModel)Application.Current.MainWindow.DataContext).CallingContact = null;
                ((MainWindow)Application.Current.MainWindow).callWindow.Hide();
            }));
        }
        public static void CancelCallUI()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ((MainWindow)Application.Current.MainWindow).CallResponseWaiting.Visibility = Visibility.Hidden;
                foreach (var i in ((MainViewModel)Application.Current.MainWindow.DataContext).Contacts)
                {
                    if (i.UserID == ((MainViewModel)Application.Current.MainWindow.DataContext).CallingContact.UserID)
                    {
                        i.Messages.Add(new MessageModel
                        {
                            Username = "Call Status",
                            Time = DateTime.Now,
                            Message = "Call has been cancelled",
                        });
                    }
                }
                ((MainViewModel)Application.Current.MainWindow.DataContext).CallingContact = null;
            }));
        }
        public static void AcceptCallUI(string IP)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ((MainWindow)Application.Current.MainWindow).CallResponseWaiting.Visibility = Visibility.Hidden;
                ((MainViewModel)Application.Current.MainWindow.DataContext).client.StartCallAccept(IP);
                foreach (var i in ((MainViewModel)Application.Current.MainWindow.DataContext).Contacts)
                {
                    if (i.UserID == ((MainViewModel)Application.Current.MainWindow.DataContext).CallingContact.UserID)
                    {
                        i.Messages.Add(new MessageModel
                        {
                            Username = "Call Status",
                            Time = DateTime.Now,
                            Message = "Call Started",
                        });
                    }
                }
                ((MainWindow)Application.Current.MainWindow).CallRequest.Visibility = Visibility.Hidden;
                ((MainWindow)Application.Current.MainWindow).callWindow.Title = ((MainViewModel)Application.Current.MainWindow.DataContext).CallingContact.Username + " - Call";
                ((MainWindow)Application.Current.MainWindow).callWindow.timer.Start();
                ((MainWindow)Application.Current.MainWindow).callWindow.startTime = DateTime.Now;
                ((MainWindow)Application.Current.MainWindow).callWindow.Show();

            }));
        }
        public static void RequestCallUI(string friendID, string IP)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                var contact = context.Contacts.Where(o => o.UserID == friendID).FirstOrDefault();
                var mainwindow = (MainWindow)Application.Current.MainWindow;

                context.CallingContact = contact;
                context.CallingContact.IP = IP;
                mainwindow.CallRequest.Visibility = Visibility.Visible;

            }));
        }
        public static void NewFileUI(string friendID, string filename)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                var contact = context.Contacts.Where(o => o.UserID == friendID).FirstOrDefault();

                contact.Messages.Add(new MessageModel
                {
                    Message = "file: " + filename,
                    Username = contact.Username,
                    Time = DateTime.Now,
                    ImageSource = "./UI/Resources/DefaultUserLogo.png",
                    Color = "#FFC2C2C2",
                    IsFocusable = false,
                    IsClickable = true,
                    Cursor = "Arrow",
                    FontWeight = "Normal",
                    FirstMessage = true
                });

            }));
        }
        public static void NewFriendReqUI(string nickname, string id)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);

                context.Requests.Add(new RequestModel()
                {
                    Username = nickname,
                    UserID = id,
                    ImageSource = "./UI/Resources/DefaultUserLogo.png"
                });
            }));
        }
        public static void NewFriendUI(string nickname, string id)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);

                context.Contacts.Add(new ContactModel()
                {
                    Username = nickname,
                    UserID = id,
                    ImageSource = "./UI/Resources/DefaultUserLogo.png",
                    Messages = new ObservableCollection<MessageModel>()
                });
            }));
        }
        public static void FriendReqGOOD()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var mainwindow = (MainWindow)Application.Current.MainWindow;
                mainwindow.AddContactInfoUserErrorLabel.Visibility = Visibility.Hidden;
                mainwindow.NewContactIDError.Visibility = Visibility.Hidden;

                mainwindow.AddContactInfoSuccessLabel.Visibility = Visibility.Visible;
                mainwindow.AddContactInfo.Visibility = Visibility.Visible;
            }));
        }

        public static void FriendReqFailedUI()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var mainwindow = (MainWindow)Application.Current.MainWindow;
                mainwindow.AddContactInfoSuccessLabel.Visibility = Visibility.Hidden;
                mainwindow.AddContactInfoUserErrorLabel.Visibility = Visibility.Hidden;
                mainwindow.NewContactIDError.Visibility = Visibility.Hidden;

                mainwindow.AddContactInfo.Visibility = Visibility.Visible;
                mainwindow.AddContactInfoUserErrorLabel.Visibility = Visibility.Visible;
            }));
        }
        public static void SendMessageUI(string message, string friendID)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                var contact = context.Contacts.Where(o => o.UserID == friendID).FirstOrDefault();

                contact.Messages.Add(new MessageModel
                {
                    Username = contact.Username,
                    Time = DateTime.Now,
                    ImageSource = "./UI/Resources/DefaultUserLogo.png",
                    Message = message,
                });
            }));
        }
        public static void RegGOOD()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var mainwindow = (MainWindow)Application.Current.MainWindow;
                mainwindow.registrationWindow.SignUpSuccesfulLabel.Visibility = Visibility.Visible;
                Thread.Sleep(250);
                mainwindow.registrationWindow.SignUpTextBlock.IsSelected = false;
                mainwindow.registrationWindow.SignInTextBlock.IsSelected = true;
                mainwindow.registrationWindow.SignUpContent.Visibility = Visibility.Hidden;
                mainwindow.registrationWindow.SignInContent.Visibility = Visibility.Visible;
            }));
        }

        public static void ClientBadAuth()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                var mainwindow = (MainWindow)Application.Current.MainWindow;
                context.client = null;
                mainwindow.registrationWindow.SignInErrorLabel.Visibility = Visibility.Visible;
            }));
        }
        public static void MainWindowUse()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainWindow)Application.Current.MainWindow);
                Application.Current.MainWindow.Show();
                context.registrationWindow.Close();
            }));
        }
        public static void UserUpData(string id, string nickname)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);

                context.User = new()
                {
                    UserID = id,
                    Username = nickname,
                    ImageSource = "./UI/Resources/DefaultUserLogo.png",
                };
            }));
        }
        public static void UserNotConnectedUI(string friendID)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                var contact = context.Contacts.Where(o => o.UserID == friendID).FirstOrDefault();

                contact.Messages.Add(new MessageModel
                {
                    Message = "User is not connected. Your message has not been sent",
                    Username = "System"
                }
                );
            }));

        }

        public static void SetFriendsUI(Dictionary<Guid, string> friends)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);

                foreach (var i in friends)
                {
                    context.Contacts.Add(new ContactModel()
                    {
                        Username = i.Value,
                        UserID = i.Key.ToString(),
                        ImageSource = "./UI/Resources/DefaultUserLogo.png",
                        Messages = new ObservableCollection<MessageModel>()
                    });
                }
            }));
        }

        public static void SetReqUI(Dictionary<Guid, string> req)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);

                foreach (var i in req)
                {
                    context.Requests.Add(new RequestModel()
                    {
                        Username = i.Value,
                        UserID = i.Key.ToString(),
                        ImageSource = "./UI/Resources/DefaultUserLogo.png"
                    });
                }
            }));
        }
    }
}
