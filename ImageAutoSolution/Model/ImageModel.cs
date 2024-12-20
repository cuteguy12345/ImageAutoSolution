using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageAutoSolution.Model
{
    public class ImageModel
    {
        public string GroupName { get; set; }
        public DateTime CreateDT { get; set; }
        public int Trigger_Index { get; set; }
        public int Good_Read { get; set; }
        public string MacAddress {  get; set; }
        public int Image_Index {  get; set; }
        public string Name { get; set; }
        public ObservableCollection<BitmapImage> Images { get; set; } = new ObservableCollection<BitmapImage>();

    }
}