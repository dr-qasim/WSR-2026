using System.Globalization;
using System.Text;
using Avalonia.Controls;

namespace PlantProduction.Desktop;

public partial class RawMaterialLotCardWindow : Window
{
    public RawMaterialLotCardWindow()
    {
        InitializeComponent();
    }

    public RawMaterialLotCardWindow(
        RawMaterialLotListItem lot,
        List<LaboratoryTestItem> tests,
        List<QualityDecisionItem> decisions)
        : this()
    {
        TitleTextBlock.Text = $"Карточка партии сырья: {lot.InternalLotNumber}";
        HeaderTextBox.Text = BuildHeaderText(lot, tests, decisions);
        TestsGrid.ItemsSource = tests;
        DecisionsGrid.ItemsSource = decisions;
    }

    private static string BuildHeaderText(
        RawMaterialLotListItem lot,
        List<LaboratoryTestItem> tests,
        List<QualityDecisionItem> decisions)
    {
        var currentDecision = decisions.FirstOrDefault(x => x.IsCurrent);
        var text = new StringBuilder();
        text.AppendLine($"Внутренний номер: {lot.InternalLotNumber}");
        text.AppendLine($"Номер поставщика: {lot.SupplierLotNumber ?? "-"}");
        text.AppendLine($"Сырье: {lot.RawMaterialName}");
        text.AppendLine($"Поставщик: {lot.SupplierName}");
        text.AppendLine($"Поступление: {lot.ReceivedAt:dd.MM.yyyy HH:mm}");
        text.AppendLine($"Получено: {lot.QuantityReceived.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Доступно: {lot.QuantityAvailable.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Место хранения: {lot.StorageLocation}");
        text.AppendLine($"Лабораторный статус: {lot.LaboratoryStatusText}");
        text.AppendLine();
        text.AppendLine($"Испытаний найдено: {tests.Count.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Решений найдено: {decisions.Count.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Текущее решение: {currentDecision?.DecisionStatusText ?? "Нет решения"}");
        text.AppendLine($"Комментарий: {currentDecision?.Comment ?? "-"}");
        text.AppendLine($"Причина блокировки: {currentDecision?.BlockReason ?? "-"}");
        return text.ToString();
    }
}
