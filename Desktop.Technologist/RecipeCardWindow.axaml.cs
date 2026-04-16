using System.Globalization;
using System.Text;
using Avalonia.Controls;

namespace PlantProduction.Desktop;

public partial class RecipeCardWindow : Window
{
    public RecipeCardWindow()
    {
        InitializeComponent();
    }

    public RecipeCardWindow(RecipeDetail detail)
        : this()
    {
        TitleTextBlock.Text = $"Карточка рецептуры: {detail.Header.ProductName}";
        HeaderTextBox.Text = BuildHeaderText(detail);
        ComponentsGrid.ItemsSource = detail.Components;
    }

    private static string BuildHeaderText(RecipeDetail detail)
    {
        var text = new StringBuilder();
        text.AppendLine($"Продукт: {detail.Header.ProductName}");
        text.AppendLine($"Версия: {detail.Header.VersionNumber.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Статус: {detail.Header.Status.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Активна: {(detail.Header.IsActive ? "Да" : "Нет")}");
        text.AppendLine($"Создана: {detail.Header.CreatedAt:dd.MM.yyyy HH:mm}");
        text.AppendLine($"Создал: {detail.Header.CreatedByName}");
        text.AppendLine($"Утверждена: {(detail.Header.ApprovedAt is null ? "-" : detail.Header.ApprovedAt.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))}");
        text.AppendLine($"Кто утвердил: {detail.Header.ApprovedByName ?? "-"}");
        text.AppendLine($"Примечание: {detail.Header.Notes ?? "-"}");
        return text.ToString();
    }
}
