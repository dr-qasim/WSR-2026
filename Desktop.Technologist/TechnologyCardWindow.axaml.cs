using System.Globalization;
using System.Text;
using Avalonia.Controls;

namespace PlantProduction.Desktop;

public partial class TechnologyCardWindow : Window
{
    public TechnologyCardWindow()
    {
        InitializeComponent();
    }

    public TechnologyCardWindow(TechnologyCardDetail detail)
        : this()
    {
        TitleTextBlock.Text = $"Карточка техкарты: {detail.Header.Title}";
        HeaderTextBox.Text = BuildHeaderText(detail);
        StepsTextBox.Text = BuildStepsText(detail);
    }

    private static string BuildHeaderText(TechnologyCardDetail detail)
    {
        var text = new StringBuilder();
        text.AppendLine($"Продукт: {detail.Header.ProductName}");
        text.AppendLine($"Версия: {detail.Header.VersionNumber.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Название: {detail.Header.Title}");
        text.AppendLine($"Статус: {detail.Header.Status.ToString(CultureInfo.InvariantCulture)}");
        text.AppendLine($"Активна: {(detail.Header.IsActive ? "Да" : "Нет")}");
        text.AppendLine($"Создана: {detail.Header.CreatedAt:dd.MM.yyyy HH:mm}");
        text.AppendLine($"Создал: {detail.Header.CreatedByName}");
        text.AppendLine($"Утверждена: {(detail.Header.ApprovedAt is null ? "-" : detail.Header.ApprovedAt.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))}");
        text.AppendLine($"Кто утвердил: {detail.Header.ApprovedByName ?? "-"}");
        text.AppendLine($"Описание: {detail.Header.Description ?? "-"}");
        return text.ToString();
    }

    private static string BuildStepsText(TechnologyCardDetail detail)
    {
        var text = new StringBuilder();

        foreach (var step in detail.Steps)
        {
            text.AppendLine($"Шаг {step.StepOrder.ToString(CultureInfo.InvariantCulture)}: {step.Title}");
            text.AppendLine($"Тип шага: {step.StepType.ToString(CultureInfo.InvariantCulture)}");
            text.AppendLine($"Обязательный: {(step.IsRequired ? "Да" : "Нет")}");
            text.AppendLine($"Длительность: {(step.ExpectedDurationMinutes?.ToString(CultureInfo.InvariantCulture) ?? "-")} мин");
            text.AppendLine($"Инструкция: {step.Instruction ?? "-"}");

            if (step.Parameters.Count == 0)
            {
                text.AppendLine("Параметры: нет");
            }
            else
            {
                text.AppendLine("Параметры:");
                foreach (var parameter in step.Parameters)
                {
                    text.AppendLine($"- {parameter.Name}");
                    text.AppendLine($"  Тип: {parameter.ValueType.ToString(CultureInfo.InvariantCulture)}");
                    text.AppendLine($"  Единица: {parameter.Unit ?? "-"}");
                    text.AppendLine($"  Целевое число: {parameter.TargetNumericValue?.ToString(CultureInfo.InvariantCulture) ?? "-"}");
                    text.AppendLine($"  Мин: {parameter.MinNumericValue?.ToString(CultureInfo.InvariantCulture) ?? "-"}");
                    text.AppendLine($"  Макс: {parameter.MaxNumericValue?.ToString(CultureInfo.InvariantCulture) ?? "-"}");
                    text.AppendLine($"  Целевой текст: {parameter.TargetTextValue ?? "-"}");
                    text.AppendLine($"  Целевое bool: {parameter.TargetBooleanValue?.ToString() ?? "-"}");
                    text.AppendLine($"  Обязательный: {(parameter.IsRequired ? "Да" : "Нет")}");
                    text.AppendLine($"  Комментарий: {parameter.Comment ?? "-"}");
                }
            }

            text.AppendLine();
        }

        return text.ToString();
    }
}
