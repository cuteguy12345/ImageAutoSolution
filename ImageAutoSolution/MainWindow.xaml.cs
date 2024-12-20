using ImageAutoSolution.Model;
using ImageAutoSolution.View_Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageAutoSolution
{
    public partial class MainWindow : Window
    {
        private const double MinScale = 1.0;
        private const double MaxScale = 5.0;

        private Point _startPoint;
        private bool _isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }

        private void ImageDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageDataGrid.SelectedItem is ImageModel selectedItem)
            {
                if (DataContext is ViewModel viewModel)
                { 
                    viewModel.FilterTabControlData(selectedItem.Trigger_Index);
                    viewModel.LoadImagesByGroupNameAndTriggerIndex(selectedItem.Trigger_Index, selectedItem.CreateDT, selectedItem.MacAddress);
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ImageModel selectedTab)
            {
                if (DataContext is ViewModel viewModel)
                {
                    viewModel.LoadImagesByGroupNameAndTriggerIndex(
                        selectedTab.Trigger_Index,
                        selectedTab.CreateDT,
                        selectedTab.MacAddress);

                    selectedTab.Images = viewModel.TabItems;
                }
            }
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Point mousePosition = e.GetPosition(PopupImage);

                double scaleFactor = e.Delta > 0 ? 1.1 : 0.9;

                ScaleTransform currentScaleTransform = PopupImageScaleTransform;

                double newScaleX = currentScaleTransform.ScaleX * scaleFactor;
                double newScaleY = currentScaleTransform.ScaleY * scaleFactor;

                newScaleX = Math.Max(MinScale, Math.Min(MaxScale, newScaleX));
                newScaleY = Math.Max(MinScale, Math.Min(MaxScale, newScaleY));

                PopupImage.RenderTransformOrigin = new Point(mousePosition.X / PopupImage.ActualWidth, mousePosition.Y / PopupImage.ActualHeight);
                currentScaleTransform.ScaleX = newScaleX;
                currentScaleTransform.ScaleY = newScaleY;
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = sender as Image;
            if (clickedImage != null)
            {
                ResetPopupImageTransform();
                PopupImage.Source = clickedImage.Source;
                ImagePopup.IsOpen = true;
            }
        }
        private void ResetPopupImageTransform()
        {
            PopupImageScaleTransform.ScaleX = 1.0;
            PopupImageScaleTransform.ScaleY = 1.0;

            PopupImageTranslateTransform.X = 0;
            PopupImageTranslateTransform.Y = 0;
        }

        private void PopupImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                _startPoint = e.GetPosition(PopupImage);
                _isDragging = true;

                Mouse.Capture(PopupImage);
            }
        }


        private void PopupImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPosition = e.GetPosition(PopupImage);
                double offsetX = currentPosition.X - _startPoint.X;
                double offsetY = currentPosition.Y - _startPoint.Y;

                TranslateTransform currentTranslateTransform = PopupImageTranslateTransform;
                currentTranslateTransform.X += offsetX;
                currentTranslateTransform.Y += offsetY;

                _startPoint = currentPosition;
            }
        }
        private void PopupImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                Mouse.Capture(null); 
            }
        }

    }
}
