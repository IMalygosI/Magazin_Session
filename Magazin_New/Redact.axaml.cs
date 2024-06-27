using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Magaz;

namespace Magazin_New;

public partial class Redact : Window
{
    public Redact()
    {
        InitializeComponent();
        Otmen.Click += Click_Otmen;
        redaktirovat.Click += Redaktirovat_Click;
        colvo.Text = Convert.ToString(ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].ColvoProduct);
        prise.Text = Convert.ToString(ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].PraiseProduct);
        TypeVibor.Text = Convert.ToString(ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].OrganaizProduct);
        opisanie.Text = Convert.ToString(ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].OpisanieProduct);
        name.Text = ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].ProductName;
        TypeVibor2.Text = Convert.ToString(ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].TypeProduct);
        SelectedImage = ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].Sourse;
        if (SelectedImage != null)
        {
            img_preview.Source = new Bitmap($"Asset/{SelectedImage}");
        }
        else
        {
            img_preview.Source = new Bitmap("Asset/default_image.jpg");
        }
        ssil();
    }
    string SelectedImage;
    public void ssil()
    {
        ShopTab.SaveMagaz.Product.Select(x => new { x.image, x.ProductName, x.PraiseProduct, x.Id, x.ColvoProduct, x.OrganaizProduct, x.OpisanieProduct, x.TypeProduct });
    }
    private void Redaktirovat_Click(object? sender, RoutedEventArgs e)
    {
        int Index = ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].Id;
        int i = 0;
        foreach (Product product in ShopTab.SaveMagaz.Product)
        {
            if (ShopTab.SaveMagaz.Product.IndexOf(product) != Index && product.ProductName == name.Text)
            {
                i++;
            }
        }

        string III = " ";
        switch (strok.SelectedIndex)
        {
            case 0:
                III = TypeVibor.Text;
                break;
            case 1:
                III = "ООО 'Олимп'";
                break;
            case 2:
                III = "ООО 'Поклоняемся Солнцу!'";
                break;
            case 3:
                III = "ООО 'Венера'";
                break;
            case 4:
                III = "ООО 'ЯщеркиТехКорп'";
                break;
        }
        string IV = " ";
        switch (strok2.SelectedIndex)
        {
            case 0:
                IV = TypeVibor2.Text;
                break;
            case 1:
                IV = "Техника";
                break;
            case 2:
                IV = "Одежда";
                break;
            case 3:
                IV = "Продукты";
                break;
            case 4:
                IV = "Канцелярия";
                break;
        }
        if (double.TryParse(prise.Text, out double price) && int.TryParse(colvo.Text, out int quantity) &&
             !string.IsNullOrWhiteSpace(name.Text) && !string.IsNullOrWhiteSpace(opisanie.Text))//Проверка чтобы была цена/количество/название товара!
        {
            if (price > 0)
            {
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].Sourse = SelectedImage;
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].ProductName = name.Text;
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].PraiseProduct = Convert.ToDouble(prise.Text);
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].ColvoProduct = Convert.ToInt32(colvo.Text);
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].OrganaizProduct = III;
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].TypeProduct = IV;
                ShopTab.SaveMagaz.Product[ShopTab.massiv[0]].OpisanieProduct = opisanie.Text;

                if (i == 0)
                {
                    DeleteUnusedImages();
                    admin taskWindow = new admin();
                    taskWindow.Show();
                    this.Close();
                }
                else if (i > 0)
                {
                    error4 taskWindow = new error4();
                    taskWindow.Show();
                }
            }
        }
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
    private void DeleteImage(string imageName)
    {
        var filePath = Path.Combine("Assets", imageName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    private string SameName(string filename)
    {
        string[] withExtentions = Directory.GetFiles("Asset");
        List<string> withoutExtentions = [];

        foreach (string file in withExtentions)
            withoutExtentions.Add(Path.GetFileNameWithoutExtension(file));

        foreach (string file1 in withoutExtentions)
            if (file1 == filename)
            {
                return filename;
            }
        return null;
    }
    private readonly FileDialogFilter fileFilter = new()
    {
        Extensions = new System.Collections.Generic.List<string>() { "png", "jpg", "jpeg" },
        Name = "Файлы изображений"
    };
    private async void ImageSelection(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = (sender as Button)!;
        switch (button.Name)
        {
            case "btn_imgAdd":
                OpenFileDialog dialog = new();
                dialog.Filters.Add(fileFilter);

                string[] result = await dialog.ShowAsync(this);
                // Проверяем, что результат не пустой и не равен null
                if (result == null || result.Length == 0)
                    return;

                string imageName = Path.GetFileName(result[0]);
                string[] extension = imageName.Split('.');
                string temp = extension[0];
                int i = 0;
                while (SameName(temp) != null)
                {
                    temp = extension[0] + $"{i}";
                    i++;
                }
                imageName = temp + '.' + extension[1];
                try
                {
                    File.Copy(result[0], $"Asset/{imageName}", true);
                    tblock_preview.IsVisible = img_preview.IsVisible = true;
                    tblock_preview.Text = SelectedImage = imageName;
                    img_preview.Source = new Bitmap($"Asset/{imageName}");
                }
                catch{}
                break;
            case "btn_imgDel":
                if (SelectedImage != null)
                {
                    DeleteImage(SelectedImage);
                    SelectedImage = null;
                }

                DeleteUnusedImages();
                tblock_preview.IsVisible = img_preview.IsVisible = false;
                break;
        }
    }

    public void Click_Otmen(object sender, RoutedEventArgs args)
    {
        admin taskWindow = new admin();
        taskWindow.Show();
        this.Close();
    }
}
