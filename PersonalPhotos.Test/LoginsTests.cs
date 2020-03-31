using PersonalPhotos.Controllers;
using Xunit;
using Moq;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalPhotos.Models;
using System.Threading.Tasks;
using Core.Models;

namespace PersonalPhotos.Test
{
    public class LoginsTests
    {
        private readonly LoginsController _controller;
        private readonly Mock<ILogins> _logins;
        private readonly Mock<IHttpContextAccessor> _accessor;

        public LoginsTests()
        {
            _logins = new Mock<ILogins>();
            

            var session = Mock.Of<ISession>();
            var httpContext = Mock.Of<HttpContext>(x => x.Session == session);

            _accessor = new Mock<IHttpContextAccessor>();
            _accessor.Setup(x => x.HttpContext).Returns(httpContext);

            _controller = new LoginsController(_logins.Object, _accessor.Object);
        }

        [Fact]
        public void Index_GivenNorReturnUrl_ReturnLoginView()
        {
            var result = (_controller.Index() as ViewResult);

            Assert.NotNull(result);
            Assert.Equal("Login", result.ViewName, ignoreCase: true);
        }

        [Fact]
        public async Task Login_GivenModelStateInvalid_ReturnLoginView()
        {
            _controller.ModelState.AddModelError("Test", "Test");

            var result = await _controller.Login(Mock.Of<LoginViewModel>()) as ViewResult;
            Assert.Equal("Login", result.ViewName, ignoreCase: true);
        }

        [Fact]
        public async Task Login_GivenCorrectPassword_RedirectToDisplayAction()
        {
            const string password = "123";
            var modelView = Mock.Of<LoginViewModel>(x=> x.Email == "a@b.com" && x.Password== password);
            var model = Mock.Of<User>(x=> x.Password == password);

            _logins.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(model);

            var result = await _controller.Login(modelView);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Login_GivenIncorrectPassword_ReturnLoginViewWithError()
        {
            var modelView = Mock.Of<LoginViewModel>(x => x.Email == "a@b.com" && x.Password == "123");
            var model = Mock.Of<User>(x => x.Password == "456");

            _logins.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(model);

            var result = await _controller.Login(modelView);

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", (result as ViewResult).ViewName, ignoreCase: true);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            Assert.True(_controller.ModelState.ContainsKey(""));
            Assert.Equal("Invalid password", _controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Login_GivenIncorrectEmail_ReturnLoginViewWithError()
        {
            var modelView = Mock.Of<LoginViewModel>(x => x.Email == "a@b.com" && x.Password == "123");
            User user = null;
            _logins.Setup(x => x.GetUser(modelView.Email)).ReturnsAsync(user);

            var result = await _controller.Login(modelView);

            Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", (result as ViewResult).ViewName, ignoreCase: true);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            Assert.True(_controller.ModelState.ContainsKey(""));
            Assert.Equal("User was not found", _controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Create_GivenModelStateInvalid_ReturnToView()
        {
            _controller.ModelState.AddModelError("Test", "Test");

            var result = await _controller.Create(Mock.Of<LoginViewModel>());
            Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            Assert.True(_controller.ModelState.ContainsKey("Test"));
            Assert.Equal("Test", _controller.ModelState["Test"].Errors[0].ErrorMessage);
            Assert.True(_controller.ModelState.ContainsKey(""));
            Assert.Equal("Invalid user details", _controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Create_GivenExistingUserEmail_ReturnToView()
        {
            const string password = "123";
            var modelView = Mock.Of<LoginViewModel>(x => x.Email == "a@b.com" && x.Password == password);
            var model = Mock.Of<User>(x => x.Password == password);

            _logins.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(model);

            var result = await _controller.Create(modelView);

            Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ErrorCount > 0);
            Assert.True(_controller.ModelState.ContainsKey(""));
            Assert.Equal("This email adress is already registered", _controller.ModelState[""].Errors[0].ErrorMessage);
        }
    }
}
