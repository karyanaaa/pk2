using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HackathonHub.Client.Models;

namespace HackathonHub.Client.Services
{
    public class ApiService
    {
        private static ApiService _instance;
        public static ApiService Instance
        {
            get
            {
                ApiService apiService = _instance ??= new ApiService();
                return apiService;
            }
        }

        private readonly HttpClient _httpClient;

        private string _baseUrl = "http://localhost:8080/api";

        public string Token { get; private set; }
        public User CurrentUser { get; private set; }

        public event Action OnAuthChanged;

        private ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public void SetToken(string token, User user)
        {
            Token = token;
            CurrentUser = user;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            OnAuthChanged?.Invoke();
        }

        public void Logout()
        {
            Token = null;
            CurrentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            OnAuthChanged?.Invoke();
        }

        private async Task<T> SendAsync<T>(HttpMethod method, string endpoint,
            object body = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");
                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body,
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(content))
                    return default;

                var trimmed = content.TrimStart();
                if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                {
                    throw new Exception($"Сервер вернул не JSON ({response.StatusCode}): {content.Substring(0, Math.Min(200, content.Length))}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                };

                return JsonSerializer.Deserialize<T>(content, options);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка соединения: {ex.Message}");
            }
        }

        // ============= AUTH =============
        public async Task<ApiResponse<User>> RegisterAsync(string fullName, string email,
            string password, string role)
        {
            return await SendAsync<ApiResponse<User>>(HttpMethod.Post, "/auth/register",
                new { fullName, email, password });
        }

        public async Task<ApiResponse<object>> LoginAsync(string email, string password)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post, "/auth/login",
                new { email, password });
        }

        public async Task<ApiResponse<object>> ForgotPasswordAsync(string email)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                "/auth/forgot-password", new { email });
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(string token,
            string newPassword)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                "/auth/reset-password", new { token, newPassword });
        }

        public async Task<ApiResponse<User>> GetCurrentUserAsync()
        {
            return await SendAsync<ApiResponse<User>>(HttpMethod.Get, "/users/me");
        }

        public async Task<ApiResponse<User>> UpdateProfileAsync(string fullName,
            string phone, string skills)
        {
            return await SendAsync<ApiResponse<User>>(HttpMethod.Put, "/users/me",
                new { fullName, phone, skills });
        }

        public async Task<ApiResponse<User>> UpdateProfileWithAvatarAsync(
            string fullName, string phone, string skills, string avatarUrl)
        {
            return await SendAsync<ApiResponse<User>>(HttpMethod.Put, "/users/me",
                new { fullName, phone, skills, avatarUrl });
        }

        public async Task<ApiResponse<object>> ChangePasswordAsync(string oldPassword,
            string newPassword)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Put,
                "/users/me/password", new { oldPassword, newPassword });
        }

        // ============= HACKATHONS =============
        public async Task<List<Hackathon>> GetHackathonsAsync()
        {
            return await SendAsync<List<Hackathon>>(HttpMethod.Get, "/hackathons");
        }

        public async Task<Hackathon> GetHackathonAsync(int id)
        {
            return await SendAsync<Hackathon>(HttpMethod.Get, $"/hackathons/{id}");
        }

        public async Task<ApiResponse<Hackathon>> CreateHackathonAsync(Hackathon h)
        {
            return await SendAsync<ApiResponse<Hackathon>>(HttpMethod.Post, "/hackathons", h);
        }

        public async Task<ApiResponse<Hackathon>> UpdateHackathonAsync(int id, Hackathon h)
        {
            return await SendAsync<ApiResponse<Hackathon>>(HttpMethod.Put,
                $"/hackathons/{id}", h);
        }

        public async Task<ApiResponse<object>> DeleteHackathonAsync(int hackathonId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Delete,
                $"/hackathons/{hackathonId}", null);
        }

        public async Task<List<TeamOverview>> GetMyTeamsOverviewAsync()
        {
            return await SendAsync<List<TeamOverview>>(HttpMethod.Get,
                "/hackathons/my/teams-overview");
        }

        // ============= REGISTRATIONS =============
        public async Task<ApiResponse<object>> RegisterForHackathonAsync(int hackathonId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/hackathons/{hackathonId}/register", null);
        }

        public async Task<object> GetMyRegistrationAsync(int hackathonId)
        {
            return await SendAsync<object>(HttpMethod.Get,
                $"/hackathons/{hackathonId}/my-registration");
        }

        public async Task<List<object>> GetHackathonRegistrationsAsync(int hackathonId)
        {
            return await SendAsync<List<object>>(HttpMethod.Get,
                $"/hackathons/{hackathonId}/registrations");
        }

        public async Task<ApiResponse<object>> ApproveRegistrationAsync(int regId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/hackathons/registrations/{regId}/approve", null);
        }

        public async Task<ApiResponse<object>> RejectRegistrationAsync(int regId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/hackathons/registrations/{regId}/reject", null);
        }

        // ============= JURY ASSIGNMENT =============
        public async Task<List<object>> GetHackathonJuryAsync(int hackathonId)
        {
            return await SendAsync<List<object>>(HttpMethod.Get,
                $"/hackathons/{hackathonId}/jury");
        }

        public async Task<ApiResponse<object>> AssignJuryAsync(int hackathonId, int userId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/hackathons/{hackathonId}/jury/{userId}", null);
        }

        public async Task<ApiResponse<object>> RemoveJuryAsync(int hackathonId, int userId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Delete,
                $"/hackathons/{hackathonId}/jury/{userId}", null);
        }

        // ============= TEAMS =============
        public async Task<List<Team>> GetTeamsByHackathonAsync(int hackathonId)
        {
            return await SendAsync<List<Team>>(HttpMethod.Get,
                $"/hackathons/{hackathonId}/teams");
        }

        public async Task<ApiResponse<Team>> CreateTeamAsync(Team team)
        {
            return await SendAsync<ApiResponse<Team>>(HttpMethod.Post, "/teams", team);
        }

        public async Task<ApiResponse<object>> JoinTeamAsync(int teamId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/teams/{teamId}/join");
        }

        public async Task<ApiResponse<object>> LeaveTeamAsync(int teamId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Delete,
                $"/teams/{teamId}/leave");
        }

        public async Task<ApiResponse<object>> InviteToTeamAsync(int teamId, int userId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/teams/{teamId}/invite", new { userId });
        }

        public async Task<List<Team>> GetMyTeamsAsync()
        {
            return await SendAsync<List<Team>>(HttpMethod.Get, "/my/teams");
        }

        // ============= PROJECTS =============
        public async Task<ApiResponse<Project>> UploadProjectAsync(Project project)
        {
            return await SendAsync<ApiResponse<Project>>(HttpMethod.Post, "/projects", project);
        }

        public async Task<List<Project>> GetProjectsForJuryAsync()
        {
            return await SendAsync<List<Project>>(HttpMethod.Get, "/projects/for-jury");
        }

        public async Task<List<Project>> GetMyProjectsAsync()
        {
            return await SendAsync<List<Project>>(HttpMethod.Get, "/projects/my");
        }

        // ============= CRITERIA =============
        public async Task<List<Criterion>> GetCriteriaAsync(int hackathonId)
        {
            return await SendAsync<List<Criterion>>(HttpMethod.Get,
                $"/criteria/{hackathonId}");
        }

        public async Task<ApiResponse<Criterion>> CreateCriterionAsync(Criterion c)
        {
            return await SendAsync<ApiResponse<Criterion>>(HttpMethod.Post, "/criteria", c);
        }

        public async Task<ApiResponse<object>> DeleteCriterionAsync(int criterionId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Delete,
                $"/criteria/{criterionId}", null);
        }

        // ============= SCORES =============
        public async Task<ApiResponse<Score>> SubmitScoreAsync(Score score)
        {
            return await SendAsync<ApiResponse<Score>>(HttpMethod.Post, "/scores", score);
        }

        public async Task<List<Score>> GetMyScoresAsync()
        {
            return await SendAsync<List<Score>>(HttpMethod.Get, "/scores/my");
        }

        // ============= RATINGS =============
        public async Task<List<Rating>> GetRatingsAsync(int hackathonId)
        {
            return await SendAsync<List<Rating>>(HttpMethod.Get,
                $"/ratings/{hackathonId}");
        }

        // ============= ADMIN =============
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await SendAsync<List<User>>(HttpMethod.Get, "/admin/users");
        }

        public async Task<ApiResponse<User>> ChangeUserRoleAsync(int userId, string role)
        {
            return await SendAsync<ApiResponse<User>>(HttpMethod.Put,
                $"/admin/users/{userId}/role", new { role });
        }

        public async Task<ApiResponse<object>> BlockUserAsync(int userId, bool block)
        {
            var endpoint = block
                ? $"/admin/users/{userId}/block"
                : $"/admin/users/{userId}/unblock";
            return await SendAsync<ApiResponse<object>>(HttpMethod.Put, endpoint, null);
        }

        public async Task<ApiResponse<User>> CreateUserAsync(string fullName, string email,
            string password, string role)
        {
            return await SendAsync<ApiResponse<User>>(HttpMethod.Post, "/admin/users",
                new { fullName, email, password, role });
        }

        // ============= STATS =============
        public async Task<object> GetHackathonStatsAsync(int hackathonId)
        {
            return await SendAsync<object>(HttpMethod.Get, $"/stats/hackathon/{hackathonId}");
        }

        // ============= UPLOADS =============
        public async Task<ApiResponse<object>> UploadAvatarAsync(string filePath)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(Token))
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", Token);

                    using (var form = new MultipartFormDataContent())
                    {
                        var bytes = System.IO.File.ReadAllBytes(filePath);
                        var fileContent = new ByteArrayContent(bytes);
                        fileContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                        form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

                        var response = await client.PostAsync($"{_baseUrl}/uploads/avatar", form);
                        var content = await response.Content.ReadAsStringAsync();

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        return JsonSerializer.Deserialize<ApiResponse<object>>(content, options);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object> { Success = false, Message = ex.Message };
            }
        }

        // ============= INVITATIONS =============
        public async Task<ApiResponse<object>> SendInvitationAsync(int teamId, int userId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/invitations/team/{teamId}/user/{userId}", null);
        }

        public async Task<List<TeamInvitation>> GetMyInvitationsAsync()
        {
            return await SendAsync<List<TeamInvitation>>(HttpMethod.Get, "/invitations/my");
        }

        public async Task<ApiResponse<object>> AcceptInvitationAsync(int invId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/invitations/{invId}/accept", null);
        }

        public async Task<ApiResponse<object>> RejectInvitationAsync(int invId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/invitations/{invId}/reject", null);
        }

        public async Task<ApiResponse<object>> RequestJoinTeamAsync(int teamId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/invitations/request/team/{teamId}", null);
        }

        public async Task<List<PendingRequest>> GetPendingRequestsAsync(int teamId)
        {
            return await SendAsync<List<PendingRequest>>(HttpMethod.Get,
                $"/invitations/team/{teamId}/pending");
        }

        public async Task<ApiResponse<object>> AcceptRequestAsync(int invId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/team-invitations/{invId}/accept", null);
        }

        public async Task<ApiResponse<object>> RejectRequestAsync(int invId)
        {
            return await SendAsync<ApiResponse<object>>(HttpMethod.Post,
                $"/team-invitations/{invId}/reject", null);
        }

        // ============= CHAT =============
        public async Task<List<HackathonChat>> GetMyChatsAsync()
        {
            return await SendAsync<List<HackathonChat>>(HttpMethod.Get, "/chat/my-hackathons");
        }

        public async Task<List<ChatMessage>> GetChatMessagesAsync(int hackathonId)
        {
            return await SendAsync<List<ChatMessage>>(HttpMethod.Get,
                $"/chat/{hackathonId}");
        }

        public async Task<ApiResponse<ChatMessage>> SendChatMessageAsync(int hackathonId, string text)
        {
            return await SendAsync<ApiResponse<ChatMessage>>(HttpMethod.Post,
                $"/chat/{hackathonId}", new { text });
        }
    }
}