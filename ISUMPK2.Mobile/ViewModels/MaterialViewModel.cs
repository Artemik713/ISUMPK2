using ISUMPK2.Application.Services;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ISUMPK2.Mobile.ViewModels
{
    public class MaterialListViewModel : INotifyPropertyChanged
    {
        private readonly IMaterialService _materialService;
        private readonly INavigationService _navigationService;
        private bool _isLoading;
        private string _searchQuery;
        private ObservableCollection<MaterialModel> _materials;

        public string Title => "Материалы";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterMaterials();
                }
            }
        }

        public ObservableCollection<MaterialModel> Materials
        {
            get => _materials;
            set
            {
                if (_materials != value)
                {
                    _materials = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewMaterialCommand { get; }
        public ICommand CreateMaterialCommand { get; }

        public MaterialListViewModel(IMaterialService materialService, INavigationService navigationService)
        {
            _materialService = materialService;
            _navigationService = navigationService;

            RefreshCommand = new Command(async () => await LoadMaterialsAsync());
            ViewMaterialCommand = new Command<Guid>(ViewMaterial);
            CreateMaterialCommand = new Command(CreateMaterial);

            Materials = new ObservableCollection<MaterialModel>();
        }

        public async Task LoadMaterialsAsync()
{
    if (IsLoading) return;

    try
    {
        IsLoading = true;
        
        // Используем существующий метод GetAllMaterialsAsync вместо отсутствующего GetMaterialsAsync
        var materialDtos = await _materialService.GetAllMaterialsAsync();
        
        // Конвертируем MaterialDto в MaterialModel
        var materials = materialDtos.Select(dto => new MaterialModel
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            UnitOfMeasure = dto.UnitOfMeasure,
            CurrentStock = dto.CurrentStock,
            MinimumStock = dto.MinimumStock,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            CategoryName = dto.CategoryName,
            Specifications = dto.Specifications,
            Manufacturer = dto.Manufacturer,
            PartNumber = dto.PartNumber,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            // Вычисляемое поле
            IsLowStock = dto.CurrentStock < dto.MinimumStock
        }).ToList();
        
        Materials = new ObservableCollection<MaterialModel>(materials);
    }
    catch (Exception ex)
    {
        await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить материалы: {ex.Message}", "OK");
    }
    finally
    {
        IsLoading = false;
    }
}

        private void FilterMaterials()
        {
            // Реализация фильтрации по поисковому запросу
        }

        private void ViewMaterial(Guid materialId)
        {
            _navigationService.NavigateToAsync($"material-details?id={materialId}");
        }

        private void CreateMaterial()
        {
            _navigationService.NavigateToAsync("create-material");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}