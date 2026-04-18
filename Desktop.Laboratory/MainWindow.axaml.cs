using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PlantProduction.Desktop.Laboratory;

public partial class MainWindow : Window
{
    private readonly DesktopApiClient _apiClient;
    private readonly LoginResponse _currentUser;
    private List<RawMaterialLotListItem> _allLots = new();
    private List<QualitySpecificationItem> _allSpecifications = new();
    private List<LaboratoryTestItem> _allTests = new();
    private List<QualityDecisionItem> _allDecisions = new();

    public MainWindow() : this(
        new DesktopApiClient(),
        new LoginResponse
        {
            FullName = "Лаборант",
            DepartmentName = "Лаборатория"
        })
    {
    }

    public MainWindow(DesktopApiClient apiClient, LoginResponse currentUser)
    {
        InitializeComponent();
        _apiClient = apiClient;
        _currentUser = currentUser;
        HeaderTextBlock.Text = $"Лаборант: {_currentUser.FullName} | {_currentUser.DepartmentName}";
        Opened += MainWindow_Opened;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        if (_currentUser.Id <= 0)
        {
            SetStatus(string.Empty);
            return;
        }

        await RefreshAllAsync();
    }

    private async void RefreshAllButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(RefreshAllAsync);
    }

    private async Task RefreshAllAsync()
    {
        await LoadLotsAsync();
        await LoadSpecificationsAsync();
        await LoadTestsAsync();
        await LoadDecisionsAsync();
        HistoryTextBox.Text = BuildHistoryText();
        SetStatus("Данные лаборатории обновлены.");
    }

    private async void LoadLotsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadLotsAsync);
    }

    private async Task LoadLotsAsync()
    {
        _allLots = await _apiClient.GetRawMaterialLotsAsync();
        LotsGrid.ItemsSource = _allLots;
        UpdateLotCard();
        SetStatus("Партии сырья загружены.");
    }

    private async void SearchLotsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(() =>
        {
            var search = (LotSearchTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(search))
            {
                LotsGrid.ItemsSource = _allLots;
            }
            else
            {
                LotsGrid.ItemsSource = _allLots
                    .Where(x => ContainsText(x.InternalLotNumber, search) || ContainsText(x.RawMaterialName, search))
                    .ToList();
            }

            UpdateLotCard();
            SetStatus("Поиск по партиям выполнен.");
            return Task.CompletedTask;
        });
    }

    private void LotsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateLotCard();
    }

    private void UpdateLotCard()
    {
        if (LotsGrid.SelectedItem is not RawMaterialLotListItem lot)
        {
            LotDetailTextBox.Text = "Выберите партию в таблице слева.";
            return;
        }

        var tests = _allTests.Where(x => x.RawMaterialLotId == lot.Id).OrderByDescending(x => x.CreatedAt).ToList();
        var decisions = _allDecisions.Where(x => x.RawMaterialLotId == lot.Id).OrderByDescending(x => x.DecidedAt).ToList();

        var text = new StringBuilder();
        text.AppendLine($"Внутренний номер: {lot.InternalLotNumber}");
        text.AppendLine($"Номер поставщика: {lot.SupplierLotNumber ?? "-"}");
        text.AppendLine($"Сырьё: {lot.RawMaterialName}");
        text.AppendLine($"Поставщик: {lot.SupplierName}");
        text.AppendLine($"Дата поступления: {lot.ReceivedAt:dd.MM.yyyy HH:mm}");
        text.AppendLine($"Получено: {lot.QuantityReceived.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Доступно: {lot.QuantityAvailable.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Место хранения: {lot.StorageLocation}");
        text.AppendLine($"Статус партии: {GetLotStatusText(lot.Status)}");
        text.AppendLine();
        text.AppendLine($"Испытаний по партии: {tests.Count}");

        foreach (var test in tests.Take(5))
        {
            text.AppendLine($"- {test.TestNumber} | {GetTestStatusText(test.Status)} | {test.TestKind}");
        }

        text.AppendLine();
        text.AppendLine($"Решений по партии: {decisions.Count}");

        foreach (var decision in decisions.Take(3))
        {
            text.AppendLine($"- {GetDecisionStatusText(decision.DecisionStatus)} | {decision.DecidedAt:dd.MM.yyyy HH:mm}");
        }

        LotDetailTextBox.Text = text.ToString();
    }

    private async void CreateTestFromLotButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(() =>
        {
            if (LotsGrid.SelectedItem is not RawMaterialLotListItem lot)
            {
                throw new InvalidOperationException("Выберите партию сырья.");
            }

            SubjectTypeComboBox.SelectedIndex = 0;
            RawMaterialLotIdTextBox.Text = lot.Id.ToString(CultureInfo.InvariantCulture);
            ProductionBatchIdTextBox.Text = string.Empty;
            TestKindTextBox.Text = "Входной контроль";

            var specification = _allSpecifications
                .Where(x => x.SubjectType == 1 && x.RawMaterialId == lot.RawMaterialId && x.IsActive)
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefault();

            if (specification is not null)
            {
                SpecificationIdTextBox.Text = specification.Id.ToString(CultureInfo.InvariantCulture);
            }

            SetStatus("Форма испытания заполнена по выбранной партии.");
            return Task.CompletedTask;
        });
    }

    private async void LoadSpecificationsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadSpecificationsAsync);
    }

    private async Task LoadSpecificationsAsync()
    {
        _allSpecifications = await _apiClient.GetSpecificationsAsync();
        SpecificationsGrid.ItemsSource = _allSpecifications;
        SetStatus("Спецификации загружены.");
    }

    private void SpecificationsGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SpecificationsGrid.SelectedItem is not QualitySpecificationItem item)
        {
            return;
        }

        SpecificationIdTextBox.Text = item.Id.ToString(CultureInfo.InvariantCulture);
        SubjectTypeComboBox.SelectedIndex = item.SubjectType == 2 ? 1 : 0;
    }

    private async void LoadTestsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(LoadTestsAsync);
    }

    private async Task LoadTestsAsync()
    {
        _allTests = await _apiClient.GetTestsAsync();
        TestsGrid.ItemsSource = _allTests;
        TestDetailTextBox.Text = "Выберите испытание.";
        UpdateLotCard();
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
            TestDetailTextBox.Text = BuildTestDetailText(detail);
            ResultsTextBox.Text = BuildResultHint(detail);
            ResultSummaryTextBox.Text = detail.Test.ResultSummary ?? string.Empty;
            DecisionCommentTextBox.Text = "Решение принято";
            DecisionBlockReasonTextBox.Text = string.Empty;
            SetStatus("Карточка испытания загружена.");
        });
    }

    private async void CreateTestButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var request = new CreateLaboratoryTestRequest
            {
                SubjectType = GetSelectedTagValue(SubjectTypeComboBox),
                RawMaterialLotId = ParseOptionalInt(RawMaterialLotIdTextBox.Text),
                ProductionBatchId = ParseOptionalInt(ProductionBatchIdTextBox.Text),
                QualitySpecificationId = ParseRequiredInt(SpecificationIdTextBox.Text, "QualitySpecificationId"),
                TestKind = TestKindTextBox.Text ?? string.Empty,
                Priority = GetSelectedTagValue(PriorityComboBox),
                Comment = NullIfWhiteSpace(CreateCommentTextBox.Text),
                TesterUserId = _currentUser.Id
            };

            await _apiClient.CreateTestAsync(request);
            await LoadTestsAsync();
            HistoryTextBox.Text = BuildHistoryText();
            SetStatus("Испытание создано.");
        });
    }

    private async void StartTestButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var item = RequireSelectedTest();
            await _apiClient.StartTestAsync(item.Id, _currentUser.Id);
            await LoadTestsAsync();
            HistoryTextBox.Text = BuildHistoryText();
            SetStatus("Испытание начато.");
        });
    }

    private async void SaveResultsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var item = RequireSelectedTest();
            var request = new SaveLaboratoryResultsRequest
            {
                Items = ParseResults(ResultsTextBox.Text)
            };

            await _apiClient.SaveResultsAsync(item.Id, request);
            var detail = await _apiClient.GetTestAsync(item.Id);
            TestDetailTextBox.Text = BuildTestDetailText(detail);
            ResultsTextBox.Text = BuildResultHint(detail);
            SetStatus("Результаты сохранены.");
        });
    }

    private async void CompleteTestButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var item = RequireSelectedTest();
            var request = new CompleteLaboratoryTestRequest
            {
                TesterUserId = _currentUser.Id,
                ResultSummary = NullIfWhiteSpace(ResultSummaryTextBox.Text)
            };

            await _apiClient.CompleteTestAsync(item.Id, request);
            await LoadTestsAsync();
            await LoadDecisionsAsync();
            HistoryTextBox.Text = BuildHistoryText();
            SetStatus("Испытание завершено.");
        });
    }

    private async void CreateDecisionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            var item = RequireSelectedTest();
            var request = new CreateQualityDecisionRequest
            {
                LaboratoryTestId = item.Id,
                DecisionStatus = GetSelectedTagValue(DecisionStatusComboBox),
                Comment = DecisionCommentTextBox.Text ?? string.Empty,
                BlockReason = NullIfWhiteSpace(DecisionBlockReasonTextBox.Text),
                DecidedByUserId = _currentUser.Id
            };

            await _apiClient.CreateDecisionAsync(request);
            await LoadDecisionsAsync();
            await LoadLotsAsync();
            HistoryTextBox.Text = BuildHistoryText();
            SetStatus("Решение по качеству принято.");
        });
    }

    private async void LoadDecisionsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await ExecuteAsync(async () =>
        {
            await LoadDecisionsAsync();
            HistoryTextBox.Text = BuildHistoryText();
        });
    }

    private async Task LoadDecisionsAsync()
    {
        _allDecisions = await _apiClient.GetDecisionsAsync();
        DecisionsGrid.ItemsSource = _allDecisions;
        SetStatus("Решения загружены.");
    }

    private string BuildHistoryText()
    {
        var text = new StringBuilder();
        text.AppendLine("Краткая история лаборатории");
        text.AppendLine();

        foreach (var test in _allTests.OrderByDescending(x => x.CreatedAt).Take(10))
        {
            text.AppendLine($"{test.CreatedAt:dd.MM.yyyy HH:mm} | {test.TestNumber} | {GetTestStatusText(test.Status)}");
        }

        text.AppendLine();

        foreach (var decision in _allDecisions.OrderByDescending(x => x.DecidedAt).Take(10))
        {
            text.AppendLine($"{decision.DecidedAt:dd.MM.yyyy HH:mm} | {decision.TestNumber} | {GetDecisionStatusText(decision.DecisionStatus)}");
        }

        return text.ToString();
    }

    private static string BuildTestDetailText(LaboratoryTestDetail detail)
    {
        var text = new StringBuilder();
        text.AppendLine($"Испытание: {detail.Test.TestNumber}");
        text.AppendLine($"Вид: {detail.Test.TestKind}");
        text.AppendLine($"Статус: {GetTestStatusText(detail.Test.Status)}");
        text.AppendLine($"Спецификация: {detail.Test.QualitySpecificationName}");
        text.AppendLine($"Итог: {detail.Test.ResultSummary ?? "-"}");
        text.AppendLine();
        text.AppendLine("Параметры:");

        foreach (var result in detail.Results)
        {
            text.AppendLine($"{result.Id}. {result.ParameterName}");
            text.AppendLine($"   Норма: {BuildNormText(result)}");
            text.AppendLine($"   Факт: {BuildActualValueText(result)}");
            text.AppendLine($"   Проверка: {GetCheckText(result.IsWithinRange)}");
        }

        return text.ToString();
    }

    private static string BuildResultHint(LaboratoryTestDetail detail)
    {
        var text = new StringBuilder();
        foreach (var result in detail.Results)
        {
            text.AppendLine($"{result.Id};;;;");
        }

        return text.ToString();
    }

    private static string BuildNormText(LaboratoryTestParameterResultItem item)
    {
        if (item.ValueType == 1)
        {
            var min = item.MinNumericValue?.ToString(CultureInfo.InvariantCulture) ?? "-";
            var max = item.MaxNumericValue?.ToString(CultureInfo.InvariantCulture) ?? "-";
            var unit = string.IsNullOrWhiteSpace(item.Unit) ? string.Empty : $" {item.Unit}";
            return $"{min} .. {max}{unit}";
        }

        if (item.ValueType == 2)
        {
            return item.TargetTextValue ?? "-";
        }

        if (item.ValueType == 3)
        {
            return item.TargetBooleanValue?.ToString() ?? "-";
        }

        return "-";
    }

    private static string BuildActualValueText(LaboratoryTestParameterResultItem item)
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

    private static string GetCheckText(bool? value)
    {
        return value switch
        {
            true => "В норме",
            false => "Не в норме",
            null => "Нет проверки"
        };
    }

    private static string GetTestStatusText(int status)
    {
        return status switch
        {
            1 => "Создано",
            2 => "В работе",
            3 => "Завершено",
            _ => $"Статус {status}"
        };
    }

    private static string GetDecisionStatusText(int decisionStatus)
    {
        return decisionStatus == 2 ? "Блокировка" : "Выпуск";
    }

    private static string GetLotStatusText(int status)
    {
        return status switch
        {
            1 => "На проверке",
            2 => "Разрешена",
            3 => "Заблокирована",
            _ => $"Статус {status}"
        };
    }

    private static bool ContainsText(string? source, string search)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(search, StringComparison.OrdinalIgnoreCase);
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

    private static decimal? ParseOptionalDecimal(string? value)
    {
        var text = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            throw new InvalidOperationException("Числовое значение результата введено некорректно.");
        }

        return result;
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

    private static int GetSelectedTagValue(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Tag?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new InvalidOperationException("Выберите значение из списка.");
    }

    private static List<SaveLaboratoryResultItem> ParseResults(string? input)
    {
        var lines = (input ?? string.Empty)
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

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

    private LaboratoryTestItem RequireSelectedTest()
    {
        return TestsGrid.SelectedItem as LaboratoryTestItem
               ?? throw new InvalidOperationException("Выберите испытание в таблице.");
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
