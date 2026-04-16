using System.Globalization;
using System.Text;
using Avalonia.Controls;

namespace PlantProduction.Desktop;

public partial class ProductCardWindow : Window
{
    public ProductCardWindow()
    {
        InitializeComponent();
    }

    public ProductCardWindow(ProductDetail detail)
        : this()
    {
        TitleTextBlock.Text = $"Карточка продукта: {detail.Header.Name}";
        HeaderTextBox.Text = BuildHeaderText(detail);
        RecipesGrid.ItemsSource = detail.Recipes;
        TechnologyCardsGrid.ItemsSource = detail.TechnologyCards;
    }

    private static string BuildHeaderText(ProductDetail detail)
    {
        var text = new StringBuilder();
        text.AppendLine($"Код: {detail.Header.Code}");
        text.AppendLine($"Наименование: {detail.Header.Name}");
        text.AppendLine($"Тип продукта: {detail.Header.ProductTypeName}");
        text.AppendLine($"Форма продукта: {detail.Header.ProductFormName}");
        text.AppendLine($"Статус: {detail.Header.Status.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Описание: {detail.Header.Description ?? "-"}");
        text.AppendLine();
        text.AppendLine($"Рецептур найдено: {detail.Recipes.Count.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Техкарт найдено: {detail.TechnologyCards.Count.ToString(CultureInfo.InvariantCulture)}");
        return text.ToString();
    }
}
