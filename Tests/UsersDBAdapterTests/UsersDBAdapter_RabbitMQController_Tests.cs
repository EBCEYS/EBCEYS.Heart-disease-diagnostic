using DataBaseObjects.UsersDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UsersDBAdapter.DataBaseRepository;
using UsersDBAdapter.RabbitMQControllers;

namespace UsersDBAdapterTests
{
    [TestClass]
    public class UsersDBAdapter_RabbitMQController_Tests
    {
        private const string userId = "123";
        private const string name = "test";
        private const string pass = "test";
        private readonly string[] roles = new[] { "adm", "usr" };
        private readonly User etalonUser = new()
        {
            Id = userId,
            Name = name,
            Password = pass
        };

        private IUsersDBRepository? repo;
        private RabbitMQUsersDBAdapterController? controller;

        private readonly HashSet<User> usersStorage = new();

        [TestInitialize] 
        public void Initialize() 
        {
            Mock<UsersDBRepository> mock = new(new Logger<UsersDBRepository>(new NullLoggerFactory()), null);

            mock.Setup(x => x.AddUserAsync(etalonUser)).ReturnsAsync(() =>
            {
                usersStorage.Add(etalonUser);
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = etalonUser
                };
            });
            mock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(() =>
            {
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = usersStorage.ToList()
                };
            });
            mock.Setup(x => x.GetUserAsync(userId)).ReturnsAsync(() =>
            {
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = usersStorage.FirstOrDefault(x => x.Id == userId)
                };
            });
            mock.Setup(x => x.GetUserByNameAndPasswordAsync(name, pass)).ReturnsAsync(() =>
            {
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = usersStorage.FirstOrDefault(x => x.Name == name && x.Password == pass && x.IsActive)
                };
            });
            mock.Setup(x => x.GetUserByNameAsync(name)).ReturnsAsync(() =>
            {
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = usersStorage.FirstOrDefault(x => x.Name == name)
                };
            });
            mock.Setup(x => x.RemoveUserAsync(userId)).ReturnsAsync(() =>
            {
                User? usr = usersStorage.FirstOrDefault(x => x.Id == userId);
                if (usr != null) usersStorage.Remove(usr);
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = usr
                };
            });
            mock.Setup(x => x.UpdateUserRolesAsync(userId, roles)).ReturnsAsync(() =>
            {
                User? usr = usersStorage.FirstOrDefault(x => x.Id == userId);
                if (usr == null) return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = null
                };
                usersStorage.AsParallel().ForAll(u =>
                {
                    if (u.Id == userId)
                    {
                        u.Role = roles;
                        return;
                    }
                });
                return new()
                {
                    Result = UsersContracts.UsersResponseContractResult.Ok,
                    Object = usr
                };
            });

            repo = mock.Object;
            controller = new(new Logger<RabbitMQUsersDBAdapterController>(new NullLoggerFactory()), repo);
        }
        [TestMethod]
        public async Task AddUserAsync_Test()
        {
            usersStorage.Clear();
            UsersContracts.UsersContractResponse<User> addedUser = await controller!.AddUserAsync(etalonUser);

            Assert.IsNotNull(addedUser);
            Assert.IsTrue(usersStorage.Contains(addedUser.Object!));
        }
        [TestMethod]
        public async Task GetUserAsync_Test()
        {
            usersStorage.Clear();
            await controller!.AddUserAsync(etalonUser);

            UsersContracts.UsersContractResponse<User> user = await controller!.GetUserAsync(etalonUser.Id);

            Assert.IsNotNull(user);
            Assert.IsTrue(user.Object!.Id == etalonUser.Id);
        }
        [TestMethod]
        public async Task LoginUser_Test()
        {
            usersStorage.Clear();
            await controller!.AddUserAsync(etalonUser);

            UsersContracts.UsersContractResponse<User> user = await controller!.LoginUser(new()
            {
                UserName = etalonUser.Name!,
                Password = etalonUser.Password!
            });

            Assert.IsNotNull(user);
            Assert.IsTrue(user.Object!.Id == etalonUser.Id);
        }
        [TestMethod]
        public async Task RemoveUser_Test()
        {
            usersStorage.Clear();
            await controller!.AddUserAsync(etalonUser);

            UsersContracts.UsersContractResponse<User> user = await controller!.RemoveUser(etalonUser.Id);

            Assert.IsNotNull(user.Object);
            Assert.IsFalse(usersStorage.Any());
        }
        [TestMethod]
        public async Task GetAllUsers_Test()
        {
            usersStorage.Clear();
            await controller!.AddUserAsync(etalonUser);

            UsersContracts.UsersContractResponse<List<User>> users = await controller!.GetAllUsers();
            Assert.IsNotNull(users.Object);
            Assert.IsTrue(users.Object.Any());
        }
        [TestMethod]
        public async Task UpdateUserRoles_Test()
        {
            usersStorage.Clear();
            await controller!.AddUserAsync(etalonUser);

            UsersContracts.UsersContractResponse<User> users = await controller!.UpdateUserRoles(etalonUser.Id, string.Join(",", roles));

            Assert.IsNotNull(users.Object);
            Assert.IsTrue(users.Object.Role!.Any());
            Assert.IsTrue(roles.SequenceEqual(users.Object.Role!));
        }
        [TestMethod]
        public async Task GetUserByName_Test()
        {
            usersStorage.Clear();
            await controller!.AddUserAsync(etalonUser);

            UsersContracts.UsersContractResponse<User> users = await controller!.GetUserByName(etalonUser.Name);

            Assert.IsNotNull(users.Object);
            Assert.IsTrue(users.Object.Id == users.Object.Id);
        }
    }
}