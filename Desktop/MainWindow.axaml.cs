using System.Globalization;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PlantProduction.Desktop;

public partial class MainWindow : Window
{
    private readonly DesktopApiClient _apiClient = new();
    private readonly JsonSerializerOptions _prettyJsonOptions = new() { WriteIndented = true };
    private LoginResponse? _currentUser;

    public MainWindow()
    {
        InitializeComponent();
        _apiClient.SetBaseUrl(ApiUrlTextBox.Text ?? "http://localhost:5114");
    }

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            _apiClient.SetBaseUrl(ApiUrlTextBox.Text ?? "http://localhost:5114");
            _currentUser = await _apiClient.LoginAsync(LoginTextBox.Text ?? string.Empty, PasswordTextBox.Text ?? string.Empty);
            CurrentUserTextBlock.Text = $"{_currentUser.FullName} | {_currentUser.RoleName} | {_currentUser.DepartmentName}";
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
        await LoadLaboratoryInputsAsync();
        await LoadTestsAsync();
        SetStatus("Все основные данные обновлены.");
    }

    private async void LoadCatalogsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadCatalogsAsync);
    }

    private async Task LoadCatalogsAsync()
    {
        EnsureLoggedIn();
        ProductsGrid.ItemsSource = await _apiClient.GetProductsAsync();
        RawMaterialsGrid.ItemsSource = await _apiClient.GetRawMaterialsAsync();
        ProductionLinesGrid.ItemsSource = await _apiClient.GetProductionLinesAsync();
        LaboratoryLotsGrid.ItemsSource = await _apiClient.GetRawMaterialLotsAsync();
        SetStatus("Справочники загружены.");
    }

    private async void LoadRecipesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadRecipesAsync);
    }

    private async Task LoadRecipesAsync()
    {
        EnsureLoggedIn();
        RecipesGrid.ItemsSource = await _apiClient.GetRecipesAsync();
        RecipeDetailTextBox.Text = string.Empty;
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

    private async Task LoadTechnologyCardsAsync()
    {
        EnsureLoggedIn();
        TechnologyCardsGrid.ItemsSource = await _apiClient.GetTechnologyCardsAsync();
        TechnologyCardDetailTextBox.Text = string.Empty;
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
        SetStatus("Производственные данные загружены.");
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

            await _apiClient.AddMeasurementAsync(step.Id, request);
            SetStatus("Фактическое значение записано.");
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
        SetStatus("Испытания загружены.");
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
            TestDetailTextBox.Text = JsonSerializer.Serialize(detail, _prettyJsonOptions);
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
            TestDetailTextBox.Text = JsonSerializer.Serialize(detail, _prettyJsonOptions);
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

    private RecipeListItem RequireSelectedRecipe()
    {
        return RecipesGrid.SelectedItem as RecipeListItem
               ?? throw new InvalidOperationException("Выберите рецептуру в таблице.");
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
