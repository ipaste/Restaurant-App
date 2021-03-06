﻿using System;
using System.Net;
using System.Windows.Input;
using JetBrains.Annotations;
using ReactiveUI;
using Restaurant.Abstractions.Facades;
using Restaurant.Abstractions.Managers;
using Restaurant.Abstractions.Services;
using Restaurant.Abstractions.ViewModels;
using Restaurant.Common.DataTransferObjects;

namespace Restaurant.Core.ViewModels
{
    [UsedImplicitly]
    public class SignInViewModel : ViewModelBase, ISignInViewModel
    {
        private string _email;
        private string _error;
        private string _password;

        public SignInViewModel(
            IAuthenticationManager authenticationManager,
            IAutoMapperFacade autoMapperFacade,
            INavigationService navigationService)
        {
            var canLogin = this.WhenAny(x => x.Email, x => x.Password,
                (e, p) => !string.IsNullOrEmpty(e.Value) && !string.IsNullOrEmpty(p.Value));

            Login = ReactiveCommand.CreateFromTask(async () =>
            {
                var loginDto = autoMapperFacade.Map<LoginDto>(this);
                var result = await authenticationManager.Login(loginDto);

                if (result.HttpStatusCode != HttpStatusCode.OK)
                {
                    Error = "Internal server error!";
                    return;
                }

                if (result.IsError)
                {
                    Error = "Invalid login or password!";
                    return;
                }

                await navigationService.NavigateToMainPage(typeof(IMainViewModel));

            }, canLogin);
        }

        /// <summary>
        ///     Gets and sets error message when login fails
        /// </summary>
        public string Error
        {
            get => _error;
            set => this.RaiseAndSetIfChanged(ref _error, value);
        }

        /// <summary>
        ///     Gets and sets login command
        ///     Command that logins to service
        /// </summary>
        public ICommand Login { get; }

        /// <summary>
        ///     Gets and sets user Email
        /// </summary>
        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        /// <summary>
        ///     Gets and sets non encrypted user passwords
        /// </summary>
        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public override string Title => "Login";
    }
}