using System.Globalization;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PlantProduction.Desktop;

public partial class MainWindow : Window
{
    private const bool LaboratoryClient = false;
    private readonly DesktopApiClient _apiClient = new();
    private readonly JsonSerializerOptions _prettyJsonOptions = new() { WriteIndented = true };
    private LoginResponse? _currentUser;

    public MainWindow()
    {
        InitializeComponent();
        CurrentUserTextBlock.Text = "Модуль технолога";
        _apiClient.SetBaseUrl(ApiUrlTextBox.Text ?? "http://localhost:5114");
        ApplyRoleLayout();
        ShowSection("Главная");
        UpdateHomeDashboard();
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            _apiClient.SetBaseUrl(ApiUrlTextBox.Text ?? "http://localhost:5114");
            _currentUser = await _apiClient.LoginAsync(LoginTextBox.Text ?? string.Empty, PasswordTextBox.Text ?? string.Empty);

            if (!string.Equals(_currentUser.RoleCode, "TECHNOLOGIST", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(_currentUser.RoleCode, "ADMINISTRATOR", StringComparison.OrdinalIgnoreCase))
            {
                _apiClient.SetToken(null);
                _currentUser = null;
                CurrentUserTextBlock.Text = "Модуль технолога";
                SetStatus("Для этого приложения нужен вход под технологом.");
                return;
            }

            CurrentUserTextBlock.Text = $"{_currentUser.FullName} | {GetRoleTitle(_currentUser.RoleCode)} | {_currentUser.DepartmentName}";
            ApplyRoleLayout();
            ShowSection("Главная");
            SetStatus($"Авторизация выполнена: {_currentUser.Login}");
            await RefreshAllAsync();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
        }
    }

    private async void RefreshAllButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(RefreshAllAsync);
    }

    private async Task RefreshAllAsync()
    {
        EnsureLoggedIn();
        await LoadCatalogsAsync();
        await LoadRecipesAsync();
        await LoadTechnologyCardsAsync();
        await LoadProductionAsync();
        await LoadDeviationsAsync();
        await LoadLaboratoryInputsAsync();
        await LoadTestsAsync();
        await LoadLaboratoryHistoryAsync();
        UpdateHomeDashboard();
        SetStatus("Все основные данные обновлены.");
    }

    private void NotificationsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (IsLaboratoryUser())
        {
            ShowSection("История");
            return;
        }

        ShowSection("События");
    }

    private void HomeSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Главная");
    private void ProductsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Продукция");
    private void RecipesSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Рецептуры");
    private void TechnologyCardsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Техкарты");
    private void OrdersSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Заказы");
    private void BatchesSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Партии");
    private void ExtruderSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Экструдер");
    private void EventsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("События");
    private void ReportsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Отчеты");
    private void LabLotsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Партии сырья");
    private void LabSpecificationsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Спецификации");
    private void LabTestsSectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("Испытания");
    private void LabHistorySectionButton_OnClick(object? sender, RoutedEventArgs e) => ShowSection("История");

    private async void LoadCatalogsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadCatalogsAsync);
    }

    private async void OpenProductCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var item = RequireSelectedProduct();
            var detail = await _apiClient.GetProductAsync(item.Id);
            var window = new ProductCardWindow(detail);
            await window.ShowDialog(this);
        });
    }

    private async Task LoadCatalogsAsync()
    {
        EnsureLoggedIn();
        ProductsGrid.ItemsSource = await _apiClient.GetProductsAsync();
        RawMaterialsGrid.ItemsSource = await _apiClient.GetRawMaterialsAsync();
        ProductionLinesGrid.ItemsSource = await _apiClient.GetProductionLinesAsync();
        LaboratoryLotsGrid.ItemsSource = await _apiClient.GetRawMaterialLotsAsync();
        UpdateHomeDashboard();
        SetStatus("Справочники загружены.");
    }

    private async void LoadRecipesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadRecipesAsync);
    }

    private async void OpenRecipeCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var item = RequireSelectedRecipe();
            var detail = await _apiClient.GetRecipeAsync(item.Id);
            var window = new RecipeCardWindow(detail);
            await window.ShowDialog(this);
        });
    }

    private async Task LoadRecipesAsync()
    {
        EnsureLoggedIn();
        RecipesGrid.ItemsSource = await _apiClient.GetRecipesAsync();
        RecipeDetailTextBox.Text = string.Empty;
        UpdateHomeDashboard();
        SetStatus("Рецептуры загружены.");
    }

    private async void RecipesGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RecipesGrid.SelectedItem is not RecipeListItem item)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            var detail = await _apiClient.GetRecipeAsync(item.Id);
            RecipeDetailTextBox.Text = JsonSerializer.Serialize(detail, _prettyJsonOptions);
            RecipeProductIdTextBox.Text = item.ProductId.ToString(CultureInfo.InvariantCulture);
            BatchRecipeIdTextBox.Text = item.Id.ToString(CultureInfo.InvariantCulture);
        });
    }

    private async void CreateRecipeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();

            var request = new CreateRecipeRequest
            {
                ProductId = ParseRequiredInt(RecipeProductIdTextBox.Text, "ProductId"),
                CreatedByUserId = _currentUser!.Id,
                Notes = NullIfWhiteSpace(RecipeNotesTextBox.Text),
                Components = ParseRecipeComponents(RecipeComponentsTextBox.Text)
            };

            await _apiClient.CreateRecipeAsync(request);
            await LoadRecipesAsync();
            SetStatus("Черновик рецептуры создан.");
        });
    }

    private async void ApproveRecipeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var item = RequireSelectedRecipe();
            await _apiClient.ApproveRecipeAsync(item.Id, _currentUser!.Id, RecipeApproveCommentTextBox.Text);
            await LoadRecipesAsync();
            SetStatus("Рецептура утверждена.");
        });
    }

    private async void LoadTechnologyCardsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadTechnologyCardsAsync);
    }

    private async void OpenTechnologyCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var item = RequireSelectedTechnologyCard();
            var detail = await _apiClient.GetTechnologyCardAsync(item.Id);
            var window = new TechnologyCardWindow(detail);
            await window.ShowDialog(this);
        });
    }

    private async Task LoadTechnologyCardsAsync()
    {
        EnsureLoggedIn();
        TechnologyCardsGrid.ItemsSource = await _apiClient.GetTechnologyCardsAsync();
        TechnologyCardDetailTextBox.Text = string.Empty;
        UpdateHomeDashboard();
        SetStatus("Технологические карты загружены.");
    }

    private async void TechnologyCardsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TechnologyCardsGrid.SelectedItem is not TechnologyCardListItem item)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            var detail = await _apiClient.GetTechnologyCardAsync(item.Id);
            TechnologyCardDetailTextBox.Text = JsonSerializer.Serialize(detail, _prettyJsonOptions);
            TechnologyCardProductIdTextBox.Text = item.ProductId.ToString(CultureInfo.InvariantCulture);
            BatchTechnologyCardIdTextBox.Text = item.Id.ToString(CultureInfo.InvariantCulture);
        });
    }

    private async void CreateTechnologyCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();

            var request = BuildTechnologyCardRequest();
            await _apiClient.CreateTechnologyCardAsync(request);
            await LoadTechnologyCardsAsync();
            SetStatus("Черновик технологической карты создан.");
        });
    }

    private async void ApproveTechnologyCardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var item = RequireSelectedTechnologyCard();
            await _apiClient.ApproveTechnologyCardAsync(item.Id, _currentUser!.Id, TechnologyCardApproveCommentTextBox.Text);
            await LoadTechnologyCardsAsync();
            SetStatus("Технологическая карта утверждена.");
        });
    }

    private async void LoadProductionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadProductionAsync);
    }

    private async Task LoadProductionAsync()
    {
        EnsureLoggedIn();
        OrdersGrid.ItemsSource = await _apiClient.GetOrdersAsync();
        BatchesGrid.ItemsSource = await _apiClient.GetBatchesAsync();
        BatchStepsGrid.ItemsSource = null;
        UpdateHomeDashboard();
        SetStatus("Производственные данные загружены.");
    }

    private async void LoadDeviationsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadDeviationsAsync);
    }

    private async Task LoadDeviationsAsync()
    {
        EnsureLoggedIn();
        DeviationsGrid.ItemsSource = await _apiClient.GetDeviationsAsync();
        EventDetailTextBox.Text = "Выберите событие в таблице слева.";
        UpdateHomeDashboard();
        SetStatus("Отклонения загружены.");
    }

    private async void LoadCriticalDeviationsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var items = await _apiClient.GetDeviationsAsync();
            DeviationsGrid.ItemsSource = items.Where(x => x.Severity >= 3).ToList();
            EventDetailTextBox.Text = "Показаны только критичные события.";
            SetStatus("Критичные события загружены.");
        });
    }

    private async void ApplyEventsSearchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var search = (SearchTextBox.Text ?? string.Empty).Trim();
            var items = await _apiClient.GetDeviationsAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(x =>
                        ContainsText(x.BatchNumber, search) ||
                        ContainsText(x.Title, search) ||
                        ContainsText(x.ParameterName, search) ||
                        ContainsText(x.StepTitle, search) ||
                        ContainsText(x.Details, search))
                    .ToList();
            }

            DeviationsGrid.ItemsSource = items;
            EventDetailTextBox.Text = $"Найдено событий: {items.Count}";
            SetStatus("Поиск по событиям выполнен.");
        });
    }

    private void DeviationsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DeviationsGrid.SelectedItem is not DeviationItem item)
        {
            return;
        }

        EventDetailTextBox.Text = FormatDeviationDetail(item);
    }

    private async void LoadReportsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadReportsAsync);
    }

    private async Task LoadReportsAsync()
    {
        EnsureLoggedIn();

        var orders = await _apiClient.GetOrdersAsync();
        var batches = await _apiClient.GetBatchesAsync();
        var deviations = await _apiClient.GetDeviationsAsync();
        var tests = await _apiClient.GetTestsAsync();
        var decisions = await _apiClient.GetDecisionsAsync();

        var text = new StringBuilder();
        text.AppendLine("Краткий отчет по системе");
        text.AppendLine();
        text.AppendLine($"Заказов всего: {orders.Count}");
        text.AppendLine($"Партий всего: {batches.Count}");
        text.AppendLine($"Партий завершено: {batches.Count(x => x.CompletedAt != null || x.Status == 6)}");
        text.AppendLine($"Отклонений всего: {deviations.Count}");
        text.AppendLine($"Критичных отклонений: {deviations.Count(x => x.Severity >= 3)}");
        text.AppendLine($"Испытаний всего: {tests.Count}");
        text.AppendLine($"Испытаний завершено: {tests.Count(x => x.CompletedAt != null || x.Status == 3)}");
        text.AppendLine($"Решений по качеству всего: {decisions.Count}");
        text.AppendLine($"Блокирующих решений: {decisions.Count(x => x.DecisionStatus == 2)}");

        ReportsTextBox.Text = text.ToString();
        ReportsGrid.ItemsSource = new List<ReportLineItem>
        {
            new() { Показатель = "Заказов всего", Значение = orders.Count.ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Партий всего", Значение = batches.Count.ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Партий завершено", Значение = batches.Count(x => x.CompletedAt != null || x.Status == 6).ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Событий и отклонений", Значение = deviations.Count.ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Критичных событий", Значение = deviations.Count(x => x.Severity >= 3).ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Испытаний всего", Значение = tests.Count.ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Испытаний завершено", Значение = tests.Count(x => x.CompletedAt != null || x.Status == 3).ToString(CultureInfo.InvariantCulture) },
            new() { Показатель = "Решений по качеству", Значение = decisions.Count.ToString(CultureInfo.InvariantCulture) }
        };
        SetStatus("Отчет собран.");
    }

    private async void ShowBatchReportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var batches = await _apiClient.GetBatchesAsync();
            ReportsGrid.ItemsSource = batches;

            var text = new StringBuilder();
            text.AppendLine("Отчет по партиям");
            text.AppendLine();
            text.AppendLine($"Партий всего: {batches.Count}");
            text.AppendLine($"Новых: {batches.Count(x => x.Status == 1)}");
            text.AppendLine($"В работе: {batches.Count(x => x.Status == 2)}");
            text.AppendLine($"Завершенных: {batches.Count(x => x.Status == 6 || x.CompletedAt != null)}");
            ReportsTextBox.Text = text.ToString();
            SetStatus("Отчет по партиям собран.");
        });
    }

    private async void ShowDeviationReportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var deviations = await _apiClient.GetDeviationsAsync();
            ReportsGrid.ItemsSource = deviations;

            var text = new StringBuilder();
            text.AppendLine("Отчет по событиям и отклонениям");
            text.AppendLine();
            text.AppendLine($"Событий всего: {deviations.Count}");
            text.AppendLine($"Средних: {deviations.Count(x => x.Severity == 2)}");
            text.AppendLine($"Критичных: {deviations.Count(x => x.Severity >= 3)}");
            ReportsTextBox.Text = text.ToString();
            SetStatus("Отчет по событиям собран.");
        });
    }

    private async void ShowLaboratoryReportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var tests = await _apiClient.GetTestsAsync();
            ReportsGrid.ItemsSource = tests;

            var text = new StringBuilder();
            text.AppendLine("Отчет по лаборатории");
            text.AppendLine();
            text.AppendLine($"Испытаний всего: {tests.Count}");
            text.AppendLine($"Новых: {tests.Count(x => x.Status == 1)}");
            text.AppendLine($"В работе: {tests.Count(x => x.Status == 2)}");
            text.AppendLine($"Завершенных: {tests.Count(x => x.Status == 3 || x.CompletedAt != null)}");
            ReportsTextBox.Text = text.ToString();
            SetStatus("Отчет по лаборатории собран.");
        });
    }

    private async void CreateOrderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();

            var request = new CreateProductionOrderRequest
            {
                ProductId = ParseRequiredInt(OrderProductIdTextBox.Text, "ProductId"),
                ProductionLineId = ParseRequiredInt(OrderLineIdTextBox.Text, "ProductionLineId"),
                PlannedQuantity = ParseRequiredDecimal(OrderQuantityTextBox.Text, "PlannedQuantity"),
                PlannedStartAt = ParseOptionalDateTime(OrderPlannedStartTextBox.Text),
                CreatedByUserId = _currentUser!.Id
            };

            await _apiClient.CreateOrderAsync(request);
            await LoadProductionAsync();
            SetStatus("Производственный заказ создан.");
        });
    }

    private async void CreateBatchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();

            var request = new CreateProductionBatchRequest
            {
                ProductionOrderId = ParseOptionalInt(BatchOrderIdTextBox.Text),
                ProductId = ParseRequiredInt(BatchProductIdTextBox.Text, "ProductId"),
                ProductionLineId = ParseRequiredInt(BatchLineIdTextBox.Text, "ProductionLineId"),
                RecipeVersionId = ParseRequiredInt(BatchRecipeIdTextBox.Text, "RecipeVersionId"),
                TechnologyCardId = ParseRequiredInt(BatchTechnologyCardIdTextBox.Text, "TechnologyCardId"),
                ExtruderProgramId = ParseOptionalInt(BatchExtruderProgramIdTextBox.Text),
                PlannedQuantity = ParseRequiredDecimal(BatchQuantityTextBox.Text, "PlannedQuantity"),
                Consumptions = ParseConsumptions(BatchConsumptionsTextBox.Text)
            };

            await _apiClient.CreateBatchAsync(request);
            await LoadProductionAsync();
            SetStatus("Производственная партия создана.");
        });
    }

    private async void BatchesGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (BatchesGrid.SelectedItem is ProductionBatchItem batch)
        {
            LabSubjectTypeTextBox.Text = "2";
            LabProductionBatchIdTextBox.Text = batch.Id.ToString(CultureInfo.InvariantCulture);
            BatchProductIdTextBox.Text = batch.ProductId.ToString(CultureInfo.InvariantCulture);
            BatchLineIdTextBox.Text = batch.ProductionLineId.ToString(CultureInfo.InvariantCulture);
            BatchRecipeIdTextBox.Text = batch.RecipeVersionId.ToString(CultureInfo.InvariantCulture);
            BatchTechnologyCardIdTextBox.Text = batch.TechnologyCardId.ToString(CultureInfo.InvariantCulture);
        }

        await LoadSelectedBatchStepsAsync();
    }

    private async void LoadBatchStepsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadSelectedBatchStepsAsync);
    }

    private async Task LoadSelectedBatchStepsAsync()
    {
        EnsureLoggedIn();
        var batch = RequireSelectedBatch();
        BatchStepsGrid.ItemsSource = await _apiClient.GetBatchStepsAsync(batch.Id);
        SetStatus($"Шаги партии {batch.BatchNumber} загружены.");
    }

    private async void StartBatchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var batch = RequireSelectedBatch();
            await _apiClient.StartBatchAsync(batch.Id);
            await LoadProductionAsync();
            SetStatus("Партия переведена в работу.");
        });
    }

    private async void CompleteBatchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var batch = RequireSelectedBatch();
            await _apiClient.CompleteBatchAsync(batch.Id);
            await LoadProductionAsync();
            SetStatus("Партия завершена.");
        });
    }

    private async void StartStepButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var step = RequireSelectedStepRun();
            await _apiClient.StartStepAsync(step.Id, new StartStepRunRequest
            {
                StartedByUserId = _currentUser!.Id,
                Comment = NullIfWhiteSpace(StepCommentTextBox.Text)
            });
            await LoadSelectedBatchStepsAsync();
            SetStatus("Шаг партии начат.");
        });
    }

    private async void CompleteStepButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var step = RequireSelectedStepRun();
            await _apiClient.CompleteStepAsync(step.Id, new CompleteStepRunRequest
            {
                CompletedByUserId = _currentUser!.Id,
                Comment = NullIfWhiteSpace(StepCommentTextBox.Text)
            });
            await LoadSelectedBatchStepsAsync();
            SetStatus("Шаг партии завершен.");
        });
    }

    private async void AddMeasurementButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var step = RequireSelectedStepRun();
            var request = new AddMeasurementRequest
            {
                TechnologyStepParameterId = ParseRequiredInt(MeasurementParameterIdTextBox.Text, "TechnologyStepParameterId"),
                ActualNumericValue = ParseOptionalDecimal(MeasurementNumericTextBox.Text),
                ActualTextValue = NullIfWhiteSpace(MeasurementTextTextBox.Text),
                ActualBooleanValue = ParseOptionalBool(MeasurementBooleanTextBox.Text),
                Comment = NullIfWhiteSpace(MeasurementCommentTextBox.Text)
            };

            var response = await _apiClient.AddMeasurementAsync(step.Id, request);
            var resultText = response.IsWithinTolerance ? "в норме" : "не в норме";
            SetStatus($"Фактическое значение записано: {resultText}.");
        });
    }

    private async void AddDeviationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var batch = RequireSelectedBatch();
            var step = BatchStepsGrid.SelectedItem as BatchStepRunItem;

            await _apiClient.CreateDeviationAsync(new CreateDeviationRequest
            {
                ProductionBatchId = batch.Id,
                BatchTechnologyStepRunId = step?.Id,
                Title = DeviationTitleTextBox.Text ?? string.Empty,
                ParameterName = NullIfWhiteSpace(DeviationParameterTextBox.Text),
                PlannedValue = NullIfWhiteSpace(DeviationPlannedValueTextBox.Text),
                ActualValue = NullIfWhiteSpace(DeviationActualValueTextBox.Text),
                Severity = ParseRequiredInt(DeviationSeverityTextBox.Text, "Severity"),
                Details = NullIfWhiteSpace(DeviationDetailsTextBox.Text),
                ReportedByUserId = _currentUser!.Id
            });

            await LoadDeviationsAsync();
            SetStatus("Отклонение зарегистрировано.");
        });
    }

    private async void LoadLaboratoryInputsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadLaboratoryInputsAsync);
    }

    private async Task LoadLaboratoryInputsAsync()
    {
        EnsureLoggedIn();
        LaboratoryLotsGrid.ItemsSource = await _apiClient.GetRawMaterialLotsAsync();
        SpecificationsGrid.ItemsSource = await _apiClient.GetSpecificationsAsync();
        UpdateHomeDashboard();
        SetStatus("Лабораторные входные данные загружены.");
    }

    private async void LoadTestsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadTestsAsync);
    }

    private async Task LoadTestsAsync()
    {
        EnsureLoggedIn();
        TestsGrid.ItemsSource = await _apiClient.GetTestsAsync();
        TestDetailTextBox.Text = string.Empty;
        UpdateHomeDashboard();
        SetStatus("Испытания загружены.");
    }

    private async void LoadLaboratoryHistoryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadLaboratoryHistoryAsync);
    }

    private async Task LoadLaboratoryHistoryAsync()
    {
        EnsureLoggedIn();
        var tests = await _apiClient.GetTestsAsync();
        var decisions = await _apiClient.GetDecisionsAsync();

        CompletedTestsGrid.ItemsSource = tests
            .Where(x => x.CompletedAt != null || x.Status == 3)
            .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
            .ToList();

        DecisionsGrid.ItemsSource = decisions;
        UpdateHomeDashboard();
        SetStatus("История лаборатории загружена.");
    }

    private async void TestsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TestsGrid.SelectedItem is not LaboratoryTestItem item)
        {
            return;
        }

        await ExecuteAsync(async () =>
        {
            var detail = await _apiClient.GetTestAsync(item.Id);
            TestDetailTextBox.Text = FormatTestDetail(detail);
        });
    }

    private async void CreateTestButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();

            var request = new CreateLaboratoryTestRequest
            {
                SubjectType = ParseRequiredInt(LabSubjectTypeTextBox.Text, "SubjectType"),
                RawMaterialLotId = ParseOptionalInt(LabRawMaterialLotIdTextBox.Text),
                ProductionBatchId = ParseOptionalInt(LabProductionBatchIdTextBox.Text),
                QualitySpecificationId = ParseRequiredInt(LabQualitySpecificationIdTextBox.Text, "QualitySpecificationId"),
                TestKind = LabTestKindTextBox.Text ?? string.Empty,
                Priority = ParseRequiredInt(LabPriorityTextBox.Text, "Priority"),
                Comment = NullIfWhiteSpace(LabCreateCommentTextBox.Text),
                TesterUserId = _currentUser!.Id
            };

            await _apiClient.CreateTestAsync(request);
            await LoadTestsAsync();
            SetStatus("Лабораторное испытание создано.");
        });
    }

    private async void StartTestButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var test = RequireSelectedTest();
            await _apiClient.StartTestAsync(test.Id, _currentUser!.Id);
            await LoadTestsAsync();
            SetStatus("Испытание начато.");
        });
    }

    private async void SaveResultsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var test = RequireSelectedTest();
            var request = new SaveLaboratoryResultsRequest
            {
                Items = ParseLaboratoryResults(LabResultsTextBox.Text)
            };

            await _apiClient.SaveResultsAsync(test.Id, request);
            var detail = await _apiClient.GetTestAsync(test.Id);
            TestDetailTextBox.Text = FormatTestDetail(detail);
            SetStatus("Результаты испытания сохранены.");
        });
    }

    private async void CompleteTestButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var test = RequireSelectedTest();
            await _apiClient.CompleteTestAsync(test.Id, new CompleteLaboratoryTestRequest
            {
                TesterUserId = _currentUser!.Id,
                ResultSummary = NullIfWhiteSpace(LabResultSummaryTextBox.Text)
            });
            await LoadTestsAsync();
            SetStatus("Испытание завершено.");
        });
    }

    private async void CreateDecisionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            EnsureLoggedIn();
            var test = RequireSelectedTest();
            await _apiClient.CreateDecisionAsync(new CreateQualityDecisionRequest
            {
                LaboratoryTestId = test.Id,
                DecisionStatus = ParseRequiredInt(LabDecisionStatusTextBox.Text, "DecisionStatus"),
                Comment = LabDecisionCommentTextBox.Text ?? string.Empty,
                BlockReason = NullIfWhiteSpace(LabDecisionBlockReasonTextBox.Text),
                DecidedByUserId = _currentUser!.Id
            });
            await LoadLaboratoryHistoryAsync();
            SetStatus("Решение по качеству принято.");
        });
    }

    private CreateTechnologyCardRequest BuildTechnologyCardRequest()
    {
        EnsureLoggedIn();

        var steps = ParseTechnologySteps(TechnologyCardStepsTextBox.Text);
        var parameters = ParseTechnologyParameters(TechnologyCardParametersTextBox.Text);
        var stepMap = steps.ToDictionary(x => x.StepOrder);

        foreach (var parameter in parameters)
        {
            if (stepMap.TryGetValue(parameter.StepOrder, out var step))
            {
                step.Parameters.Add(parameter.Parameter);
            }
        }

        return new CreateTechnologyCardRequest
        {
            ProductId = ParseRequiredInt(TechnologyCardProductIdTextBox.Text, "ProductId"),
            CreatedByUserId = _currentUser!.Id,
            Title = TechnologyCardTitleTextBox.Text ?? string.Empty,
            Description = NullIfWhiteSpace(TechnologyCardDescriptionTextBox.Text),
            Steps = steps
        };
    }

    private List<CreateRecipeComponentRequest> ParseRecipeComponents(string? input)
    {
        var lines = SplitLines(input);
        var result = new List<CreateRecipeComponentRequest>();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 4)
            {
                throw new InvalidOperationException("Компоненты рецептуры должны быть в формате rawMaterialId;percentage;loadOrder;allowedDeviation.");
            }

            result.Add(new CreateRecipeComponentRequest
            {
                RawMaterialId = ParseRequiredInt(parts[0], "RawMaterialId"),
                Percentage = ParseRequiredDecimal(parts[1], "Percentage"),
                LoadOrder = ParseRequiredInt(parts[2], "LoadOrder"),
                AllowedDeviationPercent = ParseRequiredDecimal(parts[3], "AllowedDeviationPercent")
            });
        }

        return result;
    }

    private List<CreateTechnologyStepRequest> ParseTechnologySteps(string? input)
    {
        var lines = SplitLines(input);
        var result = new List<CreateTechnologyStepRequest>();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 6)
            {
                throw new InvalidOperationException("Шаги техкарты должны быть в формате stepOrder;stepType;title;isRequired;durationMinutes;instruction.");
            }

            result.Add(new CreateTechnologyStepRequest
            {
                StepOrder = ParseRequiredInt(parts[0], "StepOrder"),
                StepType = ParseRequiredInt(parts[1], "StepType"),
                Title = parts[2].Trim(),
                IsRequired = ParseRequiredBool(parts[3], "IsRequired"),
                ExpectedDurationMinutes = ParseOptionalInt(parts[4]),
                Instruction = NullIfWhiteSpace(parts[5]),
                Parameters = new List<CreateTechnologyStepParameterRequest>()
            });
        }

        return result;
    }

    private List<(int StepOrder, CreateTechnologyStepParameterRequest Parameter)> ParseTechnologyParameters(string? input)
    {
        var lines = SplitLines(input);
        var result = new List<(int StepOrder, CreateTechnologyStepParameterRequest Parameter)>();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 10)
            {
                throw new InvalidOperationException("Параметры техкарты должны быть в формате stepOrder;name;valueType;unit;targetNumeric;min;max;targetText;targetBool;isRequired;comment.");
            }

            result.Add((ParseRequiredInt(parts[0], "StepOrder"), new CreateTechnologyStepParameterRequest
            {
                Name = parts[1].Trim(),
                ValueType = ParseRequiredInt(parts[2], "ValueType"),
                Unit = NullIfWhiteSpace(parts[3]),
                TargetNumericValue = ParseOptionalDecimal(parts[4]),
                MinNumericValue = ParseOptionalDecimal(parts[5]),
                MaxNumericValue = ParseOptionalDecimal(parts[6]),
                TargetTextValue = NullIfWhiteSpace(parts[7]),
                TargetBooleanValue = ParseOptionalBool(parts[8]),
                IsRequired = ParseRequiredBool(parts[9], "IsRequired"),
                Comment = parts.Length > 10 ? NullIfWhiteSpace(parts[10]) : null
            }));
        }

        return result;
    }

    private List<BatchConsumptionRequest> ParseConsumptions(string? input)
    {
        var lines = SplitLines(input);
        var result = new List<BatchConsumptionRequest>();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 2)
            {
                throw new InvalidOperationException("Расход сырья должен быть в формате rawMaterialLotId;quantityUsed.");
            }

            result.Add(new BatchConsumptionRequest
            {
                RawMaterialLotId = ParseRequiredInt(parts[0], "RawMaterialLotId"),
                QuantityUsed = ParseRequiredDecimal(parts[1], "QuantityUsed")
            });
        }

        return result;
    }

    private List<SaveLaboratoryResultItem> ParseLaboratoryResults(string? input)
    {
        var lines = SplitLines(input);
        var result = new List<SaveLaboratoryResultItem>();

        foreach (var line in lines)
        {
            var parts = line.Split(';');
            if (parts.Length < 5)
            {
                throw new InvalidOperationException("Результаты должны быть в формате parameterResultId;numeric;text;bool;comment.");
            }

            result.Add(new SaveLaboratoryResultItem
            {
                ParameterResultId = ParseRequiredInt(parts[0], "ParameterResultId"),
                ActualNumericValue = ParseOptionalDecimal(parts[1]),
                ActualTextValue = NullIfWhiteSpace(parts[2]),
                ActualBooleanValue = ParseOptionalBool(parts[3]),
                Comment = NullIfWhiteSpace(parts[4])
            });
        }

        return result;
    }

    private static List<string> SplitLines(string? input)
    {
        return (input ?? string.Empty)
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static int ParseRequiredInt(string? value, string fieldName)
    {
        if (!int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new InvalidOperationException($"Поле {fieldName} должно быть целым числом.");
        }

        return result;
    }

    private static int? ParseOptionalInt(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return ParseRequiredInt(text, "OptionalInt");
    }

    private static decimal ParseRequiredDecimal(string? value, string fieldName)
    {
        var normalized = (value ?? string.Empty).Trim().Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            throw new InvalidOperationException($"Поле {fieldName} должно быть числом.");
        }

        return result;
    }

    private static decimal? ParseOptionalDecimal(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return ParseRequiredDecimal(text, "OptionalDecimal");
    }

    private static DateTime? ParseOptionalDateTime(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result))
        {
            throw new InvalidOperationException("Дата должна быть в формате ISO, например 2026-04-08T10:00:00Z.");
        }

        return result;
    }

    private static bool ParseRequiredBool(string? value, string fieldName)
    {
        var result = ParseOptionalBool(value);
        if (result is null)
        {
            throw new InvalidOperationException($"Поле {fieldName} должно быть true или false.");
        }

        return result.Value;
    }

    private static bool? ParseOptionalBool(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (bool.TryParse(text, out var result))
        {
            return result;
        }

        return text switch
        {
            "1" => true,
            "0" => false,
            _ => null
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void ApplyRoleLayout()
    {
        var isLab = IsLaboratoryUser();
        TechnologistMenuPanel.IsVisible = !isLab;
        LaboratoryMenuPanel.IsVisible = isLab;
        HomeTechnologistActionsPanel.IsVisible = !isLab;
        HomeLaboratoryActionsPanel.IsVisible = isLab;
        UpdateHomeDashboard();
    }

    private bool IsLaboratoryUser()
    {
        return LaboratoryClient;
    }

    private void ShowSection(string sectionName)
    {
        HomeSection.IsVisible = false;
        ProductsSection.IsVisible = false;
        RecipesSection.IsVisible = false;
        TechnologyCardsSection.IsVisible = false;
        OrdersSection.IsVisible = false;
        BatchesSection.IsVisible = false;
        ExtruderSection.IsVisible = false;
        EventsSection.IsVisible = false;
        ReportsSection.IsVisible = false;
        LabLotsSection.IsVisible = false;
        LabSpecificationsSection.IsVisible = false;
        LabTestsSection.IsVisible = false;
        LabHistorySection.IsVisible = false;

        switch (sectionName)
        {
            case "Главная":
                HomeSection.IsVisible = true;
                break;
            case "Продукция":
                ProductsSection.IsVisible = true;
                break;
            case "Рецептуры":
                RecipesSection.IsVisible = true;
                break;
            case "Техкарты":
                TechnologyCardsSection.IsVisible = true;
                break;
            case "Заказы":
                OrdersSection.IsVisible = true;
                break;
            case "Партии":
                BatchesSection.IsVisible = true;
                break;
            case "Экструдер":
                ExtruderSection.IsVisible = true;
                break;
            case "События":
                EventsSection.IsVisible = true;
                break;
            case "Отчеты":
                ReportsSection.IsVisible = true;
                break;
            case "Партии сырья":
                LabLotsSection.IsVisible = true;
                break;
            case "Спецификации":
                LabSpecificationsSection.IsVisible = true;
                break;
            case "Испытания":
                LabTestsSection.IsVisible = true;
                break;
            case "История":
                LabHistorySection.IsVisible = true;
                break;
            default:
                HomeSection.IsVisible = true;
                sectionName = "Главная";
                break;
        }

        SectionTitleTextBlock.Text = sectionName;
    }

    private static string GetRoleTitle(string? roleCode)
    {
        return roleCode?.ToUpperInvariant() switch
        {
            "TECHNOLOGIST" => "Технолог",
            "LAB_TECHNICIAN" => "Лаборатория",
            "OPERATOR" => "Аппаратчик",
            "ADMINISTRATOR" => "Администратор",
            _ => "Пользователь"
        };
    }

    private void UpdateHomeDashboard()
    {
        var text = new StringBuilder();
        text.AppendLine("Краткая сводка");
        text.AppendLine();

        if (_currentUser is null)
        {
            text.AppendLine("Пользователь: нет входа");
            text.AppendLine("Чтобы начать работу:");
            text.AppendLine("1. Укажите адрес API");
            text.AppendLine("2. Введите логин и пароль");
            text.AppendLine("3. Нажмите «Войти»");
            HomeSummaryTextBox.Text = text.ToString();
            return;
        }

        text.AppendLine($"Пользователь: {_currentUser.FullName}");
        text.AppendLine($"Роль: {GetRoleTitle(_currentUser.RoleCode)}");
        text.AppendLine($"Подразделение: {_currentUser.DepartmentName}");
        text.AppendLine();

        if (IsLaboratoryUser())
        {
            var lots = GetItems<RawMaterialLotListItem>(LaboratoryLotsGrid.ItemsSource);
            var specifications = GetItems<QualitySpecificationItem>(SpecificationsGrid.ItemsSource);
            var tests = GetItems<LaboratoryTestItem>(TestsGrid.ItemsSource);
            var decisions = GetItems<QualityDecisionItem>(DecisionsGrid.ItemsSource);

            text.AppendLine($"Партий сырья: {lots.Count}");
            text.AppendLine($"Спецификаций качества: {specifications.Count}");
            text.AppendLine($"Испытаний всего: {tests.Count}");
            text.AppendLine($"Испытаний завершено: {tests.Count(x => x.CompletedAt != null || x.Status == 3)}");
            text.AppendLine($"Решений по качеству: {decisions.Count}");
            text.AppendLine($"Текущих блокировок: {decisions.Count(x => x.IsCurrent && x.DecisionStatus == 2)}");
        }
        else
        {
            var products = GetItems<ProductListItem>(ProductsGrid.ItemsSource);
            var rawMaterials = GetItems<RawMaterialListItem>(RawMaterialsGrid.ItemsSource);
            var recipes = GetItems<RecipeListItem>(RecipesGrid.ItemsSource);
            var cards = GetItems<TechnologyCardListItem>(TechnologyCardsGrid.ItemsSource);
            var orders = GetItems<ProductionOrderItem>(OrdersGrid.ItemsSource);
            var batches = GetItems<ProductionBatchItem>(BatchesGrid.ItemsSource);
            var deviations = GetItems<DeviationItem>(DeviationsGrid.ItemsSource);

            text.AppendLine($"Продукции: {products.Count}");
            text.AppendLine($"Видов сырья: {rawMaterials.Count}");
            text.AppendLine($"Рецептур: {recipes.Count}");
            text.AppendLine($"Утвержденных рецептур: {recipes.Count(x => x.Status == 3 && x.IsActive)}");
            text.AppendLine($"Техкарт: {cards.Count}");
            text.AppendLine($"Утвержденных техкарт: {cards.Count(x => x.Status == 3 && x.IsActive)}");
            text.AppendLine($"Заказов: {orders.Count}");
            text.AppendLine($"Партий: {batches.Count}");
            text.AppendLine($"Завершенных партий: {batches.Count(x => x.CompletedAt != null || x.Status == 6)}");
            text.AppendLine($"Событий и отклонений: {deviations.Count}");
        }

        HomeSummaryTextBox.Text = text.ToString();
    }

    private static List<T> GetItems<T>(System.Collections.IEnumerable? source)
    {
        if (source is null)
        {
            return new List<T>();
        }

        return source.Cast<object>().OfType<T>().ToList();
    }

    private void ProductsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ProductsGrid.SelectedItem is not ProductListItem item)
        {
            return;
        }

        var value = item.Id.ToString(CultureInfo.InvariantCulture);
        RecipeProductIdTextBox.Text = value;
        TechnologyCardProductIdTextBox.Text = value;
        OrderProductIdTextBox.Text = value;
        BatchProductIdTextBox.Text = value;
    }

    private void ProductionLinesGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ProductionLinesGrid.SelectedItem is not ProductionLineItem item)
        {
            return;
        }

        var value = item.Id.ToString(CultureInfo.InvariantCulture);
        OrderLineIdTextBox.Text = value;
        BatchLineIdTextBox.Text = value;
    }

    private void OrdersGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (OrdersGrid.SelectedItem is not ProductionOrderItem item)
        {
            return;
        }

        BatchOrderIdTextBox.Text = item.Id.ToString(CultureInfo.InvariantCulture);
        BatchProductIdTextBox.Text = item.ProductId.ToString(CultureInfo.InvariantCulture);
        BatchLineIdTextBox.Text = item.ProductionLineId.ToString(CultureInfo.InvariantCulture);
        BatchQuantityTextBox.Text = item.PlannedQuantity.ToString(CultureInfo.InvariantCulture);
    }

    private void LaboratoryLotsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LaboratoryLotsGrid.SelectedItem is not RawMaterialLotListItem item)
        {
            return;
        }

        LabSubjectTypeTextBox.Text = "1";
        LabRawMaterialLotIdTextBox.Text = item.Id.ToString(CultureInfo.InvariantCulture);
        LabProductionBatchIdTextBox.Text = string.Empty;
    }

    private void SpecificationsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SpecificationsGrid.SelectedItem is not QualitySpecificationItem item)
        {
            return;
        }

        LabQualitySpecificationIdTextBox.Text = item.Id.ToString(CultureInfo.InvariantCulture);
        LabSubjectTypeTextBox.Text = item.SubjectType.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatTestDetail(LaboratoryTestDetail detail)
    {
        var text = new StringBuilder();
        text.AppendLine($"Испытание: {detail.Test.TestNumber}");
        text.AppendLine($"Спецификация: {detail.Test.QualitySpecificationName}");
        text.AppendLine($"Статус: {detail.Test.Status}");
        text.AppendLine($"Вид: {detail.Test.TestKind}");
        text.AppendLine($"Итог: {detail.Test.ResultSummary ?? "-"}");
        text.AppendLine();
        text.AppendLine("Результаты:");

        foreach (var item in detail.Results)
        {
            text.AppendLine($"{item.SortOrder}. {item.ParameterName}");
            text.AppendLine($"   Факт: {GetActualValueText(item)}");
            text.AppendLine($"   Проверка: {GetRangeText(item.IsWithinRange)}");
            if (!string.IsNullOrWhiteSpace(item.Comment))
            {
                text.AppendLine($"   Комментарий: {item.Comment}");
            }
        }

        return text.ToString();
    }

    private static string GetActualValueText(LaboratoryTestParameterResultItem item)
    {
        if (item.ActualNumericValue is not null)
        {
            return item.ActualNumericValue.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(item.ActualTextValue))
        {
            return item.ActualTextValue;
        }

        if (item.ActualBooleanValue is not null)
        {
            return item.ActualBooleanValue.Value ? "true" : "false";
        }

        return "нет данных";
    }

    private static string GetRangeText(bool? value)
    {
        return value switch
        {
            true => "В норме",
            false => "Не в норме",
            null => "Нет проверки"
        };
    }

    private static bool ContainsText(string? source, string search)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatDeviationDetail(DeviationItem item)
    {
        var text = new StringBuilder();
        text.AppendLine($"Событие: {item.Title}");
        text.AppendLine($"Партия: {item.BatchNumber}");
        text.AppendLine($"Шаг: {item.StepOrder?.ToString(CultureInfo.InvariantCulture) ?? "-"}");
        text.AppendLine($"Название шага: {item.StepTitle ?? "-"}");
        text.AppendLine($"Параметр: {item.ParameterName ?? "-"}");
        text.AppendLine($"План: {item.PlannedValue ?? "-"}");
        text.AppendLine($"Факт: {item.ActualValue ?? "-"}");
        text.AppendLine($"Критичность: {item.Severity}");
        text.AppendLine($"Дата: {item.CreatedAt:dd.MM.yyyy HH:mm}");
        text.AppendLine($"Сообщил: {item.ReportedByName ?? "-"}");
        text.AppendLine();
        text.AppendLine($"Описание: {item.Details ?? "-"}");
        return text.ToString();
    }

    private RecipeListItem RequireSelectedRecipe()
    {
        return RecipesGrid.SelectedItem as RecipeListItem
               ?? throw new InvalidOperationException("Выберите рецептуру в таблице.");
    }

    private ProductListItem RequireSelectedProduct()
    {
        return ProductsGrid.SelectedItem as ProductListItem
               ?? throw new InvalidOperationException("Выберите продукт в таблице.");
    }

    private TechnologyCardListItem RequireSelectedTechnologyCard()
    {
        return TechnologyCardsGrid.SelectedItem as TechnologyCardListItem
               ?? throw new InvalidOperationException("Выберите технологическую карту в таблице.");
    }

    private ProductionBatchItem RequireSelectedBatch()
    {
        return BatchesGrid.SelectedItem as ProductionBatchItem
               ?? throw new InvalidOperationException("Выберите производственную партию в таблице.");
    }

    private BatchStepRunItem RequireSelectedStepRun()
    {
        return BatchStepsGrid.SelectedItem as BatchStepRunItem
               ?? throw new InvalidOperationException("Выберите шаг партии в таблице.");
    }

    private LaboratoryTestItem RequireSelectedTest()
    {
        return TestsGrid.SelectedItem as LaboratoryTestItem
               ?? throw new InvalidOperationException("Выберите лабораторное испытание в таблице.");
    }

    private void EnsureLoggedIn()
    {
        if (_currentUser is null)
        {
            throw new InvalidOperationException("Сначала выполните вход в систему.");
        }
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
        }
    }
}
