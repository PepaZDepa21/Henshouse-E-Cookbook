using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Henshouse_E_Cookbook
{
    public partial class MainWindow : Window
    {
        Recipe rec;
        public MainWindow()
        {
            InitializeComponent();
            rec = new Recipe();
            DataContext = rec;
            lbInstructions.Visibility = Visibility.Hidden;
            lbRecipeName.Visibility = Visibility.Hidden;
            tbInstructions.Visibility = Visibility.Hidden;
            tbRecipeName.Visibility = Visibility.Hidden;
            lvIngrediencesEdit.Visibility = Visibility.Hidden;
            lvIngrediencesRead.Visibility = Visibility.Hidden;
            btnAddIngredience.Visibility = Visibility.Hidden;
        }

        private void BtnNewRecipeClick(object sender, RoutedEventArgs e)
        {
            tbRecipeName.Visibility = Visibility.Visible;
            lvIngrediencesEdit.Visibility = Visibility.Visible;
            btnAddIngredience.Visibility = Visibility.Visible;
            tbInstructions.Visibility = Visibility.Visible;
        }
        private void BtnImportClick(object sender, RoutedEventArgs e)
        {
            string path = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Txt files (*.txt)|*.txt|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.InitialDirectory = @"C:\";
            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName;
                MessageBox.Show(File.ReadAllText(path));
            }
        }
        private void BtnExportClick(object sender, RoutedEventArgs e)
        {
            
        }
        private void BtnRandomClick(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            rec = Recipe.AllRecipes[r.Next(Recipe.AllRecipes.Count - 1)];
        }
        private void BtnUndoClick(object sender, RoutedEventArgs e)
        {
            rec.ClearRecipe();
        }
        private void BtnSaveClick(object sender, RoutedEventArgs e)
        {
            Recipe temp = Recipe.AllRecipes.Find(r => r.ID == rec.ID);
            int ind = Recipe.AllRecipes.IndexOf(temp);

            if (temp != null)
            {
                Recipe.AllRecipes[ind] = Recipe.CopyRecipe(rec);
            }
            else
            {
                Recipe.AllRecipes.Add(Recipe.CopyRecipe(rec));
            }

            rec.ClearRecipe();
            UpdateRecipeListview();
            UpdateIngredienceListview();
        }
        private void BtnAddIngredinceClick(object sender, RoutedEventArgs e)
        {
            rec.Ingrediences.Add(new Ingredience());
            UpdateIngredienceListview();
        }
        private void BtnRemoveIngredienceClick(object sender, RoutedEventArgs e)
        {
            Ingredience ing = ((Button)sender).DataContext as Ingredience;
            rec.Ingrediences.Remove(ing);
            UpdateIngredienceListview();
        }

        private void BtnReadRecipeClick(object sender, RoutedEventArgs e)
        {
            Recipe recipe = ((Button)sender).DataContext as Recipe;
            rec.Name = recipe.Name;
            rec.Ingrediences = recipe.Ingrediences;
            rec.Instructions = recipe.Instructions;
            rec.ID = recipe.ID;
        }
        private void BtnEditRecipeClick(object sender, RoutedEventArgs e)
        {
            Recipe recipe = ((Button)sender).DataContext as Recipe;
            rec.Name = recipe.Name;
            rec.Ingrediences = recipe.Ingrediences;
            rec.Instructions = recipe.Instructions;
            rec.ID = recipe.ID;
        }
        private void BtnDeleteRecipeClick(object sender, RoutedEventArgs e)
        {
            Recipe recipe = ((Button)sender).DataContext as Recipe;
            Recipe.AllRecipes.Remove(recipe);
            UpdateIngredienceListview();
        }

        public void UpdateRecipeListview()
        {
            lvRecipes.ItemsSource = null;
            lvRecipes.ItemsSource = Recipe.AllRecipes;
        }

        public void UpdateIngredienceListview()
        {
            lvIngrediencesEdit.ItemsSource = null;
            lvIngrediencesEdit.ItemsSource = rec.Ingrediences;
            lvIngrediencesRead.ItemsSource = null;
            lvIngrediencesRead.ItemsSource = rec.Ingrediences;
        }
    }

    class Recipe : INotifyPropertyChanged
    {
        private string? name, instructions, ingrediencesStr;
        public Guid ID { get; set; }
        private List<Ingredience> ingrediences { get; set; }
        public string IngrediencesStr
        {
            get
            {
                return ingrediencesStr;
            }
            set 
            {
                ingrediencesStr = value;
            }
        }
        
        [JsonIgnore]
        public static List<Recipe> AllRecipes { get; set; } = new List<Recipe>();
        [JsonIgnore]
        public List<Ingredience> Ingrediences 
        { 
            get
            {
                return ingrediences;
            }
            set
            {
                ingrediences = value;
                OnPropertyChanged("Ingrediences");
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("Ingrediences");
            }
        }

        public string Instructions
        {
            get
            {
                return instructions;
            }
            set
            {
                instructions = value;
                OnPropertyChanged("Instructions");
                OnPropertyChanged("Ingrediences");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Recipe() 
        {
            ID = Guid.NewGuid();
            Ingrediences = new List<Ingredience>();
        }
        public Recipe(string? rName, string? instructs, Guid iD, List<Ingredience> ingrediences)
        {
            Name = rName;
            Instructions = instructs;
            ID = iD;
            Ingrediences = ingrediences;
            StringBuilder sb = new StringBuilder();
            foreach (var item in ingrediences)
            {
                sb.AppendLine(Ingredience.SerializeIngredience(item));
            }
            IngrediencesStr = sb.ToString();
        }
        public static Recipe CopyRecipe(Recipe recipe) 
        { 
            
            return new Recipe(recipe.Name, recipe.Instructions, recipe.ID, recipe.Ingrediences); 
        }
        

        public void ClearRecipe()
        {
            Name = null;
            Instructions = null;
            ID = Guid.NewGuid();
            Ingrediences = new List<Ingredience>();
            IngrediencesStr = null;
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    class Ingredience
    {
        private string iName;
        public Guid ID { get; set; }

        public string IName
        {
            get
            {
                return iName;
            }
            set
            {
                iName = value;
            }
        }
        private string amount;
        public string Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
            }
        }
        public static string SerializeIngredience(Ingredience ingredience)
        {
            return JsonSerializer.Serialize(ingredience);
        }
        public static Ingredience DeserializeIngredience(string ingredience)
        {
            return JsonSerializer.Deserialize<Ingredience>(ingredience);
        }
        public Ingredience()
        {
            IName = string.Empty;
            amount = string.Empty;
        }
    }
}
