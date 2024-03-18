using Domain;
using Domain.In;
using Domain.Out;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services {
    public class AlgoService {
        private readonly MentorDataContext _context;
        private const int STARTING_INDEX = 0;
        private const int EMPTY = 0;
        private const string SITE_URL = "http://localhost:5000/algoComplete";
        private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";

        public AlgoService(MentorDataContext context) {
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

        public async Task<UiComplete?> CalculateSchedule(string userId, JointMissions missionListSetting) {
            // pre-process
            User? user = await UserWithAll(userId);

            if (user == null) {
                return null;
            }

            List<AlgoMission> missions = new List<AlgoMission>();
            List<int> missionIdList = missionListSetting.MissionsId;
            List<Mission> missionsComplete = await _context.Mission.Include(y => y.OptionalDays).Include(y => y.OptionalHours).Include(y => y.RankListHistory).Where(y => missionIdList.Contains(y.Id)).ToListAsync();
            missionsComplete.ForEach(x => {
                missions.Add(x.ToAlgo());
            });

            AlgoComplete complete = new AlgoComplete() { AlgoMission = missions, ScheduleSetting = missionListSetting.Setting };
            // call algo with complete
            var myContent = JsonConvert.SerializeObject(complete);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpClientHandler handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            HttpClient client = new HttpClient(handler);

            client.BaseAddress = new Uri(SITE_URL); // algo address
            HttpResponseMessage response = client.PostAsync("algoComplete", byteContent).Result;
            response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Result: " + responseString);
            List<string> responseStringList = JsonConvert.DeserializeObject<List<string>>(responseString);
            List<string> cantSchedAllHigh = new List<string> { "0", "0", "0" };
            if (cantSchedAllHigh.SequenceEqual(responseStringList)) {
                return null;
            }
            string stringOfUn = responseStringList[STARTING_INDEX]; // Get the first element
            responseStringList.RemoveAt(STARTING_INDEX); // Remove the first element from the list
            int numOfUnschedualed = int.Parse(stringOfUn);
            List<InAlgo> results = new List<InAlgo>();
            List<int> idList = new List<int>();
            while (responseStringList.Count != EMPTY) {
                int solutionId = int.Parse(responseStringList[STARTING_INDEX]);
                responseStringList.RemoveAt(STARTING_INDEX); //remove the solution id
                for (int i = STARTING_INDEX; i < missionIdList.Count - numOfUnschedualed; i++) {
                    string misionIdString = responseStringList[STARTING_INDEX]; // Get the first element
                    responseStringList.RemoveAt(STARTING_INDEX); // Remove the first element from the list
                    int misionIdint = int.Parse(misionIdString);
                    idList.Add(misionIdint);
                    string startDate = responseStringList[STARTING_INDEX];
                    responseStringList.RemoveAt(STARTING_INDEX); // Remove the first element from the list
                    string endDate = responseStringList[STARTING_INDEX];
                    responseStringList.RemoveAt(STARTING_INDEX); // Remove the first element from the list
                    DateTimeOffset dateTimeOffsetS = DateTimeOffset.Parse(startDate, CultureInfo.InvariantCulture);
                    string formattedDateStart = dateTimeOffsetS.ToString(DATE_FORMAT);
                    DateTimeOffset dateTimeOffsetE = DateTimeOffset.Parse(endDate, CultureInfo.InvariantCulture);
                    string formattedDateEnd = dateTimeOffsetE.ToString(DATE_FORMAT);
                    InAlgo newAlgoMission = new InAlgo {
                        Id = misionIdint,
                        StartDate = DateTime.ParseExact(formattedDateStart, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
                        EndDate = DateTime.ParseExact(formattedDateEnd, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                    };
                    results.Add(newAlgoMission);

                }
            }

            user.Missions.ForEach(a => a.unsettle());

            // save algo results
            results.ForEach(x => {
                user.Missions.Find(a => a.Id == x.Id).update(x);
            });



            _context.Entry(user).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                throw;
            }

            // return algo results
            return new UiComplete(missionListSetting.Setting, user.Missions.Select(x => x.ToUi()).ToList());
        }
    }
}
