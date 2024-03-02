using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VortexApp.UI.Controls
{
    public class CustomTextBlock : TextBlock
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        "IsSelected", typeof(bool), typeof(CustomTextBlock), new PropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
            "SelectionBrush", typeof(Brush), typeof(CustomTextBlock), new PropertyMetadata(Brushes.White));

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        public static readonly DependencyProperty DefaultBrushProperty = DependencyProperty.Register(
            "DefaultBrush", typeof(Brush), typeof(CustomTextBlock), new PropertyMetadata(Brushes.Black, OnDefaultBrushChanged));

        public Brush DefaultBrush
        {
            get { return (Brush)GetValue(DefaultBrushProperty); }
            set { SetValue(DefaultBrushProperty, value); }
        }

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            private set { SetValue(ForegroundProperty, value); }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customTextBlock = (CustomTextBlock)d;

            if (customTextBlock != null)
            {
                if (customTextBlock.IsSelected)
                {
                    customTextBlock.Foreground = customTextBlock.SelectionBrush;
                    return;
                }

                customTextBlock.Foreground = customTextBlock.DefaultBrush;
            }
        }

        private static void OnDefaultBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customTextBlock = (CustomTextBlock)d;

            if (!customTextBlock.IsSelected)
            {
                customTextBlock.Foreground = customTextBlock.DefaultBrush;
            }
        }
    }
}
