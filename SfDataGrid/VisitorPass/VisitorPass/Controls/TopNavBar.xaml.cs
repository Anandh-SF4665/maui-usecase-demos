namespace VisitorPass.Controls;

/// <summary>
/// VP-022 + VP-080 — top navigation bar injected into every screen.
///
/// Consumers set:
///   <c>Title</c>, <c>Subtitle</c>, <c>ShowRegisterButton</c>,
///   <c>ShowThemeToggle</c>, <c>IsDarkMode</c>
///
/// and listen to <see cref="RegisterClicked"/> and
/// <see cref="ThemeToggleClicked"/> in code-behind to open the
/// registration surface or to invoke <c>ThemeService.Toggle()</c>.
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

    /// <summary>When true the sun/moon theme toggle is shown (default true).</summary>
    public static readonly BindableProperty ShowThemeToggleProperty = BindableProperty.Create(
        nameof(ShowThemeToggle), typeof(bool), typeof(TopNavBar), defaultValue: true);

    /// <summary>When true the toggle shows a sun glyph (i.e. it's currently in dark mode).</summary>
    public static readonly BindableProperty IsDarkModeProperty = BindableProperty.Create(
        nameof(IsDarkMode), typeof(bool), typeof(TopNavBar), defaultValue: false,
        propertyChanged: OnIsDarkModeChanged);

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

    /// <summary>When true the theme toggle pill is shown (default true).</summary>
    public bool ShowThemeToggle
    {
        get => (bool)GetValue(ShowThemeToggleProperty);
        set => SetValue(ShowThemeToggleProperty, value);
    }

    /// <summary>When true the toggle shows a sun glyph; when false a moon glyph.</summary>
    public bool IsDarkMode
    {
        get => (bool)GetValue(IsDarkModeProperty);
        set => SetValue(IsDarkModeProperty, value);
    }

    /// <summary>
    /// Glyph rendered inside the toggle pill. Uses MAUI's bundled
    /// Material Icons font (light_mode / dark_mode codepoints).
    /// VP-104 — replaces the earlier emoji glyphs.
    /// </summary>
    public string ThemeToggleGlyph => IsDarkMode
        ? "\ue51c"  // light_mode — tap to switch to light
        : "\ue51d"; // dark_mode  — tap to switch to dark

    /// <summary>Raised when the user taps the "Register Visitor" trigger.</summary>
    public event EventHandler? RegisterClicked;

    /// <summary>Raised when the user taps the sun/moon theme toggle.</summary>
    public event EventHandler? ThemeToggleClicked;

    public TopNavBar()
    {
        InitializeComponent();
        // Wire both TapGestureRecognizers to handlers declared in
        // code-behind. The two pills live at known grid columns
        // (Column="1" = theme, Column="2" = register) so the
        // Tapped handlers hook them in XAML-free code below.
        ThemeTrigger.Tapped += (_, e) => ThemeToggleClicked?.Invoke(this, EventArgs.Empty);
    }

    private static void OnIsDarkModeChanged(BindableObject bindable, object oldValue, object newValue)
        => ((TopNavBar)bindable).OnPropertyChanged(nameof(ThemeToggleGlyph));

    private void SfButton_Clicked(object sender, EventArgs e)
    {
        RegisterClicked?.Invoke(this, EventArgs.Empty);
    }
}
