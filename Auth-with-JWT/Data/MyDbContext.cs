using Auth_with_JWT.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth_with_JWT.Data
{
	public class MyDbContext : DbContext
	{
		public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
		public DbSet<User> Users { get; set; }

	}
}


