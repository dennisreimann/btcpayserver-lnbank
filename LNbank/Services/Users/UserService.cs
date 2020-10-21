using System;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Client.Models;
using LNbank.Data;
using LNbank.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace LNbank.Services.Users
{
    public class UserService
    {
        private static readonly string AdminRoleName = "ServerAdmin";

        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly BTCPayService _btcpayService;

        public UserService(
            ILogger<UserService> logger,
            ApplicationDbContext dbContext,
            BTCPayService btcpayService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _btcpayService = btcpayService;
        }

        public async Task<User> CreateOrUpdateBtcPayUser(string userId, string apiKey)
        {
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

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.BTCPayUserId == userId);
            bool isAdmin = userData.Roles.Contains(AdminRoleName);

            if (user != null)
            {
                var entry = _dbContext.Entry(user);
                user.BTCPayApiKey = apiKey;
                user.BTCPayIsAdmin = isAdmin;
                entry.State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                user = new User
                {
                    BTCPayUserId = userId,
                    BTCPayApiKey = apiKey,
                    BTCPayIsAdmin = isAdmin
                };

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }

            return user;
        }
    }
}
