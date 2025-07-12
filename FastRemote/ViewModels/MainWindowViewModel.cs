using Prism.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace  FastRemote.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = "Prism Application";
      
        public MainWindowViewModel()
        {

        }

        [RelayCommand]
        void Test()
        { 
        
        }
    }
}
