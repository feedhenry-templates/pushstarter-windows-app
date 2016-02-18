/*
 * JBoss, Home of Professional Open Source.
 * Copyright Red Hat, Inc., and individual contributors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using AeroGear.Push;
using FHSDK;
using FHSDKPortable;

namespace PushStarter
{
    /// <summary>
    ///     Main view that has 3 states
    /// </summary>
    public sealed partial class MainPage : INotifyPropertyChanged
    {
        public MainPage()
        {
            InitializeComponent();

            MessageList = new ObservableCollection<string>();
            RegisterState = new UiState();
            DataContext = this;

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        public ObservableCollection<string> MessageList { get; }
        public UiState RegisterState { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnRegistrationComplete()
        {
            RegisterState.RegistrationDone();
            PropertyChanged(this, new PropertyChangedEventArgs("RegisterState"));
        }

        private async void HandleNotification(object sender, PushReceivedEvent e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RegisterState.HasContent();
                PropertyChanged(this, new PropertyChangedEventArgs("RegisterState"));
                MessageList.Add(e.Args.Message);
            });
        }

        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.
        ///     This parameter is typically used to configure the page.
        /// </param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await FHClient.Init();
                FH.RegisterPush(HandleNotification);
                OnRegistrationComplete();
            }
            catch (Exception ex)
            {
                new MessageDialog("Error", ex.Message).ShowAsync();
            }

            InitializeComponent();
        }
    }

    public class UiState
    {
        private enum State
        {
            Register,
            Empty,
            List
        }

        private State _state = State.Register;

        public Visibility IsRegistering
        {
            get { return _state == State.Register ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility IsNotRegistering
        {
            get { return _state != State.Register ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility NoContent
        {
            get { return _state == State.Empty ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void RegistrationDone()
        {
            _state = State.Empty;
        }

        public void HasContent()
        {
            _state = State.List;
        }
    }
}