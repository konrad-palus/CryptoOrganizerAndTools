using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class CryptoOrganizerContext(DbContextOptions<CryptoOrganizerContext> options) : IdentityDbContext<CryptoOrganizerUser>(options), ICryptoOrganizerContext
    {
        public DbSet<CryptoOrganizerUser> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        //public DbSet<Exchange> Exchanges { get; set; }
        //public DbSet<LiquidityPool> LiquidityPools { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}