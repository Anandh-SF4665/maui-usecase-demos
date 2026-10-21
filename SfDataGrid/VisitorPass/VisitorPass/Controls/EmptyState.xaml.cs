namespace VisitorPass.Controls;

/// <summary>
/// VP-023 — empty-state placeholder.
///
///     &lt;controls:EmptyState Title="No matching visitors"
///                          Message="Try a different status filter or
///                                   clear the search box." /&gt;
///
/// Rendered when a filter returns zero rows. The placeholder uses
/// the existing <c>SectionTitle</c> and <c>SectionEyebrow</c> styles
/// from <c>Resources/Styles/VisitorPassCard.xaml</c>.
/// </summary>
public partial class EmptyState : ContentView
{
    /// <summary>Bold heading.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(EmptyState), defaultValue: string.Empty);

    /// <summary>Body copy.</summary>
    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message), typeof(string), typeof(EmptyState), defaultValue: string.Empty);

    /// <summary>Bold heading.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Body copy.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public EmptyState() => InitializeComponent();
}
