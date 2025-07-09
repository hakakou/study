namespace MauiStudyApp.Custom
{
    public class MyLabel : Label
    {
        public static readonly BindableProperty ColorProperty =
            BindableProperty.Create(nameof(Color), typeof(string),
                typeof(MyLabel), default(string),
                propertyChanged: OnColorPropertyChanged);

        public string Color
        {
            get => (string)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        static void OnColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        { }
    }
}
