using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TeamoSharp.DataAccessLayer;
using TeamoSharp.Services;
using Xunit;
using Xunit.Abstractions;

namespace TeamoSharp.Tests
{
    public class MainServiceTester
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public MainServiceTester(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestTest()
        {
            _testOutputHelper.WriteLine("Running test!!");
            Assert.Equal(4, 4);
        }

        [Fact]
        public async void TestEverything()
        {
            //var connection = new SqliteConnection("DataSource=:memory:");
            var connection = new SqliteConnection("DataSource=myshareddb;mode=memory;cache=shared");
            connection.Open();

            try
            {
                var deps = new ServiceCollection()
                    .AddLogging((logging) =>
                    {
                        logging.AddProvider(new XUnitLoggerProvider(_testOutputHelper));
                       
                    })
                    .AddDbContext<TeamoContext>((options) =>
                    {
                        _testOutputHelper.WriteLine("Configuring options from test");
                        options.UseSqlite(connection);

                    })
                    .AddSingleton<IClientService, TestClientService>()
                    .AddSingleton<IMainService, MainService>();

                var serviceProvider = deps.BuildServiceProvider();

                var context = serviceProvider.GetService<TeamoContext>();
                context.Database.EnsureCreated();

                var mainService = serviceProvider.GetService<IMainService>();

                var entry1 = new Entities.TeamoEntry
                {
                    EndDate = DateTime.Now + TimeSpan.FromSeconds(20),
                    Game = "Test Game 1",
                    MaxPlayers = 7,
                    Message = new Entities.ClientMessage
                    {
                        ServerId = "1",
                        ChannelId = "1"
                    }
                };

                var entry2 = new Entities.TeamoEntry
                {
                    EndDate = DateTime.Now + TimeSpan.FromSeconds(20),
                    Game = "Test Game 2",
                    MaxPlayers = 3,
                    Message = new Entities.ClientMessage
                    {
                        ServerId = "1",
                        ChannelId = "1"
                    }
                };

                var entry1WithId = await mainService.CreateAsync(entry1);
                var entry2WithId = await mainService.CreateAsync(entry2);

                Assert.NotEqual(entry1WithId.Id, entry2WithId.Id);
                Assert.NotEqual(entry1WithId.Message.MessageId, entry2WithId.Message.MessageId);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
