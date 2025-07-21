using CommunityToolkit.Maui.Animations;
using CommunityToolkit.Maui.Behaviors;


namespace MauiStudyApp2;

// package CommunityToolkit.Maui
class AttentionAnimation : BaseAnimation
{
    public override async Task Animate(VisualElement view, CancellationToken token = default)
    {
        for (int i = 0; i < 6; i++)
        {
            token.ThrowIfCancellationRequested();
            await view.FadeTo(0.0, Length, Easing);
            token.ThrowIfCancellationRequested();
            await view.FadeTo(1, Length, Easing);
        }
    }

    public static readonly BindableProperty AttachBehaviorProperty =
        BindableProperty.CreateAttached(
            "AttachBehavior", typeof(bool),
            typeof(AttentionAnimation),
            false, propertyChanged: OnAttachBehaviorChanged);

    public static bool GetAttachBehavior(BindableObject view) =>
        (bool)view.GetValue(AttachBehaviorProperty);

    public static void SetAttachBehavior(BindableObject view, bool value) =>
        view.SetValue(AttachBehaviorProperty, value);

    static void OnAttachBehaviorChanged(BindableObject view, object oldValue, object newValue)
    {
        var img = view as VisualElement;
        if (img == null)
            return;

        bool shouldAttach = (bool)newValue;

        if (shouldAttach)
        {
            var b = new AnimationBehavior()
            {
                EventName = "Loaded",
                AnimationType = new AttentionAnimation()
                {
                    Length = 300,
                    Easing = Easing.Linear
                }
            };
            img.Behaviors.Add(b);
            
            // b.AnimateCommand.Execute(CancellationToken.None);
        }
        else
        {
            var toRemove = img.Behaviors
                .FirstOrDefault(b => b is AnimationBehavior);

            if (toRemove != null)
                img.Behaviors.Remove(toRemove);
        }
    }
}
