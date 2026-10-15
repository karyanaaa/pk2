using HackathonHub.Client.Models;
using HackathonHub.Client.Services;
using QuestPDF.Fluent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HackathonHub.Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ApiService _api = ApiService.Instance;
        private readonly DialogService _dialog = new DialogService();

        private User _currentUser;
        private ObservableCollection<Hackathon> _hackathons = new ObservableCollection<Hackathon>();
        private ObservableCollection<Hackathon> _allHackathons = new ObservableCollection<Hackathon>();
        private ObservableCollection<Team> _myTeams = new ObservableCollection<Team>();
        private ObservableCollection<Project> _projectsForJury = new ObservableCollection<Project>();
        private ObservableCollection<Project> _myProjects = new ObservableCollection<Project>();
        private ObservableCollection<Project> _allProjectsForOrganizer = new ObservableCollection<Project>();
        private ObservableCollection<Score> _myScores = new ObservableCollection<Score>();
        private ObservableCollection<Rating> _ratings = new ObservableCollection<Rating>();
        private ObservableCollection<User> _allUsers = new ObservableCollection<User>();
        private ObservableCollection<TeamOverview> _teamsOverview = new ObservableCollection<TeamOverview>();
        private ObservableCollection<TeamInvitation> _myInvitations = new ObservableCollection<TeamInvitation>();
        private ObservableCollection<PendingRequest> _pendingRequests = new ObservableCollection<PendingRequest>();
        private ObservableCollection<HackathonChat> _myChats = new ObservableCollection<HackathonChat>();

        private Project _selectedProject;
        private User _selectedUser;
        private string _currentPage = "Hackathons";
        private string _searchText;
        private bool _isLoading;
        private bool _onlyMine;

        private string _filterStatus = "all";
        private string _filterSort = "date_desc";
        private bool _onlyMyParticipating;
        private System.Collections.Generic.List<int> _myParticipatingHackathonIds = new();

        private string _organizerSearch;

        private string _newUserName;
        private string _newUserEmail;
        private string _newUserPassword;
        private string _newUserRole = "participant";

        private string _editFullName;
        private string _editPhone;
        private string _editSkills;
        private string _oldPassword;
        private string _newPassword;
        private string _newPasswordConfirm;

        private string _statsTeams = "0";
        private string _statsProjects = "0";
        private string _statsMembers = "0";
        private string _statsNotSubmitted = "0";

        public User CurrentUser
        {
            get => _currentUser;
            set { SetProperty(ref _currentUser, value); NotifyRoleProps(); }
        }

        public ObservableCollection<Hackathon> Hackathons
        {
            get => _hackathons;
            set { SetProperty(ref _hackathons, value); OnPropertyChanged(nameof(FilteredMyHackathons)); }
        }

        public ObservableCollection<Team> MyTeams { get => _myTeams; set => SetProperty(ref _myTeams, value); }
        public ObservableCollection<Project> ProjectsForJury { get => _projectsForJury; set => SetProperty(ref _projectsForJury, value); }
        public ObservableCollection<Project> MyProjects { get => _myProjects; set => SetProperty(ref _myProjects, value); }
        public ObservableCollection<Project> AllProjectsForOrganizer { get => _allProjectsForOrganizer; set => SetProperty(ref _allProjectsForOrganizer, value); }
        public ObservableCollection<Score> MyScores { get => _myScores; set => SetProperty(ref _myScores, value); }
        public ObservableCollection<Rating> Ratings { get => _ratings; set => SetProperty(ref _ratings, value); }
        public ObservableCollection<User> AllUsers { get => _allUsers; set => SetProperty(ref _allUsers, value); }
        public ObservableCollection<TeamOverview> TeamsOverview { get => _teamsOverview; set => SetProperty(ref _teamsOverview, value); }
        public ObservableCollection<TeamInvitation> MyInvitations { get => _myInvitations; set => SetProperty(ref _myInvitations, value); }
        public ObservableCollection<PendingRequest> PendingRequests { get => _pendingRequests; set => SetProperty(ref _pendingRequests, value); }
        public ObservableCollection<HackathonChat> MyChats { get => _myChats; set => SetProperty(ref _myChats, value); }

        public string OrganizerSearch
        {
            get => _organizerSearch;
            set { SetProperty(ref _organizerSearch, value); OnPropertyChanged(nameof(FilteredMyHackathons)); }
        }

        public ObservableCollection<Hackathon> FilteredMyHackathons
        {
            get
            {
                var src = Hackathons.AsEnumerable();

                if (CurrentUser != null)
                    src = src.Where(h => h.OrganizerId == CurrentUser.Id);

                if (!string.IsNullOrWhiteSpace(OrganizerSearch))
                    src = src.Where(h => h.Title?.ToLower().Contains(OrganizerSearch.ToLower()) == true);

                return new ObservableCollection<Hackathon>(src);
            }
        }

        public Project SelectedProject
        {
            get => _selectedProject;
            set { SetProperty(ref _selectedProject, value); OnPropertyChanged(nameof(HasSelectedProject)); }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set { SetProperty(ref _selectedUser, value); OnPropertyChanged(nameof(HasSelectedUser)); }
        }

        public bool OnlyMine
        {
            get => _onlyMine;
            set { SetProperty(ref _onlyMine, value); ApplyFilter(); }
        }

        public string CurrentPage
        {
            get => _currentPage;
            set
            {
                SetProperty(ref _currentPage, value);
                NotifyPageProps();
                _ = OnPageChangedAsync();
            }
        }

        public string CurrentPageTitle => CurrentPage switch
        {
            "Hackathons" => "Хакатоны",
            "MySpace" => "Моё пространство",
            "MyHackathons" => "Мои хакатоны",
            "Jury" => "Оценка проектов",
            "Ratings" => "Рейтинг",
            "Admin" => "Пользователи",
            "Profile" => "Профиль",
            _ => "HackathonHub"
        };

        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); ApplyFilter(); }
        }

        public string FilterStatus
        {
            get => _filterStatus;
            set { SetProperty(ref _filterStatus, value); ApplyFilter(); }
        }

        public string FilterSort
        {
            get => _filterSort;
            set { SetProperty(ref _filterSort, value); ApplyFilter(); }
        }

        public bool OnlyMyParticipating
        {
            get => _onlyMyParticipating;
            set { SetProperty(ref _onlyMyParticipating, value); ApplyFilter(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string NewUserName { get => _newUserName; set => SetProperty(ref _newUserName, value); }
        public string NewUserEmail { get => _newUserEmail; set => SetProperty(ref _newUserEmail, value); }
        public string NewUserPassword { get => _newUserPassword; set => SetProperty(ref _newUserPassword, value); }
        public string NewUserRole { get => _newUserRole; set => SetProperty(ref _newUserRole, value); }

        public string EditFullName { get => _editFullName; set => SetProperty(ref _editFullName, value); }
        public string EditPhone { get => _editPhone; set => SetProperty(ref _editPhone, value); }
        public string EditSkills { get => _editSkills; set => SetProperty(ref _editSkills, value); }
        public string OldPassword { get => _oldPassword; set => SetProperty(ref _oldPassword, value); }
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
        public string NewPasswordConfirm { get => _newPasswordConfirm; set => SetProperty(ref _newPasswordConfirm, value); }

        public string StatsTeams { get => _statsTeams; set => SetProperty(ref _statsTeams, value); }
        public string StatsProjects { get => _statsProjects; set => SetProperty(ref _statsProjects, value); }
        public string StatsMembers { get => _statsMembers; set => SetProperty(ref _statsMembers, value); }
        public string StatsNotSubmitted { get => _statsNotSubmitted; set => SetProperty(ref _statsNotSubmitted, value); }

        public Visibility IsHackathonsPage => CurrentPage == "Hackathons" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsMySpacePage => CurrentPage == "MySpace" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsMyHackathonsPage => CurrentPage == "MyHackathons" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsJuryPage => CurrentPage == "Jury" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsRatingsPage => CurrentPage == "Ratings" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsProfilePage => CurrentPage == "Profile" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsAdminPage => CurrentPage == "Admin" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility MenuHackathons => CurrentUser?.Role == "participant" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuMySpace => CurrentUser?.Role == "participant" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuMyHackathons => CurrentUser?.Role == "organizer" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuJury => CurrentUser?.Role == "jury" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuRatings => CurrentUser?.Role == "jury" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility MenuAdmin => CurrentUser?.Role == "admin" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsOrganizerOrAdmin =>
            (CurrentUser?.Role == "organizer" || CurrentUser?.Role == "admin") ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsJuryOrAdmin =>
            (CurrentUser?.Role == "jury" || CurrentUser?.Role == "admin") ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsAdmin =>
            CurrentUser?.Role == "admin" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsParticipant =>
            CurrentUser?.Role == "participant" ? Visibility.Visible : Visibility.Collapsed;

        public Visibility HasSelectedProject =>
            SelectedProject != null ? Visibility.Visible : Visibility.Collapsed;

        public Visibility HasSelectedUser =>
            SelectedUser != null ? Visibility.Visible : Visibility.Collapsed;

        public bool HasAvatar => !string.IsNullOrEmpty(CurrentUser?.AvatarUrl);
        public Visibility AvatarVisibility => HasAvatar ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InitialVisibility => HasAvatar ? Visibility.Collapsed : Visibility.Visible;
        public string AvatarSource => CurrentUser?.AvatarUrl;

        public string CurrentUserInitial =>
            CurrentUser?.FullName?.Length > 0
                ? CurrentUser.FullName[0].ToString().ToUpper()
                : "?";

        public string CurrentRoleText => CurrentUser?.Role switch
        {
            "admin" => "Администратор",
            "organizer" => "Организатор",
            "participant" => "Участник",
            "jury" => "Жюри",
            _ => ""
        };

        public Brush CurrentRoleColor => CurrentUser?.Role switch
        {
            "admin" => new SolidColorBrush(Color.FromRgb(0x4a, 0x90, 0xa4)),
            "organizer" => new SolidColorBrush(Color.FromRgb(0x6b, 0xb6, 0xc9)),
            "participant" => new SolidColorBrush(Color.FromRgb(0xa8, 0xc8, 0xe8)),
            "jury" => new SolidColorBrush(Color.FromRgb(0xe8, 0xa8, 0xc4)),
            _ => new SolidColorBrush(Color.FromRgb(0x5f, 0x7f, 0x8f))
        };

        public string GreetingText
        {
            get
            {
                var h = System.DateTime.Now.Hour;
                if (h < 6) return "🌙 Доброй ночи";
                if (h < 12) return "☀️ Доброе утро";
                if (h < 18) return "🌤️ Добрый день";
                return "🌆 Добрый вечер";
            }
        }

        public string TotalUsers => AllUsers.Count.ToString();
        public string JuryCount => AllUsers.Count(u => u.Role == "jury").ToString();
        public string OrganizerCount => AllUsers.Count(u => u.Role == "organizer").ToString();

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateHackathonCommand { get; }
        public ICommand ViewHackathonCommand { get; }
        public ICommand OpenScoreWindowCommand { get; }
        public ICommand SetRoleParticipantCommand { get; }
        public ICommand SetRoleOrganizerCommand { get; }
        public ICommand SetRoleJuryCommand { get; }
        public ICommand SetRoleAdminCommand { get; }
        public ICommand BlockUserCommand { get; }
        public ICommand UnblockUserCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }
        public ICommand UploadAvatarCommand { get; }
        public ICommand ExportRatingsCsvCommand { get; }
        public ICommand ExportRatingsExcelCommand { get; }
        public ICommand ExportRatingsPdfCommand { get; }
        public ICommand ExportTeamsCsvCommand { get; }
        public ICommand ExportTeamsExcelCommand { get; }
        public ICommand ViewTeamDetailsCommand { get; }
        public ICommand DeleteHackathonCommand { get; }
        public ICommand ExportProjectsCsvCommand { get; }
        public ICommand ViewProjectDetailsCommand { get; }
        public ICommand OpenChatCommand { get; }
        public ICommand OpenTeamChatCommand { get; }
        public ICommand AcceptInvitationCommand { get; }
        public ICommand RejectInvitationCommand { get; }
        public ICommand ShowRulesCommand { get; }
        public ICommand OpenChatFromListCommand { get; }

        public MainViewModel()
        {
            CurrentUser = _api.CurrentUser;

            NavigateCommand = new RelayCommand(p => CurrentPage = p?.ToString());
            LogoutCommand = new RelayCommand(_ => Logout());
            RefreshCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
            CreateHackathonCommand = new RelayCommand(_ => OpenCreateHackathon());
            ViewHackathonCommand = new RelayCommand(p => OpenHackathonDetails(p as Hackathon));
            OpenScoreWindowCommand = new RelayCommand(_ => OpenScoreWindow());
            SetRoleParticipantCommand = new AsyncRelayCommand(async _ => await SetRoleAsync("participant"));
            SetRoleOrganizerCommand = new AsyncRelayCommand(async _ => await SetRoleAsync("organizer"));
            SetRoleJuryCommand = new AsyncRelayCommand(async _ => await SetRoleAsync("jury"));
            SetRoleAdminCommand = new AsyncRelayCommand(async _ => await SetRoleAsync("admin"));
            BlockUserCommand = new AsyncRelayCommand(async _ => await BlockUserAsync(true));
            UnblockUserCommand = new AsyncRelayCommand(async _ => await BlockUserAsync(false));
            CreateUserCommand = new AsyncRelayCommand(async _ => await CreateUserAsync());

            SaveProfileCommand = new AsyncRelayCommand(async _ => await SaveProfileAsync());
            ChangePasswordCommand = new AsyncRelayCommand(async _ => await ChangePasswordAsync());
            UploadAvatarCommand = new AsyncRelayCommand(async _ => await UploadAvatarAsync());

            ExportRatingsCsvCommand = new RelayCommand(_ => ExportRatingsCsv());
            ExportRatingsExcelCommand = new RelayCommand(_ => ExportRatingsExcel());
            ExportRatingsPdfCommand = new RelayCommand(_ => ExportRatingsPdf());
            ExportTeamsCsvCommand = new RelayCommand(_ => ExportTeamsCsv());
            ExportTeamsExcelCommand = new RelayCommand(_ => ExportTeamsExcel());
            ViewTeamDetailsCommand = new RelayCommand(p => ViewTeamDetails(p as TeamOverview));
            DeleteHackathonCommand = new AsyncRelayCommand(p => DeleteHackathonAsync(p as Hackathon));
            ExportProjectsCsvCommand = new RelayCommand(_ => ExportProjectsCsv());
            ViewProjectDetailsCommand = new RelayCommand(p => OpenProjectDetails(p as Project));

            OpenChatCommand = new RelayCommand(p => OpenChat(p as Hackathon));
            OpenTeamChatCommand = new RelayCommand(_ => OpenTeamChat());
            AcceptInvitationCommand = new AsyncRelayCommand(p => AcceptInvitationAsync(p as TeamInvitation));
            RejectInvitationCommand = new AsyncRelayCommand(p => RejectInvitationAsync(p as TeamInvitation));
            ShowRulesCommand = new RelayCommand(_ => ShowRules());
            OpenChatFromListCommand = new RelayCommand(p => OpenChatFromList(p as HackathonChat));

            EditFullName = CurrentUser?.FullName;
            EditPhone = CurrentUser?.Phone;
            EditSkills = CurrentUser?.Skills;

            _ = LoadDataAsync();

            if (CurrentUser?.Role == "participant") CurrentPage = "Hackathons";
            else if (CurrentUser?.Role == "organizer") CurrentPage = "MyHackathons";
            else if (CurrentUser?.Role == "jury") CurrentPage = "Jury";
            else if (CurrentUser?.Role == "admin") CurrentPage = "Admin";
        }

        private void NotifyRoleProps()
        {
            OnPropertyChanged(nameof(CurrentUserInitial));
            OnPropertyChanged(nameof(CurrentRoleText));
            OnPropertyChanged(nameof(CurrentRoleColor));
            OnPropertyChanged(nameof(HasAvatar));
            OnPropertyChanged(nameof(AvatarVisibility));
            OnPropertyChanged(nameof(InitialVisibility));
            OnPropertyChanged(nameof(AvatarSource));
            OnPropertyChanged(nameof(MenuHackathons));
            OnPropertyChanged(nameof(MenuMySpace));
            OnPropertyChanged(nameof(MenuMyHackathons));
            OnPropertyChanged(nameof(MenuJury));
            OnPropertyChanged(nameof(MenuRatings));
            OnPropertyChanged(nameof(MenuAdmin));
            OnPropertyChanged(nameof(IsOrganizerOrAdmin));
            OnPropertyChanged(nameof(IsJuryOrAdmin));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsParticipant));
            OnPropertyChanged(nameof(FilteredMyHackathons));
        }

        private void NotifyPageProps()
        {
            OnPropertyChanged(nameof(IsHackathonsPage));
            OnPropertyChanged(nameof(IsMySpacePage));
            OnPropertyChanged(nameof(IsMyHackathonsPage));
            OnPropertyChanged(nameof(IsJuryPage));
            OnPropertyChanged(nameof(IsRatingsPage));
            OnPropertyChanged(nameof(IsProfilePage));
            OnPropertyChanged(nameof(IsAdminPage));
            OnPropertyChanged(nameof(CurrentPageTitle));
        }

        private async Task OnPageChangedAsync()
        {
            switch (CurrentPage)
            {
                case "Admin":
                    if (CurrentUser?.Role == "admin") await LoadUsersAsync();
                    break;
                case "Ratings":
                    await LoadRatingsAsync();
                    break;
                case "MySpace":
                    await LoadMySpaceAsync();
                    break;
                case "MyHackathons":
                    await LoadMyHackathonsAsync();
                    break;
                case "Jury":
                    await LoadAllProjectsForJuryAsync();
                    break;
                case "Profile":
                    EditFullName = CurrentUser?.FullName;
                    EditPhone = CurrentUser?.Phone;
                    EditSkills = CurrentUser?.Skills;
                    break;
            }
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var list = await _api.GetHackathonsAsync();
                _allHackathons = new ObservableCollection<Hackathon>(
                    list ?? new System.Collections.Generic.List<Hackathon>());
                ApplyFilter();

                if (CurrentUser?.Role == "jury" ||
                    CurrentUser?.Role == "organizer" ||
                    CurrentUser?.Role == "admin")
                {
                    var projects = await _api.GetProjectsForJuryAsync();
                    AllProjectsForOrganizer = new ObservableCollection<Project>(
                        projects ?? new System.Collections.Generic.List<Project>());
                    ProjectsForJury = new ObservableCollection<Project>(AllProjectsForOrganizer);
                }

                OnPropertyChanged(nameof(FilteredMyHackathons));
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError($"Ошибка загрузки: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        private void ApplyFilter()
        {
            var src = _allHackathons.AsEnumerable();

            if (OnlyMine && CurrentUser != null)
                src = src.Where(h => h.OrganizerId == CurrentUser.Id);

            if (!string.IsNullOrWhiteSpace(SearchText))
                src = src.Where(h => h.Title?.ToLower().Contains(SearchText.ToLower()) == true);

            if (FilterStatus != "all")
                src = src.Where(h => h.Status == FilterStatus);

            if (OnlyMyParticipating)
                src = src.Where(h => _myParticipatingHackathonIds.Contains(h.Id));

            src = FilterSort switch
            {
                "date_asc" => src.OrderBy(h => h.StartDate),
                "date_desc" => src.OrderByDescending(h => h.StartDate),
                "title" => src.OrderBy(h => h.Title),
                "deadline" => src.OrderBy(h => h.RegistrationDeadline),
                _ => src.OrderByDescending(h => h.StartDate)
            };

            Hackathons = new ObservableCollection<Hackathon>(src);
            OnPropertyChanged(nameof(FilteredMyHackathons));
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var list = await _api.GetAllUsersAsync();
                AllUsers = new ObservableCollection<User>(list ?? new System.Collections.Generic.List<User>());
                OnPropertyChanged(nameof(TotalUsers));
                OnPropertyChanged(nameof(JuryCount));
                OnPropertyChanged(nameof(OrganizerCount));
            }
            catch (System.Exception ex) { _dialog.ShowError($"Ошибка: {ex.Message}"); }
        }

        private async Task LoadRatingsAsync()
        {
            try
            {
                if (_allHackathons.Count > 0)
                {
                    var r = await _api.GetRatingsAsync(_allHackathons[0].Id);
                    Ratings = new ObservableCollection<Rating>(r ?? new System.Collections.Generic.List<Rating>());
                }
            }
            catch { }
        }

        private async Task LoadMySpaceAsync()
        {
            await LoadMyTeamsAsync();
            await LoadPendingRequestsAsync();
            await LoadMyProjectsAsync();
            await LoadMyScoresAsync();
            await LoadMyInvitationsAsync();
            await LoadMyChatsAsync();

            _myParticipatingHackathonIds = MyTeams.Select(t => t.HackathonId).Distinct().ToList();
            ApplyFilter();
        }

        private async Task LoadMyChatsAsync()
        {
            try
            {
                var list = await _api.GetMyChatsAsync();
                MyChats = new ObservableCollection<HackathonChat>(
                    list ?? new System.Collections.Generic.List<HackathonChat>());
            }
            catch { }
        }

        private async Task LoadPendingRequestsAsync()
        {
            try
            {
                var pending = new ObservableCollection<PendingRequest>();

                foreach (var team in MyTeams)
                {
                    if (team.CaptainId != CurrentUser?.Id) continue;

                    var list = await _api.GetPendingRequestsAsync(team.Id);
                    if (list == null) continue;

                    foreach (var r in list)
                        pending.Add(r);
                }

                PendingRequests = pending;
            }
            catch (System.Exception ex)
            {
                _dialog.ShowError($"Ошибка загрузки заявок: {ex.Message}");
            }
        }

        private async Task LoadMyInvitationsAsync()
        {
            try
            {
                var list = await _api.GetMyInvitationsAsync();
                MyInvitations = new ObservableCollection<TeamInvitation>(
                    list ?? new System.Collections.Generic.List<TeamInvitation>());
            }
            catch { }
        }

        private async Task LoadMyHackathonsAsync()
        {
            await LoadTeamsOverviewAsync();
            await LoadAllProjectsForOrganizerAsync();

            try
            {
                int totalTeams = TeamsOverview.Count;
                int totalMembers = TeamsOverview.Sum(t => t.MembersCount);
                int totalProjects = AllProjectsForOrganizer.Count;
                int notSubmitted = totalTeams - totalProjects;
                if (notSubmitted < 0) notSubmitted = 0;

                StatsTeams = totalTeams.ToString();
                StatsMembers = totalMembers.ToString();
                StatsProjects = totalProjects.ToString();
                StatsNotSubmitted = notSubmitted.ToString();
            }
            catch { }

            OnPropertyChanged(nameof(FilteredMyHackathons));
        }

        private async Task LoadAllProjectsForOrganizerAsync()
        {
            try
            {
                var list = await _api.GetProjectsForJuryAsync();
                AllProjectsForOrganizer = new ObservableCollection<Project>(
                    list ?? new System.Collections.Generic.List<Project>());
            }
            catch { }
        }

        private async Task LoadMyTeamsAsync()
        {
            try
            {
                var list = await _api.GetMyTeamsAsync();
                MyTeams = new ObservableCollection<Team>(list ?? new System.Collections.Generic.List<Team>());
            }
            catch (System.Exception ex) { _dialog.ShowError($"Ошибка: {ex.Message}"); }
        }

        private async Task LoadMyProjectsAsync()
        {
            try
            {
                var list = await _api.GetMyProjectsAsync();
                MyProjects = new ObservableCollection<Project>(list ?? new System.Collections.Generic.List<Project>());
            }
            catch (System.Exception ex) { _dialog.ShowError($"Ошибка: {ex.Message}"); }
        }

        private async Task LoadMyScoresAsync()
        {
            try
            {
                var list = await _api.GetMyScoresAsync();
                MyScores = new ObservableCollection<Score>(list ?? new System.Collections.Generic.List<Score>());
            }
            catch (System.Exception ex) { _dialog.ShowError($"Ошибка: {ex.Message}"); }
        }

        private async Task LoadTeamsOverviewAsync()
        {
            try
            {
                var list = await _api.GetMyTeamsOverviewAsync();
                TeamsOverview = new ObservableCollection<TeamOverview>(list ?? new System.Collections.Generic.List<TeamOverview>());
            }
            catch (System.Exception ex) { _dialog.ShowError($"Ошибка: {ex.Message}"); }
        }

        private async Task LoadAllProjectsForJuryAsync()
        {
            try
            {
                var list = await _api.GetProjectsForJuryAsync();
                ProjectsForJury = new ObservableCollection<Project>(list ?? new System.Collections.Generic.List<Project>());
                AllProjectsForOrganizer = new ObservableCollection<Project>(list ?? new System.Collections.Generic.List<Project>());
            }
            catch { }
        }

        private void OpenScoreWindow()
        {
            if (SelectedProject == null)
            {
                _dialog.ShowWarning("Выберите проект в таблице");
                return;
            }
            var win = new Views.ScoreProjectWindow(SelectedProject)
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        }

        private void OpenProjectDetails(Project p)
        {
            if (p == null) return;
            var win = new Views.ProjectDetailsWindow(p)
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        }

        private void OpenChat(Hackathon h)
        {
            if (h == null) return;
            var win = new Views.ChatWindow(h.Id, h.Title)
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        }

        private void OpenChatFromList(HackathonChat chat)
        {
            if (chat == null) return;
            var win = new Views.ChatWindow(chat.Id, chat.Title)
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        }

        private void OpenTeamChat()
        {
            var team = MyTeams.FirstOrDefault();
            if (team == null)
            {
                _dialog.ShowWarning("Вы не состоите ни в одной команде");
                return;
            }

            var win = new Views.ChatWindow(team.HackathonId, team.HackathonTitle ?? "Хакатон")
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        }

        private void ShowRules()
        {
            var win = new Views.RulesWindow
            {
                Owner = Application.Current.MainWindow
            };
            win.ShowDialog();
        }

        public async Task AcceptRequestByIdAsync(int invId)
        {
            try
            {
                var resp = await _api.AcceptRequestAsync(invId);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Заявка принята");
                    await LoadPendingRequestsAsync();
                    await LoadMyTeamsAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        public async Task RejectRequestByIdAsync(int invId)
        {
            try
            {
                var resp = await _api.RejectRequestAsync(invId);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Заявка отклонена");
                    await LoadPendingRequestsAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task AcceptInvitationAsync(TeamInvitation inv)
        {
            if (inv == null) return;
            try
            {
                var resp = await _api.AcceptInvitationAsync(inv.Id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Вы вступили в команду!");
                    await LoadMyInvitationsAsync();
                    await LoadMyTeamsAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task RejectInvitationAsync(TeamInvitation inv)
        {
            if (inv == null) return;
            try
            {
                var resp = await _api.RejectInvitationAsync(inv.Id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Приглашение отклонено");
                    await LoadMyInvitationsAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task SetRoleAsync(string role)
        {
            if (SelectedUser == null) return;
            if (!_dialog.Confirm($"Сменить роль пользователя {SelectedUser.FullName} на '{role}'?")) return;
            try
            {
                var resp = await _api.ChangeUserRoleAsync(SelectedUser.Id, role);
                if (resp.Success) { _dialog.ShowInfo("Роль изменена"); await LoadUsersAsync(); }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task BlockUserAsync(bool block)
        {
            if (SelectedUser == null) return;
            try
            {
                var resp = await _api.BlockUserAsync(SelectedUser.Id, block);
                if (resp.Success) { _dialog.ShowInfo(block ? "Заблокирован" : "Разблокирован"); await LoadUsersAsync(); }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task CreateUserAsync()
        {
            if (string.IsNullOrWhiteSpace(NewUserName) ||
                string.IsNullOrWhiteSpace(NewUserEmail) ||
                string.IsNullOrWhiteSpace(NewUserPassword))
            {
                _dialog.ShowWarning("Заполните все поля");
                return;
            }
            if (NewUserPassword.Length < 8)
            {
                _dialog.ShowWarning("Пароль минимум 8 символов");
                return;
            }
            try
            {
                var resp = await _api.CreateUserAsync(NewUserName, NewUserEmail, NewUserPassword, NewUserRole);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Пользователь создан!");
                    NewUserName = ""; NewUserEmail = ""; NewUserPassword = ""; NewUserRole = "participant";
                    await LoadUsersAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(EditFullName))
            {
                _dialog.ShowWarning("ФИО не может быть пустым");
                return;
            }
            try
            {
                var resp = await _api.UpdateProfileAsync(EditFullName, EditPhone, EditSkills);
                if (resp.Success && resp.Data != null)
                {
                    _dialog.ShowInfo("Профиль обновлён!");
                    CurrentUser = resp.Data;
                    _api.SetToken(_api.Token, resp.Data);
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
            {
                _dialog.ShowWarning("Заполните оба поля");
                return;
            }
            if (NewPassword.Length < 8)
            {
                _dialog.ShowWarning("Новый пароль минимум 8 символов");
                return;
            }
            if (NewPassword != NewPasswordConfirm)
            {
                _dialog.ShowWarning("Новые пароли не совпадают");
                return;
            }
            try
            {
                var resp = await _api.ChangePasswordAsync(OldPassword, NewPassword);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Пароль изменён!");
                    OldPassword = ""; NewPassword = ""; NewPasswordConfirm = "";
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private async Task UploadAvatarAsync()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.gif;*.webp"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var resp = await _api.UploadAvatarAsync(dlg.FileName);
                if (resp.Success)
                {
                    string json;
                    if (resp.Data is System.Text.Json.JsonElement je)
                        json = je.GetRawText();
                    else
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(resp.Data);

                    var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var url = obj["url"]?.ToString();

                    if (!string.IsNullOrEmpty(url))
                    {
                        var upd = await _api.UpdateProfileWithAvatarAsync(
                            CurrentUser.FullName, CurrentUser.Phone,
                            CurrentUser.Skills, url);

                        if (upd.Success && upd.Data != null)
                        {
                            CurrentUser = upd.Data;
                            _api.SetToken(_api.Token, upd.Data);
                            _dialog.ShowInfo("Аватар загружен!");
                        }
                        else _dialog.ShowError("Не удалось сохранить URL аватара");
                    }
                    else _dialog.ShowError("Сервер не вернул URL");
                }
                else _dialog.ShowError(resp.Message ?? "Не удалось загрузить аватар");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private void ExportRatingsCsv()
        {
            if (Ratings.Count == 0) { _dialog.ShowWarning("Нет данных"); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "CSV|*.csv", FileName = "rating.csv" };
            if (dlg.ShowDialog() != true) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Место;Команда;Проект;Очки;Судей");
            foreach (var r in Ratings)
                sb.AppendLine($"{r.Position};{r.TeamName};{r.ProjectTitle};{r.TotalScore};{r.JuryCount}");
            System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
            _dialog.ShowInfo("Сохранено: " + dlg.FileName);
        }

        private void ExportRatingsExcel()
        {
            if (Ratings.Count == 0) { _dialog.ShowWarning("Нет данных"); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "rating.xlsx" };
            if (dlg.ShowDialog() != true) return;

            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Рейтинг");
                ws.Cell(1, 1).Value = "Место";
                ws.Cell(1, 2).Value = "Команда";
                ws.Cell(1, 3).Value = "Проект";
                ws.Cell(1, 4).Value = "Очки";
                ws.Cell(1, 5).Value = "Судей";
                for (int i = 0; i < Ratings.Count; i++)
                {
                    var r = Ratings[i];
                    ws.Cell(i + 2, 1).Value = r.Position;
                    ws.Cell(i + 2, 2).Value = r.TeamName ?? "";
                    ws.Cell(i + 2, 3).Value = r.ProjectTitle ?? "";
                    ws.Cell(i + 2, 4).Value = (double)r.TotalScore;
                    ws.Cell(i + 2, 5).Value = r.JuryCount;
                }
                ws.Columns().AdjustToContents();
                wb.SaveAs(dlg.FileName);
            }
            _dialog.ShowInfo("Сохранено: " + dlg.FileName);
        }

        private void ExportRatingsPdf()
        {
            if (Ratings.Count == 0) { _dialog.ShowWarning("Нет данных"); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PDF|*.pdf", FileName = "rating.pdf" };
            if (dlg.ShowDialog() != true) return;

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text("Протокол рейтинга").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(50);
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.ConstantColumn(60);
                            c.ConstantColumn(60);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("#").Bold();
                            h.Cell().Text("Команда").Bold();
                            h.Cell().Text("Проект").Bold();
                            h.Cell().Text("Очки").Bold();
                            h.Cell().Text("Судей").Bold();
                        });
                        foreach (var r in Ratings)
                        {
                            table.Cell().Text(r.Position.ToString());
                            table.Cell().Text(r.TeamName ?? "");
                            table.Cell().Text(r.ProjectTitle ?? "");
                            table.Cell().Text(r.TotalScore.ToString());
                            table.Cell().Text(r.JuryCount.ToString());
                        }
                    });
                });
            }).GeneratePdf(dlg.FileName);

            _dialog.ShowInfo("Сохранено: " + dlg.FileName);
        }

        private void ExportTeamsCsv()
        {
            if (TeamsOverview.Count == 0) { _dialog.ShowWarning("Нет данных"); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "CSV|*.csv", FileName = "teams.csv" };
            if (dlg.ShowDialog() != true) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Команда;Хакатон;Капитан;Участников");
            foreach (var t in TeamsOverview)
                sb.AppendLine($"{t.TeamName};{t.HackathonTitle};{t.CaptainName};{t.MembersCount}");
            System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
            _dialog.ShowInfo("Сохранено: " + dlg.FileName);
        }

        private void ExportTeamsExcel()
        {
            if (TeamsOverview.Count == 0) { _dialog.ShowWarning("Нет данных"); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "Excel|*.xlsx", FileName = "teams.xlsx" };
            if (dlg.ShowDialog() != true) return;

            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Команды");
                ws.Cell(1, 1).Value = "Команда";
                ws.Cell(1, 2).Value = "Хакатон";
                ws.Cell(1, 3).Value = "Капитан";
                ws.Cell(1, 4).Value = "Участников";
                for (int i = 0; i < TeamsOverview.Count; i++)
                {
                    var t = TeamsOverview[i];
                    ws.Cell(i + 2, 1).Value = t.TeamName ?? "";
                    ws.Cell(i + 2, 2).Value = t.HackathonTitle ?? "";
                    ws.Cell(i + 2, 3).Value = t.CaptainName ?? "";
                    ws.Cell(i + 2, 4).Value = t.MembersCount;
                }
                ws.Columns().AdjustToContents();
                wb.SaveAs(dlg.FileName);
            }
            _dialog.ShowInfo("Сохранено: " + dlg.FileName);
        }

        private void ExportProjectsCsv()
        {
            if (AllProjectsForOrganizer.Count == 0) { _dialog.ShowWarning("Нет данных"); return; }
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "CSV|*.csv", FileName = "projects.csv" };
            if (dlg.ShowDialog() != true) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Проект;Команда;Хакатон;Репозиторий;Загружен");
            foreach (var p in AllProjectsForOrganizer)
                sb.AppendLine($"{p.Title};{p.TeamName};{p.HackathonTitle};{p.RepositoryUrl};{p.UploadedAt:dd.MM.yyyy}");
            System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
            _dialog.ShowInfo("Сохранено: " + dlg.FileName);
        }

        private void ViewTeamDetails(TeamOverview team)
        {
            if (team == null) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Команда: {team.TeamName}");
            sb.AppendLine($"Хакатон: {team.HackathonTitle}");
            sb.AppendLine($"Капитан: {team.CaptainName}");
            sb.AppendLine($"Участников: {team.MembersCount}");
            sb.AppendLine();
            sb.AppendLine("Состав:");
            if (team.Members != null)
            {
                foreach (var m in team.Members)
                    sb.AppendLine($"• {m.FullName} ({m.Email})");
            }

            MessageBox.Show(sb.ToString(), "Детали команды",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task DeleteHackathonAsync(Hackathon h)
        {
            if (h == null) return;
            if (!_dialog.Confirm($"Удалить хакатон \"{h.Title}\"?\nВсе команды и проекты тоже будут удалены."))
                return;

            try
            {
                var resp = await _api.DeleteHackathonAsync(h.Id);
                if (resp.Success)
                {
                    _dialog.ShowInfo("Хакатон удалён");
                    await LoadDataAsync();
                }
                else _dialog.ShowError(resp.Message ?? "Ошибка удаления");
            }
            catch (System.Exception ex) { _dialog.ShowError(ex.Message); }
        }

        private void Logout()
        {
            if (!_dialog.Confirm("Вы действительно хотите выйти?")) return;
            _api.Logout();
            var windows = Application.Current.Windows.Cast<Window>().ToList();
            foreach (var w in windows)
                if (!(w is Views.LoginWindow)) w.Close();
            var login = new Views.LoginWindow();
            login.Show();
            Application.Current.MainWindow = login;
        }

        private void OpenCreateHackathon()
        {
            var win = new Views.CreateHackathonWindow();
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true) _ = LoadDataAsync();
        }

        private void OpenHackathonDetails(Hackathon h)
        {
            if (h == null) return;
            var win = new Views.HackathonDetailsWindow(h);
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
            _ = LoadDataAsync();
        }
    }
}