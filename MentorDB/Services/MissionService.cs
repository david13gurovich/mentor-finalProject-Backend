using Domain;
using Domain.In;
using Domain.Out;
using Domain.Priv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services {
    public class MissionService {

        private readonly MentorDataContext _context;

        public MissionService(MentorDataContext context) {
            _context = context;
        }

        private async ValueTask<User?> UserWithAll(string userId) {
            if (_context.User == null) {
                return null;
            }
            User? user = await _context.User.FindAsync(userId);

            if (user == null) {
                return null;
            }

            user = await _context.User.Include(x => x.Missions).Include(x => x.Schedule).FirstOrDefaultAsync(u => u.Id == user.Id);
            return user;
        }

        public async Task<List<UiMission>> GetMissions(string userId) {

            User ? user = await UserWithAll(userId);

            if (user == null) {
                return null;
            }

            List<UiMission> missions = new List<UiMission>();
            user.Missions.ForEach(x => {
                missions.Add(
new UiMission() { Id = x.Id, Settled = x.Settled, Description = x.Description, EndDate = x.EndDate, StartDate = x.StartDate, Title = x.Title, Type = x.Type, Rank = x.Rank });
            });

            return missions;
        }

        public async Task<bool> UpdateMission(InMission mission, string userId) {

            User? user = await UserWithAll(userId);

            if (user == null) {
                return false;
            }

            Mission? user_mission = user.Missions.Find(m => m.Id == mission.Id);

            if (user_mission == null) {
                return false;
            }

            user_mission.Settled = mission.Settled;
            user_mission.Description = mission.Description;
            user_mission.EndDate = mission.EndDate;
            user_mission.StartDate = mission.StartDate;
            user_mission.Title = mission.Title;
            user_mission.Type = mission.Type;
            user_mission.OptionalDays = mission.OptionalDays;
            user_mission.OptionalHours = mission.OptionalHours;
            user_mission.DeadLine = mission.DeadLine;
            user_mission.Length = mission.Length;
            _context.Entry(user_mission).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw;
            }

            return true;
        }

        public async Task<int?> AddMission(Mission mission, string userId) {
            if (_context.User == null) {
                return null;
            }
            User? user = await UserWithAll(userId);
            if (user == null) {
                return null;
            }
            if (user.Missions == null) {
                user.Missions = new List<Mission>();
            }
            user.Missions.Add(mission);
            _context.Entry(user).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw;
            }

            return mission.Id;
        }

        public async Task<bool?> DeleteMission(int missionId, string userId) {
            if (_context.User == null) {
                return null;
            }
            User? user = await UserWithAll(userId);
            if (user == null) {
                return null;
            }

            int numRemoved = user.Missions.RemoveAll(x => {
                return x.Id == missionId;
            });

            _context.Entry(user).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw;
            }

            if (numRemoved == 0) {
                return false;
            }

            return true;
        }

        public async Task<List<UiMission>> SuggestPopularMissions(string userId, string type)
        {
            User? user = await UserWithAll(userId);

            if (user == null)
            {
                return null;
            }
            if (user.Missions == null) {
                user.Missions = new List<Mission>();
            }

            List<UiMission> suggestedMissions = new List<UiMission>();

            // Retrieve distinct missions with the same type that are most common among other users
            var mostCommonMissions = _context.Mission
                .Where(m => m.Type == type)
                .GroupBy(m => m.Title)
                .OrderByDescending(g => g.Count())
                .Select(g => g.First())
                .AsEnumerable() // Fetch missions from the database and switch to in-memory evaluation
                .Where(m => !user.Missions.Any(um => um.Title == m.Title)) // Exclude missions with the same title as the user's existing missions
                .Take(3)
                .ToList();
            Mission? lastM = _context.Mission.OrderBy(m=>m.Id).Last();
            if (lastM == null || mostCommonMissions.IsNullOrEmpty()) {
                return suggestedMissions;
            }

            foreach (var mission in mostCommonMissions) {
                int? id = await AddMission(new Mission() {
                    Title = mission.Title,
                    Description = mission.Description,
                    Type = mission.Type,
                    Length = mission.Length,
                    OptionalDays = mission.OptionalDays,
                    OptionalHours = mission.OptionalHours,
                    DeadLine = mission.DeadLine,
                    Priority = mission.Priority,
                    Settled = false,
                    StartDate = mission.StartDate,
                    EndDate = mission.EndDate,

                }, userId);
                if (!id.HasValue) {
                    continue;
                }
                suggestedMissions.Add(new UiMission() {
                    Id = (int)id,
                    Settled = mission.Settled,
                    Description = mission.Description,
                    EndDate = mission.EndDate,
                    StartDate = mission.StartDate,
                    Title = mission.Title,
                    Type = mission.Type,                  
                });
            }
            

            return suggestedMissions;
        }

       

        public async Task UpdateMissionRanks(List<Mission> missions, int rank)
        {
            foreach (var mission in missions)
            {
                if (mission.StartDate != null && mission.EndDate != null)
                {
                    MissionRank newRank = new MissionRank
                    {
                        Rank = rank,
                        StartTime = mission.StartDate,
                        EndTime = mission.EndDate
                    };

                    mission.RankListHistory.Add(newRank);
                }
            }

            _context.SaveChanges();
        }

        public async Task<List<Mission>> GetMissionsByIds(List<int> missionIds)
        {
            return await _context.Mission
                .Where(m => missionIds.Contains(m.Id))
                .ToListAsync();
        }


    }
}
