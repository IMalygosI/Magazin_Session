using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Magaz;

namespace Magazin_New;

public partial class user : Window
{
    public user()
    {
        InitializeComponent();

        if (0 <= ShopTab.SaveMagaz.Product.Count - 1)
        {
            for (int i = 0; i < ShopTab.SaveMagaz.Product.Count; i++)
            {
                ShopTab.SaveMagaz.Product[i].Id = i;
            }
            AAA.ItemsSource = ShopTab.SaveMagaz.Product.ToList();
        }
        sotirovka.SelectionChanged += Sotirovka_SelectionChanged;//ЦЕНА
        sotirovka2.SelectionChanged += Sotirovka_OrganaizProduct;//ПРОИЗВОДИТЕЛЬ
        sotirovka3.SelectionChanged += Sotirovka_SortByType;//Тип продукта

        exit.Click += Click_exit;
        in_korz.Click += Click_korz;
        LBoxInitialization(ShopTab.SaveMagaz.Product);
        DeleteUnusedImages();

        ssil("");
    }
    private void ResetSort(object sender, RoutedEventArgs e)
    {
        sotirovka.SelectedIndex = 0;
        sotirovka2.SelectedIndex = 0;
        sotirovka3.SelectedIndex = 0;
        poisk.Text = "";

        ShopTab.SaveMagaz.vremenno.Clear();

        AAA.ItemsSource = ShopTab.SaveMagaz.Product.Select(x => new
        {
            x.ProductName,
            x.Id,
            x.OpisanieProduct,
            x.Sourse,
            x.OrganaizProduct,
            x.ColvoProduct,
            x.PraiseProduct,
            x.TypeProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();
        FiltersTogether();
        ssil("");
    }

    private void LBoxInitialization(List<Product> listBoxSource) //Метод для обновления листбокса
    {
        AAA.ItemsSource = ShopTab.SaveMagaz.Product.Select(x => new //обновление лисбокса, в качестве источника - список, принимаемый методом
        {
            x.ProductName,
            x.Id,
            x.OpisanieProduct,
            x.Sourse,
            x.OrganaizProduct,
            x.ColvoProduct,
            x.PraiseProduct,
            x.TypeProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();
    }
    private void FiltersTogether()
    {
        var searchTerms = poisk.Text.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var productsToFilter = ShopTab.SaveMagaz.vremenno.Any() ? ShopTab.SaveMagaz.vremenno : ShopTab.SaveMagaz.Product;
        var filteredProducts = productsToFilter.AsEnumerable();

        if (searchTerms.Length > 0)
        {
            foreach (var term in searchTerms)
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.ProductName.ToLower().Contains(term) ||
                    p.OpisanieProduct.ToLower().Contains(term) ||
                    p.OrganaizProduct.ToLower().Contains(term) ||
                    p.PraiseProduct.ToString().Contains(term) ||
                    p.ColvoProduct.ToString().Contains(term));
            }
        }

        // Сортировка по типу продукта
        if (sotirovka3.SelectedIndex > 0)
        {
            string selectedTypeProduct = (sotirovka3.SelectedItem as ComboBoxItem).Content.ToString();
            filteredProducts = filteredProducts.Where(p => p.TypeProduct == selectedTypeProduct);
        }

        // Сортировка по производителю
        if (sotirovka2.SelectedIndex > 0)
        {
            string selectedOrganaizProduct = (sotirovka2.SelectedItem as ComboBoxItem).Content.ToString();
            filteredProducts = filteredProducts.Where(p => p.OrganaizProduct == selectedOrganaizProduct);
        }

        // Сортировка по цене
        switch (sotirovka.SelectedIndex)
        {
            case 1: // по возрастанию цены
                filteredProducts = filteredProducts.OrderBy(p => p.PraiseProduct);
                break;
            case 2: // по убыванию цены
                filteredProducts = filteredProducts.OrderByDescending(p => p.PraiseProduct);
                break;
        }

        AAA.ItemsSource = filteredProducts.Select(x => new
        {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.TypeProduct,
            x.OpisanieProduct,
            x.ColvoProduct,
            x.OrganaizProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();

        // Обновление счётчиков товаров
        Colvo.Text = filteredProducts.Count().ToString(); // показано
        TotalCount.Text = ShopTab.SaveMagaz.Product.Count.ToString(); // Всего
    }
    public void Searching(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        var searchTerms = poisk.Text.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        ShopTab.SaveMagaz.vremenno.Clear();
        bool matchesFound = false;

        if (searchTerms.Length > 0)
        {
            foreach (Product product in ShopTab.SaveMagaz.Product)
            {
                bool allTermsMatch = true;
                foreach (var term in searchTerms)
                {
                    allTermsMatch &= product.ProductName.ToLower().Contains(term) ||
                                     product.OpisanieProduct.ToLower().Contains(term) ||
                                     product.OrganaizProduct.ToLower().Contains(term) ||
                                     product.PraiseProduct.ToString().Contains(term) ||
                                     product.ColvoProduct.ToString().Contains(term) ||
                                     product.TypeProduct.ToLower().Contains(term) ||
                                     product.Id.ToString().Contains(term);
                }

                if (allTermsMatch)
                {
                    ShopTab.SaveMagaz.vremenno.Add(product);
                    matchesFound = true;
                }
            }
            if (matchesFound)
            {
                FiltersTogether(); // Вызываем метод фильтрации
            }
            else
            {
                AAA.ItemsSource = new List<Product>(); // Устанавливаем пустой список, если нет совпадений
            }
        }
        else
        {
            AAA.ItemsSource = ShopTab.SaveMagaz.Product.ToList(); // Если строка поиска пуста, отображаем все товары
        }
        FiltersTogether();
    }
    private void Sotirovka_SortByType(object sender, SelectionChangedEventArgs e)
    {
        var searchTerm = poisk.Text.ToLower();
        var productsToFilter = ShopTab.SaveMagaz.Product.AsEnumerable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsToFilter = productsToFilter.Where(p =>
            p.ProductName.ToLower().Contains(searchTerm) ||
            p.OpisanieProduct.ToLower().Contains(searchTerm) ||
            p.OrganaizProduct.ToLower().Contains(searchTerm) ||
            p.PraiseProduct.ToString().Contains(searchTerm) ||
            p.ColvoProduct.ToString().Contains(searchTerm));

        }
        if (sotirovka3.SelectedIndex > 0)
        {
            string selectedTypeProduct = (sotirovka3.SelectedItem as ComboBoxItem).Content.ToString();
            productsToFilter = productsToFilter.Where(p => p.TypeProduct == selectedTypeProduct);
        }
        AAA.ItemsSource = productsToFilter.Select(x => new
        {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.TypeProduct,
            x.OpisanieProduct,
            x.ColvoProduct,
            x.OrganaizProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();

        FiltersTogether(); // Вызываем метод фильтрации для применения остальных фильтров
    }

    private void Sotirovka_OrganaizProduct(object sender, SelectionChangedEventArgs e)
    {
        var searchTerm = poisk.Text.ToLower();
        var productsToFilter = ShopTab.SaveMagaz.Product.AsEnumerable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            productsToFilter = productsToFilter.Where(p =>
            p.ProductName.ToLower().Contains(searchTerm) ||
            p.OpisanieProduct.ToLower().Contains(searchTerm) ||
            p.OrganaizProduct.ToLower().Contains(searchTerm) ||
            p.PraiseProduct.ToString().Contains(searchTerm) ||
            p.ColvoProduct.ToString().Contains(searchTerm));

        }

        if (sotirovka2.SelectedIndex > 0)
        {
            string selectedOrganaizProduct = (sotirovka2.SelectedItem as ComboBoxItem).Content.ToString();
            productsToFilter = productsToFilter.Where(p => p.OrganaizProduct == selectedOrganaizProduct);
        }

        AAA.ItemsSource = productsToFilter.Select(x => new
        {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.TypeProduct,
            x.OpisanieProduct,
            x.ColvoProduct,
            x.OrganaizProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();
        FiltersTogether(); // Вызываем метод фильтрации для применения остальных фильтров
    }
    private void Sotirovka_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (sotirovka.SelectedIndex)
        {
            case 0:
                break;
            case 1: // Сортировка по Воззрастанию
                SortByPrice();
                break;
            case 2: // Сортировка по Убыванию
                OrderByDescending();
                break;
        }
        FiltersTogether();
    }
    private void OrderByDescending()//Убывание "От большего"
    {
        var sortedList = ShopTab.SaveMagaz.Product
            .OrderByDescending(p => p.PraiseProduct)
            .ToList();

        AAA.ItemsSource = sortedList.Select(x => new {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.TypeProduct,
            x.OpisanieProduct,
            x.ColvoProduct,
            x.OrganaizProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();
    }
    private void SortByPrice()//Воззрастание
    {
        var sortedList = ShopTab.SaveMagaz.Product
            .OrderBy(p => p.PraiseProduct)
            .ToList();

        AAA.ItemsSource = sortedList.Select(x => new
        {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.ColvoProduct,
            x.OrganaizProduct,
            x.TypeProduct,
            x.OpisanieProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();
    }
    private void ssil(string type)
    {
        ShopTab.SaveMagaz.vremenno.Clear();
        AAA.ItemsSource = ShopTab.SaveMagaz.Product.Select(x => new {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.ColvoProduct,
            x.OrganaizProduct,
            x.OpisanieProduct,
            x.TypeProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();

        Colvo.Text = $"{ShopTab.SaveMagaz.Product.Count()}";
        TotalCount.Text = ShopTab.SaveMagaz.Product.Count.ToString();//Всего
    }
    private void UpdateList()
    {
        AAA.ItemsSource = null;
        AAA.ItemsSource = ShopTab.SaveMagaz.Product.ToList();

        if (!string.IsNullOrEmpty(poisk.Text))
        {
            var searchResults = ShopTab.SaveMagaz.Product.Where(p => p.ProductName.Contains(poisk.Text)).ToList();
            AAA.ItemsSource = searchResults;
        }
    }
    private void LBoxInitiaLization(List<Product> listBox)
    {
        AAA.ItemsSource = listBox.Select(x => new {
            x.image,
            x.ProductName,
            x.PraiseProduct,
            x.Id,
            x.ColvoProduct,
            x.OrganaizProduct,
            x.OpisanieProduct,
            x.TypeProduct,
            Color = x.ColvoProduct > 0 ? "CenterScreen" : "Gray"
        }).ToList();
    }
    private void DeleteUnusedImages()
    {
        var allImages = Directory.GetFiles("Asset", "*.*", SearchOption.TopDirectoryOnly)
                                 .Select(Path.GetFileName).ToList();
        var Images = ShopTab.SaveMagaz.Product.Select(p => p.Sourse).ToList();
        var protectedImages = new List<string> { "zagluska", "default_image.jpg" };

        foreach (var image in allImages)
        {
            if (!Images.Contains(image) && !protectedImages.Contains(image))
            {
                File.Delete(Path.Combine("Asset", image));
            }
        }
    }
    public void basket(object sender, RoutedEventArgs args)
    {
        if (ShopTab.SaveMagaz.Product.Count > 0)
        {
            int productIndex = (int)(sender as Button)!.Tag!;

            int count = ShopTab.SaveMagaz.Product[productIndex].ColvoProduct;
            if (count > 0)
            {
                ShopTab.SaveMagaz.Product[productIndex].ColvoProduct = count - 1;

                var productInBasket = ShopTab.SaveMagaz.korzinaa.FirstOrDefault(p => p.Id == productIndex);
                if (productInBasket != null)
                {
                    productInBasket.ColvoProduct += 1;
                }
                else
                {
                    var newProduct = ShopTab.SaveMagaz.Product[productIndex].Clone();
                    newProduct.ColvoProduct = 1;
                    ShopTab.SaveMagaz.korzinaa.Add(newProduct);
                }
            }
            for (int i = 0; i < ShopTab.SaveMagaz.Product.Count; i++)
            {
                ShopTab.SaveMagaz.Product[i].Id = i;
            }
            AAA.ItemsSource = ShopTab.SaveMagaz.Product.ToList();
        }
        ssil("");
        FiltersTogether();
    }
    public void Click_korz(object sender, RoutedEventArgs args)
    {
        basket taskWindow = new basket();
        taskWindow.Show();
        this.Close();
    }
    public void Click_exit(object sender, RoutedEventArgs args)
    {
        MainWindow taskWindow = new MainWindow();
        taskWindow.Show();
        this.Close();
    }
}