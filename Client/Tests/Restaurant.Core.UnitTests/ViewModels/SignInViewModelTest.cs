﻿using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Restaurant.Abstractions.Facades;
using Restaurant.Abstractions.Managers;
using Restaurant.Abstractions.Services;
using Restaurant.Abstractions.ViewModels;
using Restaurant.Common.DataTransferObjects;
using Restaurant.Core.ViewModels;

namespace Restaurant.Core.UnitTests.ViewModels
{
    [TestFixture]
    public class SignInViewModelTest : BaseAutoMockedTest<SignInViewModel>
    {
        public override void Init()
        {
            base.Init();
            ViewModel = ClassUnderTest;
        }

        private SignInViewModel ViewModel;

        [Test]
        public void Given_empty_login_and_password_Login_should_not_be_executable()
        {
            ViewModel.Email = "";
            ViewModel.Password = "";

            Assert.That(ViewModel.Login.CanExecute(null), Is.False);
        }

        [Test]
        public void Given_invalid_data_Login_should_create_internal_server_error_message()
        {
            // Given
            ViewModel.Email = "invalid";
            ViewModel.Password = "invalid";
            var tokenResponse = new TokenResponse {IsError = true, HttpStatusCode = HttpStatusCode.BadRequest};

            GetMock<IAutoMapperFacade>().Setup(x => x.Map<LoginDto>(ViewModel))
                .Returns<LoginDto>(null);

            GetMock<IAuthenticationManager>().Setup(x => x.Login(null)).Returns(Task.FromResult(tokenResponse));

            // when
            ViewModel.Login.Execute(null);

            // then
            Assert.That(ViewModel.Error, Is.EqualTo("Internal server error!"));
            GetMock<INavigationService>().Verify(x => x.NavigateToMainPage(typeof(IMainViewModel)), Times.Never);
        }

        [Test]
        public void Given_invalid_login_password_Login_should_create_invalid_message()
        {
            // Given
            ViewModel.Email = "invalid";
            ViewModel.Password = "invalid";
            var tokenResponse = new TokenResponse {IsError = true, HttpStatusCode = HttpStatusCode.OK};

            var loginDto = new LoginDto {Login = ViewModel.Email, Password = ViewModel.Password};

            GetMock<IAutoMapperFacade>().Setup(x => x.Map<LoginDto>(ViewModel))
                .Returns(loginDto);

            GetMock<IAuthenticationManager>().Setup(x => x.Login(loginDto)).Returns(Task.FromResult(tokenResponse));

            // when
            ViewModel.Login.Execute(null);

            // then
            Assert.That(ViewModel.Error, Is.EqualTo("Invalid login or password!"));
            GetMock<INavigationService>().Verify(x => x.NavigateToMainPage(typeof(IMainViewModel)), Times.Never);
        }

        [Test]
        public void Given_login_and_password_Login_with_valid_data_should_be_ok()
        {
            // given
            ViewModel.Email = "12@123.com";
            ViewModel.Password = "test123";
            var tokenResponse = new TokenResponse {IsError = false, HttpStatusCode = HttpStatusCode.OK};

            var loginDto = new LoginDto {Login = ViewModel.Email, Password = ViewModel.Password};

            GetMock<IAutoMapperFacade>().Setup(x => x.Map<LoginDto>(ViewModel))
                .Returns(loginDto);

            GetMock<IAuthenticationManager>().Setup(x => x.Login(loginDto)).Returns(Task.FromResult(tokenResponse));

            // when
            ViewModel.Login.Execute(null);

            // then
            GetMock<INavigationService>().Verify(x => x.NavigateToMainPage(typeof(IMainViewModel)), Times.Once);
        }

        [Test]
        public void Title_should_be_Login()
        {
            Assert.That(ViewModel.Title, Is.EqualTo("Login"));
        }
    }
}