using System;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Client.Models;
using LNbank.Data;
using LNbank.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LNbank.Services.Users
{
    public class UserService
    {
        private static readonly string AdminRoleName = "ServerAdmin";

        private readonly ILogger<UserService> _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly BTCPayService _btcpayService;

        public UserService(
            ILogger<UserService> logger,
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            BTCPayService btcpayService)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _btcpayService = btcpayService;
        }

        public async Task<User> CreateOrUpdateBtcPayUser(string userId, string apiKey)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            ApplicationUserData userData = null;
            try
            {
                userData = await _btcpayService.GetUserDataForApiKey(apiKey);
            }
            catch (Exception exception)
            {
                _logger.LogError($"GetUserDataForApiKey failed! {exception}");
            }

            if (userData == null || userData.Id != userId)
            {
                throw new Exception($"Invalid user or user {userId} not registered at endpoint.");
            }

            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.BTCPayUserId == userId);
            bool isAdmin = userData.Roles.Contains(AdminRoleName);

            if (user != null)
            {
                var entry = dbContext.Entry(user);
                user.BTCPayApiKey = apiKey;
                user.BTCPayIsAdmin = isAdmin;
                entry.State = EntityState.Modified;
                await dbContext.SaveChangesAsync();
            }
            else
            {
                user = new User
                {
                    BTCPayUserId = userId,
                    BTCPayApiKey = apiKey,
                    BTCPayIsAdmin = isAdmin
                };

                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
            }

            return user;
        }

        public async Task<User> FindUserById(string userId)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            return await dbContext.Users.SingleOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> FindUserByBtcPayApiKey(string apiKey)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            return await dbContext.Users.SingleOrDefaultAsync(u => u.BTCPayApiKey == apiKey);
        }
    }
}
