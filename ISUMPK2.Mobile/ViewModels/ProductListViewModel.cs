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
    public class ProductListViewModel : INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private readonly INavigationService _navigationService;
        private bool _isLoading;
        private string _searchQuery;
        private ObservableCollection<ProductModel> _products;

        public string Title => "Продукция";

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
                    FilterProducts();
                }
            }
        }

        public ObservableCollection<ProductModel> Products
        {
            get => _products;
            set
            {
                if (_products != value)
                {
                    _products = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewProductCommand { get; }

        public ProductListViewModel(IProductService productService, INavigationService navigationService)
        {
            _productService = productService;
            _navigationService = navigationService;

            RefreshCommand = new Command(async () => await LoadProductsAsync());
            ViewProductCommand = new Command<Guid>(ViewProduct);

            Products = new ObservableCollection<ProductModel>();
        }

        public async Task LoadProductsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                // Используем существующий метод GetAllProductsAsync вместо отсутствующего GetProductsAsync
                var productDtos = await _productService.GetAllProductsAsync();

                // Конвертируем DTO в модели
                var products = productDtos.Select(dto => new ProductModel
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Name = dto.Name,
                    Description = dto.Description,
                    ProductTypeId = dto.ProductTypeId,
                    ProductTypeName = dto.ProductTypeName,
                    UnitOfMeasure = dto.UnitOfMeasure,
                    CurrentStock = dto.CurrentStock,
                    Price = dto.Price,
                    Materials = dto.Materials?.Select(m => new ProductMaterialModel
                    {
                        ProductId = m.ProductId,
                        MaterialId = m.MaterialId,
                        MaterialName = m.MaterialName,
                        MaterialCode = m.MaterialCode,
                        Quantity = m.Quantity,
                        UnitOfMeasure = m.UnitOfMeasure
                    }).ToList() ?? new List<ProductMaterialModel>(),
                    CreatedAt = dto.CreatedAt,
                    UpdatedAt = dto.UpdatedAt
                }).ToList();

                Products = new ObservableCollection<ProductModel>(products);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить продукцию: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterProducts()
        {
            // Реализация фильтрации по поисковому запросу
        }

        private void ViewProduct(Guid productId)
        {
            _navigationService.NavigateToAsync($"product-details?id={productId}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}