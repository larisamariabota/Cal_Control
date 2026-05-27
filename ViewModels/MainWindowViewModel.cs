using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UniApp.Models;
using UniApp.Services;

namespace UniApp.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isLoggedIn;
        private bool _isStatsVisible;
        private User? _currentUser;
        private DateTimeOffset _selectedDate = DateTimeOffset.Now;
        private string _foodSearchText = string.Empty;
        private FoodCatalogItem? _selectedFood;
        private FoodLog? _selectedFoodLog;
        private int? _editingFoodLogId;
        private string _gramsString = "100";
        private string _selectedMealType = "Dimineata";
        private int _previewFoodCalories;
        private SportOption? _selectedSport;
        private string _sportMinutesString = "30";
        private int _previewSportCalories;
        private int _totalFoodCalories;
        private int _totalBurnedCalories;
        private int _netCalories;
        private int _breakfastCalories;
        private int _lunchCalories;
        private int _dinnerCalories;
        private int _snackCalories;
        private int _weekFoodCalories;
        private int _weekBurnedCalories;
        private int _weekNetCalories;
        private double _breakfastBarWidth;
        private double _lunchBarWidth;
        private double _dinnerBarWidth;
        private double _snackBarWidth;
        private double _sportBarWidth;
        private UserProfile? _profile;
        private string _ageString = "25";
        private string _heightString = "170";
        private string _weightString = "70";
        private string _selectedSex = "F";
        private string _selectedActivityLevel = "Moderat";
        private string _selectedGoal = "Mentinere";
        private int _dailyCalorieGoal = 2000;
        private int _remainingCalories;
        private double _proteinTotal;
        private double _carbsTotal;
        private double _fatTotal;
        private string _targetStatus = string.Empty;
        private FavoriteMeal? _selectedFavoriteMeal;
        private string _lastWeeklyReportPath = string.Empty;

        public ObservableCollection<FoodLog> TodayFoodLogs { get; } = new ObservableCollection<FoodLog>();
        public ObservableCollection<SportLog> TodaySportLogs { get; } = new ObservableCollection<SportLog>();
        public ObservableCollection<WeeklyStat> WeeklyStats { get; } = new ObservableCollection<WeeklyStat>();
        public ObservableCollection<FoodCatalogItem> FoodSuggestions { get; } = new ObservableCollection<FoodCatalogItem>();
        public ObservableCollection<string> Recommendations { get; } = new ObservableCollection<string>();
        public ObservableCollection<FavoriteMeal> FavoriteMeals { get; } = new ObservableCollection<FavoriteMeal>();
        public ObservableCollection<DayStatus> DayStatuses { get; } = new ObservableCollection<DayStatus>();
        public List<FoodCatalogItem> FoodOptions { get; } = FoodCatalogService.LoadFoods();
        public List<SportOption> SportOptions { get; } = new List<SportOption>
        {
            new SportOption { Name = "Mers pe jos", CaloriesPerHour = 220 },
            new SportOption { Name = "Alergare usoara", CaloriesPerHour = 520 },
            new SportOption { Name = "Ciclism", CaloriesPerHour = 450 },
            new SportOption { Name = "Inot", CaloriesPerHour = 500 },
            new SportOption { Name = "Fitness", CaloriesPerHour = 430 },
            new SportOption { Name = "Dans", CaloriesPerHour = 330 },
            new SportOption { Name = "Fotbal", CaloriesPerHour = 600 },
            new SportOption { Name = "Yoga", CaloriesPerHour = 180 }
        };
        public List<string> MealTypes { get; } = new List<string> { "Dimineata", "Amiaza", "Seara", "Gustare" };
        public List<string> SexOptions { get; } = new List<string> { "F", "M" };
        public List<string> ActivityLevels { get; } = new List<string> { "Sedentar", "Usor", "Moderat", "Activ" };
        public List<string> GoalOptions { get; } = new List<string> { "Slabire", "Mentinere", "Masa musculara" };

        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public bool IsLoggedIn { get => _isLoggedIn; set { _isLoggedIn = value; OnPropertyChanged(); } }
        public bool IsAuthVisible => !IsLoggedIn;
        public bool IsTodayVisible => IsLoggedIn && !IsStatsVisible;
        public bool IsStatsVisible { get => IsLoggedIn && _isStatsVisible; set { _isStatsVisible = value; OnPropertyChanged(); } }
        public string DatabasePath => DatabaseService.DatabasePath;
        public string JsonExportPath => DatabaseService.JsonExportPath;
        public string WelcomeMessage => CurrentUser == null ? string.Empty : $"Salut, {CurrentUser.Username}!";
        public string SelectedDayTitle => $"Zi selectata: {SelectedDate.DateTime:dd.MM.yyyy}";
        public User? CurrentUser { get => _currentUser; set { _currentUser = value; OnPropertyChanged(); } }

        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDayTitle));
                CancelFoodEdit();
                LoadSelectedDayData();
                LoadWeekData();
            }
        }

        public string FoodSearchText
        {
            get => _foodSearchText;
            set
            {
                _foodSearchText = value;
                OnPropertyChanged();
                UpdateFoodSuggestions();
                RecalculateFoodPreview();
            }
        }

        public FoodCatalogItem? SelectedFood
        {
            get => _selectedFood;
            set
            {
                _selectedFood = value;
                if (value != null)
                {
                    _foodSearchText = value.Name;
                    OnPropertyChanged(nameof(FoodSearchText));
                    FoodSuggestions.Clear();
                }
                OnPropertyChanged();
                RecalculateFoodPreview();
            }
        }

        public FoodLog? SelectedFoodLog { get => _selectedFoodLog; set { _selectedFoodLog = value; OnPropertyChanged(); } }
        public bool IsEditingFood => _editingFoodLogId.HasValue;
        public string FoodActionText => IsEditingFood ? "Salveaza modificarea" : "Salveaza masa";

        public string GramsString
        {
            get => _gramsString;
            set
            {
                _gramsString = value;
                OnPropertyChanged();
                RecalculateFoodPreview();
            }
        }

        public string SelectedMealType { get => _selectedMealType; set { _selectedMealType = value; OnPropertyChanged(); } }
        public int PreviewFoodCalories { get => _previewFoodCalories; set { _previewFoodCalories = value; OnPropertyChanged(); } }

        public SportOption? SelectedSport
        {
            get => _selectedSport;
            set
            {
                _selectedSport = value;
                OnPropertyChanged();
                RecalculateSportPreview();
            }
        }

        public string SportMinutesString
        {
            get => _sportMinutesString;
            set
            {
                _sportMinutesString = value;
                OnPropertyChanged();
                RecalculateSportPreview();
            }
        }

        public int PreviewSportCalories { get => _previewSportCalories; set { _previewSportCalories = value; OnPropertyChanged(); } }
        public int TotalFoodCalories { get => _totalFoodCalories; set { _totalFoodCalories = value; OnPropertyChanged(); } }
        public int TotalBurnedCalories { get => _totalBurnedCalories; set { _totalBurnedCalories = value; OnPropertyChanged(); } }
        public int NetCalories { get => _netCalories; set { _netCalories = value; OnPropertyChanged(); } }
        public int BreakfastCalories { get => _breakfastCalories; set { _breakfastCalories = value; OnPropertyChanged(); } }
        public int LunchCalories { get => _lunchCalories; set { _lunchCalories = value; OnPropertyChanged(); } }
        public int DinnerCalories { get => _dinnerCalories; set { _dinnerCalories = value; OnPropertyChanged(); } }
        public int SnackCalories { get => _snackCalories; set { _snackCalories = value; OnPropertyChanged(); } }
        public int WeekFoodCalories { get => _weekFoodCalories; set { _weekFoodCalories = value; OnPropertyChanged(); } }
        public int WeekBurnedCalories { get => _weekBurnedCalories; set { _weekBurnedCalories = value; OnPropertyChanged(); } }
        public int WeekNetCalories { get => _weekNetCalories; set { _weekNetCalories = value; OnPropertyChanged(); } }
        public double BreakfastBarWidth { get => _breakfastBarWidth; set { _breakfastBarWidth = value; OnPropertyChanged(); } }
        public double LunchBarWidth { get => _lunchBarWidth; set { _lunchBarWidth = value; OnPropertyChanged(); } }
        public double DinnerBarWidth { get => _dinnerBarWidth; set { _dinnerBarWidth = value; OnPropertyChanged(); } }
        public double SnackBarWidth { get => _snackBarWidth; set { _snackBarWidth = value; OnPropertyChanged(); } }
        public double SportBarWidth { get => _sportBarWidth; set { _sportBarWidth = value; OnPropertyChanged(); } }
        public string AgeString { get => _ageString; set { _ageString = value; OnPropertyChanged(); } }
        public string HeightString { get => _heightString; set { _heightString = value; OnPropertyChanged(); } }
        public string WeightString { get => _weightString; set { _weightString = value; OnPropertyChanged(); } }
        public string SelectedSex { get => _selectedSex; set { _selectedSex = value; OnPropertyChanged(); } }
        public string SelectedActivityLevel { get => _selectedActivityLevel; set { _selectedActivityLevel = value; OnPropertyChanged(); } }
        public string SelectedGoal { get => _selectedGoal; set { _selectedGoal = value; OnPropertyChanged(); } }
        public int DailyCalorieGoal { get => _dailyCalorieGoal; set { _dailyCalorieGoal = value; OnPropertyChanged(); } }
        public int RemainingCalories { get => _remainingCalories; set { _remainingCalories = value; OnPropertyChanged(); } }
        public double ProteinTotal { get => _proteinTotal; set { _proteinTotal = value; OnPropertyChanged(); } }
        public double CarbsTotal { get => _carbsTotal; set { _carbsTotal = value; OnPropertyChanged(); } }
        public double FatTotal { get => _fatTotal; set { _fatTotal = value; OnPropertyChanged(); } }
        public string TargetStatus { get => _targetStatus; set { _targetStatus = value; OnPropertyChanged(); } }
        public FavoriteMeal? SelectedFavoriteMeal { get => _selectedFavoriteMeal; set { _selectedFavoriteMeal = value; OnPropertyChanged(); } }
        public string LastWeeklyReportPath { get => _lastWeeklyReportPath; set { _lastWeeklyReportPath = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand SaveFoodCommand { get; }
        public ICommand AddSportCommand { get; }
        public ICommand EditSelectedFoodCommand { get; }
        public ICommand DeleteSelectedFoodCommand { get; }
        public ICommand CancelFoodEditCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand AddFavoriteCommand { get; }
        public ICommand UseFavoriteCommand { get; }
        public ICommand ExportWeeklyReportCommand { get; }
        public ICommand ShowTodayCommand { get; }
        public ICommand ShowStatsCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainWindowViewModel()
        {
            SelectedSport = SportOptions.FirstOrDefault();
            UpdateFoodSuggestions();
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
            SaveFoodCommand = new RelayCommand(ExecuteSaveFood);
            AddSportCommand = new RelayCommand(ExecuteAddSport);
            EditSelectedFoodCommand = new RelayCommand(ExecuteEditSelectedFood);
            DeleteSelectedFoodCommand = new RelayCommand(ExecuteDeleteSelectedFood);
            CancelFoodEditCommand = new RelayCommand(CancelFoodEdit);
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile);
            AddFavoriteCommand = new RelayCommand(ExecuteAddFavorite);
            UseFavoriteCommand = new RelayCommand(ExecuteUseFavorite);
            ExportWeeklyReportCommand = new RelayCommand(ExecuteExportWeeklyReport);
            ShowTodayCommand = new RelayCommand(() => IsStatsVisible = false);
            ShowStatsCommand = new RelayCommand(() => { IsStatsVisible = true; LoadWeekData(); });
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        private void ExecuteLogin()
        {
            try
            {
                var user = AuthService.Login(Username, Password);
                if (user != null)
                {
                    CurrentUser = user;
                    IsLoggedIn = true;
                    IsStatsVisible = false;
                    Password = string.Empty;
                    StatusMessage = string.Empty;
                    LoadProfile();
                    LoadFavorites();
                    LoadSelectedDayData();
                    LoadWeekData();
                }
                else
                {
                    StatusMessage = "Eroare: utilizator sau parola incorecta.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la login: {ex.Message}";
            }
        }

        private void ExecuteRegister()
        {
            try
            {
                var success = AuthService.Register(Username, Password);
                StatusMessage = success ? "Cont creat. Acum te poti loga." : "Eroare: nume existent sau invalid.";
                if (success) Password = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Eroare la creare cont: {ex.Message}";
            }
        }

        private void ExecuteSaveFood()
        {
            if (CurrentUser == null) return;

            var food = ResolveFoodFromSearch();
            if (food == null)
            {
                StatusMessage = "Scrie numele alimentului si alege-l din rezultate.";
                return;
            }

            if (!int.TryParse(GramsString, out var grams) || grams <= 0)
            {
                StatusMessage = "Introdu un numar valid de grame.";
                return;
            }

            var calories = CalculateCalories(food.CaloriesPer100g, grams);
            var protein = CalculateMacro(food.ProteinPer100g, grams);
            var carbs = CalculateMacro(food.CarbsPer100g, grams);
            var fat = CalculateMacro(food.FatPer100g, grams);
            var selectedDate = SelectedDate.DateTime.Date;
            var success = IsEditingFood
                ? FoodService.UpdateFoodLog(CurrentUser.Id, _editingFoodLogId!.Value, food.Name, grams, food.CaloriesPer100g, calories, protein, carbs, fat, SelectedMealType, selectedDate)
                : FoodService.AddFoodLog(CurrentUser.Id, food.Name, grams, food.CaloriesPer100g, calories, protein, carbs, fat, SelectedMealType, selectedDate);

            if (success)
            {
                StatusMessage = IsEditingFood ? "Masa a fost modificata." : "Masa a fost salvata.";
                CancelFoodEdit();
                LoadSelectedDayData();
                LoadWeekData();
            }
        }

        private void ExecuteAddSport()
        {
            if (CurrentUser == null || SelectedSport == null) return;

            if (!int.TryParse(SportMinutesString, out var minutes) || minutes <= 0)
            {
                StatusMessage = "Introdu un timp valid pentru sport.";
                return;
            }

            var calories = CalculateSportCalories(SelectedSport.CaloriesPerHour, minutes);
            var success = SportService.AddSportLog(CurrentUser.Id, SelectedSport.Name, minutes, calories, SelectedDate.DateTime.Date);
            if (success)
            {
                SportMinutesString = "30";
                StatusMessage = "Sportul a fost salvat si scazut din total.";
                LoadSelectedDayData();
                LoadWeekData();
            }
        }

        private void ExecuteEditSelectedFood()
        {
            if (SelectedFoodLog == null)
            {
                StatusMessage = "Selecteaza o masa din lista.";
                return;
            }

            _editingFoodLogId = SelectedFoodLog.Id;
            FoodSearchText = SelectedFoodLog.FoodName;
            GramsString = SelectedFoodLog.Grams.ToString();
            SelectedMealType = SelectedFoodLog.MealType;
            SelectedFood = FoodOptions.FirstOrDefault(f => f.Name.Equals(SelectedFoodLog.FoodName, StringComparison.OrdinalIgnoreCase));
            OnPropertyChanged(nameof(IsEditingFood));
            OnPropertyChanged(nameof(FoodActionText));
            StatusMessage = "Modifici masa selectata.";
        }

        private void ExecuteDeleteSelectedFood()
        {
            if (CurrentUser == null || SelectedFoodLog == null)
            {
                StatusMessage = "Selecteaza o masa din lista.";
                return;
            }

            if (FoodService.DeleteFoodLog(CurrentUser.Id, SelectedFoodLog.Id))
            {
                StatusMessage = "Masa a fost stearsa.";
                CancelFoodEdit();
                LoadSelectedDayData();
                LoadWeekData();
            }
        }

        private void ExecuteLogout()
        {
            CurrentUser = null;
            IsLoggedIn = false;
            IsStatsVisible = false;
            Username = string.Empty;
            Password = string.Empty;
            StatusMessage = string.Empty;
            TodayFoodLogs.Clear();
            TodaySportLogs.Clear();
            WeeklyStats.Clear();
            Recommendations.Clear();
            FavoriteMeals.Clear();
            DayStatuses.Clear();
            CancelFoodEdit();
            ResetTotals();
        }

        private void LoadSelectedDayData()
        {
            if (CurrentUser == null) return;

            var date = SelectedDate.DateTime.Date;
            var foodLogs = FoodService.GetLogsForDate(CurrentUser.Id, date);
            var sportLogs = SportService.GetLogsForDate(CurrentUser.Id, date);

            TodayFoodLogs.Clear();
            foreach (var log in foodLogs) TodayFoodLogs.Add(log);

            TodaySportLogs.Clear();
            foreach (var log in sportLogs) TodaySportLogs.Add(log);

            BreakfastCalories = foodLogs.Where(l => l.MealType == "Dimineata").Sum(l => l.Calories);
            LunchCalories = foodLogs.Where(l => l.MealType == "Amiaza").Sum(l => l.Calories);
            DinnerCalories = foodLogs.Where(l => l.MealType == "Seara").Sum(l => l.Calories);
            SnackCalories = foodLogs.Where(l => l.MealType == "Gustare").Sum(l => l.Calories);
            TotalFoodCalories = foodLogs.Sum(l => l.Calories);
            TotalBurnedCalories = sportLogs.Sum(l => l.CaloriesBurned);
            NetCalories = TotalFoodCalories - TotalBurnedCalories;
            ProteinTotal = foodLogs.Sum(l => l.Protein);
            CarbsTotal = foodLogs.Sum(l => l.Carbs);
            FatTotal = foodLogs.Sum(l => l.Fat);
            RemainingCalories = DailyCalorieGoal - NetCalories;
            TargetStatus = BuildTargetStatus();
            UpdateDayChartBars();
            BuildRecommendations(foodLogs, sportLogs);
        }

        private void LoadWeekData()
        {
            if (CurrentUser == null) return;

            var end = SelectedDate.DateTime.Date;
            var start = end.AddDays(-6);
            var foods = FoodService.GetLogsBetween(CurrentUser.Id, start, end);
            var sports = SportService.GetLogsBetween(CurrentUser.Id, start, end);
            var maxValue = 1;

            WeeklyStats.Clear();
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var foodCalories = foods.Where(f => f.Date.Date == date.Date).Sum(f => f.Calories);
                var burnedCalories = sports.Where(s => s.Date.Date == date.Date).Sum(s => s.CaloriesBurned);
                maxValue = Math.Max(maxValue, Math.Max(foodCalories, burnedCalories));
                WeeklyStats.Add(new WeeklyStat
                {
                    DayLabel = date.ToString("dd.MM"),
                    FoodCalories = foodCalories,
                    BurnedCalories = burnedCalories,
                    NetCalories = foodCalories - burnedCalories,
                });
            }

            foreach (var stat in WeeklyStats)
            {
                stat.FoodBarWidth = stat.FoodCalories * 220.0 / maxValue;
                stat.BurnedBarWidth = stat.BurnedCalories * 220.0 / maxValue;
            }

            WeekFoodCalories = foods.Sum(f => f.Calories);
            WeekBurnedCalories = sports.Sum(s => s.CaloriesBurned);
            WeekNetCalories = WeekFoodCalories - WeekBurnedCalories;
            BuildDayStatuses(foods, sports, start, end);
        }

        private void UpdateFoodSuggestions()
        {
            FoodSuggestions.Clear();
            var query = FoodSearchText.Trim();
            var matches = string.IsNullOrWhiteSpace(query)
                ? FoodOptions.Take(8)
                : FoodOptions
                    .Where(f => f.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || f.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Take(8);

            foreach (var food in matches)
            {
                FoodSuggestions.Add(food);
            }

            SelectedFood = FoodOptions.FirstOrDefault(f => f.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
        }

        private FoodCatalogItem? ResolveFoodFromSearch()
        {
            if (SelectedFood != null && SelectedFood.Name.Equals(FoodSearchText.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return SelectedFood;
            }

            return FoodOptions.FirstOrDefault(f => f.Name.Equals(FoodSearchText.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private void CancelFoodEdit()
        {
            _editingFoodLogId = null;
            SelectedFoodLog = null;
            SelectedFood = null;
            FoodSearchText = string.Empty;
            GramsString = "100";
            SelectedMealType = "Dimineata";
            OnPropertyChanged(nameof(IsEditingFood));
            OnPropertyChanged(nameof(FoodActionText));
        }

        private void RecalculateFoodPreview()
        {
            var food = ResolveFoodFromSearch();
            PreviewFoodCalories = food != null && int.TryParse(GramsString, out var grams)
                ? CalculateCalories(food.CaloriesPer100g, grams)
                : 0;
        }

        private void RecalculateSportPreview()
        {
            PreviewSportCalories = SelectedSport != null && int.TryParse(SportMinutesString, out var minutes)
                ? CalculateSportCalories(SelectedSport.CaloriesPerHour, minutes)
                : 0;
        }

        private void LoadProfile()
        {
            if (CurrentUser == null) return;

            _profile = ProfileService.GetOrCreateProfile(CurrentUser.Id);
            AgeString = _profile.Age.ToString();
            HeightString = _profile.HeightCm.ToString();
            WeightString = _profile.WeightKg.ToString("0.#");
            SelectedSex = _profile.Sex;
            SelectedActivityLevel = _profile.ActivityLevel;
            SelectedGoal = _profile.Goal;
            DailyCalorieGoal = _profile.DailyCalorieGoal;
        }

        private void ExecuteSaveProfile()
        {
            if (CurrentUser == null) return;
            if (!int.TryParse(AgeString, out var age) || !int.TryParse(HeightString, out var height) || !double.TryParse(WeightString, out var weight))
            {
                StatusMessage = "Completeaza profilul cu valori numerice valide.";
                return;
            }

            _profile = new UserProfile
            {
                UserId = CurrentUser.Id,
                Age = age,
                HeightCm = height,
                WeightKg = weight,
                Sex = SelectedSex,
                ActivityLevel = SelectedActivityLevel,
                Goal = SelectedGoal
            };
            ProfileService.SaveProfile(_profile);
            DailyCalorieGoal = _profile.DailyCalorieGoal;
            StatusMessage = $"Profil salvat. Target automat: {DailyCalorieGoal} kcal/zi.";
            LoadSelectedDayData();
            LoadWeekData();
        }

        private void LoadFavorites()
        {
            if (CurrentUser == null) return;
            FavoriteMeals.Clear();
            foreach (var favorite in FavoriteMealService.GetFavorites(CurrentUser.Id))
            {
                FavoriteMeals.Add(favorite);
            }
        }

        private void ExecuteAddFavorite()
        {
            if (CurrentUser == null) return;
            var food = ResolveFoodFromSearch();
            if (food == null || !int.TryParse(GramsString, out var grams) || grams <= 0)
            {
                StatusMessage = "Alege un aliment si gramele pentru favorita.";
                return;
            }

            if (FavoriteMealService.AddFavorite(CurrentUser.Id, food.Name, grams, food.CaloriesPer100g, SelectedMealType))
            {
                StatusMessage = "Masa rapida a fost salvata.";
                LoadFavorites();
            }
        }

        private void ExecuteUseFavorite()
        {
            if (SelectedFavoriteMeal == null)
            {
                StatusMessage = "Selecteaza o masa rapida.";
                return;
            }

            FoodSearchText = SelectedFavoriteMeal.FoodName;
            SelectedFood = FoodOptions.FirstOrDefault(f => f.Name.Equals(SelectedFavoriteMeal.FoodName, StringComparison.OrdinalIgnoreCase));
            GramsString = SelectedFavoriteMeal.Grams.ToString();
            SelectedMealType = SelectedFavoriteMeal.MealType;
            ExecuteSaveFood();
        }

        private void ExecuteExportWeeklyReport()
        {
            if (CurrentUser == null) return;
            LastWeeklyReportPath = WeeklyReportService.ExportWeeklyReport(CurrentUser.Id, SelectedDate.DateTime.Date);
            StatusMessage = $"Raport exportat: {LastWeeklyReportPath}";
        }

        private void BuildRecommendations(List<FoodLog> foodLogs, List<SportLog> sportLogs)
        {
            Recommendations.Clear();
            if (DailyCalorieGoal > 0 && NetCalories > DailyCalorieGoal)
            {
                Recommendations.Add($"Ai depasit targetul cu {NetCalories - DailyCalorieGoal} kcal.");
            }
            else if (DailyCalorieGoal > 0)
            {
                Recommendations.Add($"Mai ai {DailyCalorieGoal - NetCalories} kcal pana la target.");
            }

            if (ProteinTotal < 60) Recommendations.Add("Ai proteine putine azi. Poti adauga oua, iaurt, pui sau leguminoase.");
            if (!foodLogs.Any(l => l.MealType == "Dimineata")) Recommendations.Add("Nu ai introdus micul dejun pentru ziua selectata.");
            if (sportLogs.Count == 0) Recommendations.Add("Nu ai sport salvat pentru ziua selectata.");
            if (foodLogs.Count == 0) Recommendations.Add("Ziua nu are mese salvate inca.");
        }

        private void BuildDayStatuses(List<FoodLog> foods, List<SportLog> sports, DateTime start, DateTime end)
        {
            DayStatuses.Clear();
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var net = foods.Where(f => f.Date.Date == date.Date).Sum(f => f.Calories) -
                          sports.Where(s => s.Date.Date == date.Date).Sum(s => s.CaloriesBurned);
                var diff = net - DailyCalorieGoal;
                var text = diff <= 0 ? "in target" : diff <= 250 ? "aproape" : "peste";
                var color = diff <= 0 ? "verde" : diff <= 250 ? "portocaliu" : "rosu";
                if (sports.Any(s => s.Date.Date == date.Date)) color += " + sport";
                DayStatuses.Add(new DayStatus
                {
                    DayLabel = date.ToString("dd.MM"),
                    NetCalories = net,
                    StatusText = text,
                    ColorName = color
                });
            }
        }

        private string BuildTargetStatus()
        {
            if (DailyCalorieGoal <= 0) return "Fara target setat";
            return RemainingCalories >= 0
                ? $"Mai ai {RemainingCalories} kcal disponibile"
                : $"Ai depasit cu {Math.Abs(RemainingCalories)} kcal";
        }

        private void UpdateDayChartBars()
        {
            var maxValue = Math.Max(1, new[] { BreakfastCalories, LunchCalories, DinnerCalories, SnackCalories, TotalBurnedCalories }.Max());
            BreakfastBarWidth = BreakfastCalories * 180.0 / maxValue;
            LunchBarWidth = LunchCalories * 180.0 / maxValue;
            DinnerBarWidth = DinnerCalories * 180.0 / maxValue;
            SnackBarWidth = SnackCalories * 180.0 / maxValue;
            SportBarWidth = TotalBurnedCalories * 180.0 / maxValue;
        }

        private static int CalculateCalories(int caloriesPer100g, int grams)
        {
            return (int)Math.Round(caloriesPer100g * grams / 100.0);
        }

        private static double CalculateMacro(double macroPer100g, int grams)
        {
            return Math.Round(macroPer100g * grams / 100.0, 1);
        }

        private static int CalculateSportCalories(int caloriesPerHour, int minutes)
        {
            return (int)Math.Round(caloriesPerHour * minutes / 60.0);
        }

        private void ResetTotals()
        {
            TotalFoodCalories = 0;
            TotalBurnedCalories = 0;
            NetCalories = 0;
            BreakfastCalories = 0;
            LunchCalories = 0;
            DinnerCalories = 0;
            SnackCalories = 0;
            ProteinTotal = 0;
            CarbsTotal = 0;
            FatTotal = 0;
            RemainingCalories = 0;
            TargetStatus = string.Empty;
            WeekFoodCalories = 0;
            WeekBurnedCalories = 0;
            WeekNetCalories = 0;
            UpdateDayChartBars();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(IsLoggedIn))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAuthVisible)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTodayVisible)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStatsVisible)));
            }

            if (propertyName == nameof(IsStatsVisible))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTodayVisible)));
            }

            if (propertyName == nameof(CurrentUser))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeMessage)));
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
