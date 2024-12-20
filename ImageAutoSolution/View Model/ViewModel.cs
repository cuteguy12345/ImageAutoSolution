using ImageAutoSolution.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Net.Mail;

namespace ImageAutoSolution.View_Model
{
    public class ViewModel : ViewModelBase
    {
        public ObservableCollection<int> Hours { get; } = new ObservableCollection<int>(Enumerable.Range(0, 24));
        public ObservableCollection<int> Minutes { get; } = new ObservableCollection<int>(Enumerable.Range(0, 60));
        
         private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                CurrentPage = 1;
                OnPropertyChanged(nameof(StartDate));
                LoadData();
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                CurrentPage = 1;
                OnPropertyChanged(nameof(EndDate));
                LoadData();
            }
        }
        private int? _startHour;
        public int? StartHour
        {
            get => _startHour;
            set
            {
                _startHour = value;
                OnPropertyChanged(nameof(StartHour));
                FilterData();
            }
        }

        private int? _startMinute;
        public int? StartMinute
        {
            get => _startMinute;
            set
            {
                _startMinute = value;
                OnPropertyChanged(nameof(StartMinute));
                FilterData();
            }
        }

        private int? _endHour;
        public int? EndHour
        {
            get => _endHour;
            set
            {
                _endHour = value;
                OnPropertyChanged(nameof(EndHour));
                FilterData();
            }
        }

        private int? _endMinute;
        public int? EndMinute
        {
            get => _endMinute;
            set
            {
                _endMinute = value;
                OnPropertyChanged(nameof(EndMinute));
                FilterData(); 
            }
        }
        private bool _goodReadFilter;
        public bool GoodReadFilter
        {
            get => _goodReadFilter;
            set
            {
                _goodReadFilter = value;
                OnPropertyChanged(nameof(GoodReadFilter));
                CurrentPage = 1;
                FilterData();
            }
        }

        private void GoToNextPage()
        {
            if (_currentPage * _itemsPerPage < TotalItemsCount)
            {
                CurrentPage++;
                FilterData();
            }
        }

        private void GoToPreviousPage()
        {
            if (_currentPage > 1)
            {
                CurrentPage--;
                FilterData();
            }
        }

        private bool CanGoToNextPage()
        {
            return _currentPage * _itemsPerPage < TotalItemsCount;
        }

        private bool CanGoToPreviousPage()
        {
            return _currentPage > 1;
        }

        private int _currentPage = 1;
        private const int _itemsPerPage = 30;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (value != _currentPage)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                    LoadData();
                }
            }
        }

        private int _totalItemsCount;
        public int TotalItemsCount
        {
            get => _totalItemsCount;
            set
            {
                _totalItemsCount = value;
                OnPropertyChanged(nameof(TotalItemsCount));
            }
        }

        

        private ObservableCollection<ImageModel> _imageData;
        public ObservableCollection<ImageModel> ImageData
        {
            get => _imageData;
            set
            {
                _imageData = value;
                OnPropertyChanged(nameof(ImageData));
            }
        }
       
        public int TotalPages => (int)Math.Ceiling((double)TotalItemsCount / _itemsPerPage);
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

       

        private ObservableCollection<string> _groupNames;
        public ObservableCollection<string> GroupNames
        {
            get => _groupNames;
            set
            {
                _groupNames = value;
                OnPropertyChanged(nameof(GroupNames));
            }
        }

        private string _selectedGroupName;
        public string SelectedGroupName
        {
            get => _selectedGroupName;
            set
            {
                _selectedGroupName = value;
                CurrentPage = 1;
                OnPropertyChanged(nameof(SelectedGroupName));
                LoadData();
            }
        }
        private string _selectedMacAddress;
        public string SelectedMacAddress
        {
            get => _selectedMacAddress;
            set
            {
                _selectedMacAddress = value;
                OnPropertyChanged(nameof(SelectedMacAddress));
            }
        }

        private ObservableCollection<ImageModel> _tabControlHeaders;
        public ObservableCollection<ImageModel> TabControlHeaders
        {
            get => _tabControlHeaders;
            set
            {
                _tabControlHeaders = value;
                OnPropertyChanged(nameof(TabControlHeaders));
            }
        }

        private ObservableCollection<BitmapImage> _tabItems;
        public ObservableCollection<BitmapImage> TabItems
        {
            get => _tabItems;
            set
            {
                _tabItems = value;
                OnPropertyChanged(nameof(TabItems));
            }
        }

        private int? _selectedTriggerIndex;

        public int? SelectedTriggerIndex
        {
            get => _selectedTriggerIndex;
            set
            {
                _selectedTriggerIndex = value;
                OnPropertyChanged(nameof(SelectedTriggerIndex));
                LoadData();
            }
        }

        private void FilterData()
        {
            if (!StartDate.HasValue || !EndDate.HasValue || string.IsNullOrEmpty(SelectedGroupName))
            {
                ImageData = new ObservableCollection<ImageModel>();
                TotalItemsCount = 0;
                return;
            }

            DateTime startDateTime = StartDate.Value.Date
                .AddHours(StartHour ?? 0)
                .AddMinutes(StartMinute ?? 0);
            DateTime endDateTime = EndDate.Value.Date
                .AddHours(EndHour ?? 23)
                .AddMinutes(EndMinute ?? 59);

            var data = DatabaseHelper.GetImageData(
                triggerIndex: SelectedTriggerIndex,
                startDate: startDateTime,
                endDate: endDateTime,
                groupName: SelectedGroupName
            );

            if (GoodReadFilter)
            {
                data = data.Where(item => item.Good_Read == 0).ToList();
            }

            TotalItemsCount = data.Count();

          
            var pagedData = data
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage);
            ImageData = new ObservableCollection<ImageModel>(pagedData);

            OnPropertyChanged(nameof(TotalPages));
        }




        public ViewModel()
        {
            NextPageCommand = new RelayCommand(GoToNextPage, CanGoToNextPage);
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, CanGoToPreviousPage);
            LoadGroupNames();
            LoadData();
        }
        private void LoadGroupNames()
        {
            var groupNames = DatabaseHelper.GetGroupNames();
            GroupNames = new ObservableCollection<string>(groupNames);
        }
        private void LoadData()
        {
            if (!StartDate.HasValue || !EndDate.HasValue || string.IsNullOrEmpty(SelectedGroupName))
            {
                ImageData = new ObservableCollection<ImageModel>();
                TotalItemsCount = 0; 
                OnPropertyChanged(nameof(TotalPages)); 
                return; 
            }

            var data = DatabaseHelper.GetImageData(
                triggerIndex: SelectedTriggerIndex,
                startDate: StartDate,
                endDate: EndDate,
                groupName: SelectedGroupName
            );

            TotalItemsCount = data.Count(); 
            OnPropertyChanged(nameof(TotalPages)); 

            if (TotalItemsCount == 0)
            {
                ImageData = new ObservableCollection<ImageModel>();
            }
            else
            {
                var pagedData = data.Skip((_currentPage - 1) * _itemsPerPage).Take(_itemsPerPage);
                ImageData = new ObservableCollection<ImageModel>(pagedData);
            }
        }



        public void LoadImagesByGroupNameAndTriggerIndex(int triggerIndex, DateTime createDate, string macAddress)
        {
            string directoryPath = $"D:\\DataManImage\\{createDate:yyyyMMdd}\\{createDate:HH}\\{macAddress}";
            var images = new ObservableCollection<BitmapImage>();

            if (Directory.Exists(directoryPath))
            {
                var imageFiles = Directory.GetFiles(directoryPath, "*.jpg", SearchOption.AllDirectories);

                foreach (var file in imageFiles)
                {
                    if (file.Contains($".{triggerIndex}."))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri(file);
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                        images.Add(image);
                    }
                }
            }

            //TabItems = images;
        }

        public void FilterTabControlData(int triggerIndex)
        {
            var filteredData = DatabaseHelper.GetImageDataByTriggerIndex(triggerIndex: triggerIndex);

            TabControlHeaders = new ObservableCollection<ImageModel>(filteredData);
            foreach (var data in filteredData)
            {
                string directoryPath = $"D:\\DataManImage\\{data.CreateDT:yyyyMMdd}\\{data.CreateDT:HH}";


            }
        }

    }
}