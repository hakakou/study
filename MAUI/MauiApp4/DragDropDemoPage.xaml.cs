namespace MauiApp4
{
    public partial class DragDropDemoPage : ContentPage
    {
        public DragDropDemoPage()
        {
            InitializeComponent();
        }

        private void OnDragStarting(object sender, DragStartingEventArgs e)
        {
            var border = sender as Border;
            if (border?.Content is Label label)
            {
                // CRITICAL: Must set Data.Text to enable drag operation
                e.Data.Text = label.Text;
                
                // Store additional data in properties
                e.Data.Properties.Add("ItemText", label.Text);
                e.Data.Properties.Add("ItemColor", border.BackgroundColor.ToHex());
                
                statusLabel.Text = $"Dragging: {label.Text}";
            }
        }

        private void DropGestureRecognizer_DragOver(object sender, DragEventArgs e)
        {
            // Change appearance when hovering over drop zone
            if (sender is Border border)
            {
                border.Opacity = 0.7;
                border.Scale = 1.05;
            }
            statusLabel.Text = "Hovering over drop zone...";
        }

        private void DropGestureRecognizer_DragLeave(object sender, DragEventArgs e)
        {
            // Reset appearance when leaving drop zone
            if (sender is Border border)
            {
                border.Opacity = 1.0;
                border.Scale = 1.0;
            }
            statusLabel.Text = "Ready to drag and drop";
        }

        private void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
        {
            // Reset appearance
            if (sender is Border border)
            {
                border.Opacity = 1.0;
                border.Scale = 1.0;

                // Get the dropped item text
                if (e.Data.Properties.TryGetValue("ItemText", out object itemText))
                {
                    string droppedItem = itemText.ToString();
                    
                    // Update the corresponding label based on which drop zone
                    if (border == dropZone1)
                    {
                        dropZone1Label.Text = $"Dropped: {droppedItem}";
                    }
                    else if (border == dropZone2)
                    {
                        dropZone2Label.Text = $"Dropped: {droppedItem}";
                    }
                    else if (border == dropZone3)
                    {
                        dropZone3Label.Text = $"Dropped: {droppedItem}";
                    }

                    statusLabel.Text = $"Successfully dropped {droppedItem}!";
                }
            }
        }
    }
}
