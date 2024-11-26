using Domain.Entities;
using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces;
public interface ICryptoOrganizerContext : IDbContext
{
    DbSet<CryptoOrganizerUser> Users { get; set; }
    DbSet<Token> Tokens { get; set; }
    //DbSet<Exchange> Exchanges { get; set; }
    //DbSet<LiquidityPool> LiquidityPools { get; set; }
    DbSet<Notification> Notifications { get; set; }
}