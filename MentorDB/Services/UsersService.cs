using Domain;
using Domain.In;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UsersService
    {
        private readonly MentorDataContext _context;

        public UsersService(MentorDataContext context)
        {
            _context = context;
        }

        public async Task<bool?> AddUser(User user)
        {
            if (_context.User == null)
            {
                return null;
            }
            _context.User.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public async Task<bool?> ValidateUser(LogUser user)
        {
            if (_context.User == null)
            {
                return null;
            }

            if (user == null || !UserExists(user.Id))
            {
                return false;
            }
            var realUser = await _context.User.FindAsync(user.Id);
            if (realUser == null || realUser.Password != user.Password)
            {
                return false;
            }

            return true;
        }
        private async ValueTask<User?> UserWithAll(string userId)
        {
            if (_context.User == null)
            {
                return null;
            }
            User? user = await _context.User.FindAsync(userId);

            if (user == null)
            {
                return null;
            }

            user = await _context.User.Include(x => x.Missions).Include(x => x.Schedule).FirstOrDefaultAsync(u => u.Id == user.Id);
            return user;
        }

        public async Task<bool?> DeleteUser(string userId)
        {
            if (_context.User == null)
            {
                return null;
            }
            User? user = await UserWithAll(userId);
            if (user == null)
            {
                return null;
            }

            _context.User.Remove(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return true;
        }
        private bool UserExists(string id)
        {
            return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
