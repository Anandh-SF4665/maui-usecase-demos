namespace VisitorPass.Controls;

/// <summary>
/// VP-022 — top navigation bar injected into every screen.
///
/// Consumers set:
///
///     &lt;controls:TopNavBar Title="Visitor Command Center"
///                         Subtitle="Real-time overview"
///                         ShowRegisterButton="True" /&gt;
///
/// and listen to <see cref="RegisterClicked"/> in code-behind to
/// open the registration surface (popup on desktop / tablet,
/// slide-up modal page on phone, per VP-073).
/// </summary>
public partial class TopNavBar : ContentView
{
    /// <summary>Title text — large headline.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(TopNavBar), defaultValue: string.Empty);

    /// <summary>Optional subtitle — small caption under the title.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(TopNavBar), defaultValue: string.Empty);

    /// <summary>When true the "Register Visitor" trigger is shown.</summary>
    public static readonly BindableProperty ShowRegisterButtonProperty = BindableProperty.Create(
        nameof(ShowRegisterButton), typeof(bool), typeof(TopNavBar), defaultValue: true);

    /// <summary>Title text — large headline.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional subtitle — small caption under the title.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>When true the "Register Visitor" trigger is shown.</summary>
    public bool ShowRegisterButton
    {
        get => (bool)GetValue(ShowRegisterButtonProperty);
        set => SetValue(ShowRegisterButtonProperty, value);
    }

    /// <summary>Raised when the user taps the "Register Visitor" trigger.</summary>
    public event EventHandler? RegisterClicked;

    public TopNavBar()
    {
        InitializeComponent();
        // The XAML's <TapGestureRecognizer x:Name="HeaderTrigger" /> is
        // declared inside the second Border child of the root Grid
        // (Column="1"). Find it by walking the visual tree rather
        // than hooking up Tapped in XAML so the event handler lives
        // next to the RegisterClicked definition.
        if (Content is Grid grid
            && grid.Children.LastOrDefault() is Border pill
            && pill.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap)
        {
            tap.Tapped += OnRegisterTapped;
        }
    }

    private void OnRegisterTapped(object? sender, TappedEventArgs e)
        => RegisterClicked?.Invoke(this, EventArgs.Empty);
}
