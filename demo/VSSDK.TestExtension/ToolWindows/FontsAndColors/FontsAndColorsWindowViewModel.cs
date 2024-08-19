using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace TestExtension
{
    public class FontsAndColorsWindowViewModel : IDisposable
    {
        private readonly ConfiguredFontAndColorSet<DemoFontAndColorCategory> _fontAndColors;
        private readonly ObservableCollection<string> _events;

        public FontsAndColorsWindowViewModel(ConfiguredFontAndColorSet<DemoFontAndColorCategory> fontAndColors, JoinableTaskFactory joinableTaskFactory)
        {
            // Remember the font and color set so that
            // we can dispose of it when we are disposed.
            _fontAndColors = fontAndColors;

            EditFontsAndColorsCommand = new DelegateCommand(
                () => VS.Commands.ExecuteAsync(
                    KnownCommands.Tools_Options,
                    "{57F6B7D2-1436-11D1-883C-0000F87579D2}"
                ).FireAndForget(),
                () => true,
                joinableTaskFactory
            );

            _events = new ObservableCollection<string>();
            Events = new ReadOnlyObservableCollection<string>(_events);

            fontAndColors.FontChanged += OnFontChanged;
            fontAndColors.ColorChanged += OnColorChanged;

            TopLeft = fontAndColors.GetColor(fontAndColors.Category.TopLeft);
            TopRight = fontAndColors.GetColor(fontAndColors.Category.TopRight);
            BottomLeft = fontAndColors.GetColor(fontAndColors.Category.BottomLeft);
            BottomRight = fontAndColors.GetColor(fontAndColors.Category.BottomRight);
        }

        private void OnFontChanged(object sender, EventArgs e)
        {
            _events.Insert(0, $"{DateTime.Now}: Font Changed");
        }

        private void OnColorChanged(object sender, ConfiguredColorChangedEventArgs e)
        {
            _events.Insert(0, $"{DateTime.Now}: Color Changed - {e.Definition.Name}");
        }

        public ICommand EditFontsAndColorsCommand { get; }

        public ReadOnlyObservableCollection<string> Events { get; }

        public ConfiguredFont Font => _fontAndColors.Font;

        public ConfiguredColor TopLeft { get; }

        public ConfiguredColor TopRight { get; }

        public ConfiguredColor BottomLeft { get; }

        public ConfiguredColor BottomRight { get; }

        public void Dispose()
        {
            _fontAndColors.FontChanged -= OnFontChanged;
            _fontAndColors.ColorChanged -= OnColorChanged;

            // Dispose of the font and color set so that it stops 
            // listening for changes to the font and colors.
            _fontAndColors.Dispose();
        }
    }
}
