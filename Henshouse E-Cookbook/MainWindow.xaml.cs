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
using System.Text.RegularExpressions;

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
            ReadRecipesFromFile();
            Recipe.UpdateToMatchFilter(rec);
            HideRecipeEditRead();
        }

        private void BtnNewRecipeClick(object sender, RoutedEventArgs e)
        {
            HideRecipeEditRead();
            rec.ClearRecipe();
            ShowEditRecipe();
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
                try
                {
                    string jsonRecipe = File.ReadAllText(path);
                    Recipe.AllRecipes.Add(Recipe.DeserializeRecipe(jsonRecipe));
                    Recipe.UpdateToMatchFilter(rec);
                    MessageBox.Show("Successfully imported recipe");
                    UpdateRecipeListview();
                }
                catch (Exception)
                {
                    MessageBox.Show($"Failed to import recipe from {path}");
                }
            }
        }
        private void BtnExportClick(object sender, RoutedEventArgs e)
        {
            if (rec.IsEmpty())
            {
                var fd = new OpenFileDialog();
                fd.ValidateNames = false;
                fd.CheckFileExists = false;
                fd.CheckPathExists = true;
                fd.FileName = "Select a folder";
                fd.Filter = "Folders|Folder";
                string selectedFolder;

                if (fd.ShowDialog() == true)
                {
                    selectedFolder = System.IO.Path.GetDirectoryName(fd.FileName);
                    MessageBox.Show(selectedFolder);
                    string path = $"{selectedFolder}\\{rec.Name}.txt";
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(path))
                        {
                            sw.WriteLine(Recipe.SerializeRecipe(rec));
                            sw.Flush();
                        }
                        MessageBox.Show($"Successfully exported to {selectedFolder}");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show($"Failed to export to {selectedFolder}");
                    }
                }
            }
            else
            {
                MessageBox.Show("First open recipe you want to export!");
            }
        }
        private void BtnRandomClick(object sender, RoutedEventArgs e)
        {
            HideRecipeEditRead();
            Random r = new Random();
            Recipe recipe = Recipe.RecipesToShow[r.Next(Recipe.RecipesToShow.Count)];
            rec.Name = recipe.Name;
            rec.IngrediencesStr = recipe.IngrediencesStr;
            rec.Instructions = recipe.Instructions;
            rec.ID = recipe.ID;
            rec.Ingrediences = Recipe.StrIngredsToListIngreds(rec.IngrediencesStr);
            UpdateIngredienceListview();
            ShowReadRecipe();
        }
        private void BtnUndoClick(object sender, RoutedEventArgs e)
        {
            rec.ClearRecipe();
            UpdateIngredienceListview();
            HideRecipeEditRead();
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
            Recipe.UpdateToMatchFilter(rec);
            rec.ClearRecipe();
            UpdateRecipeListview();
            UpdateIngredienceListview();
            WriteRecipesToFile();
            HideRecipeEditRead();
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
            HideRecipeEditRead();
            Recipe recipe = ((Button)sender).DataContext as Recipe;
            rec.Name = recipe.Name;
            rec.IngrediencesStr = recipe.IngrediencesStr;
            rec.Instructions = recipe.Instructions;
            rec.ID = recipe.ID;
            rec.Ingrediences = Recipe.StrIngredsToListIngreds(rec.IngrediencesStr);
            UpdateIngredienceListview();
            ShowReadRecipe();
        }
        private void BtnEditRecipeClick(object sender, RoutedEventArgs e)
        {
            HideRecipeEditRead();
            Recipe recipe = ((Button)sender).DataContext as Recipe;
            rec.Name = recipe.Name;
            rec.IngrediencesStr = recipe.IngrediencesStr;
            rec.Instructions = recipe.Instructions;
            rec.ID = recipe.ID;
            rec.Ingrediences = Recipe.StrIngredsToListIngreds(rec.IngrediencesStr);
            ShowEditRecipe();
            UpdateIngredienceListview();
        }
        private void BtnDeleteRecipeClick(object sender, RoutedEventArgs e)
        {
            Recipe recipe = ((Button)sender).DataContext as Recipe;
            Recipe.AllRecipes.Remove(recipe);
            Recipe.UpdateToMatchFilter(rec);
            UpdateRecipeListview();
            WriteRecipesToFile();
        }

        private void BtnSearchClick(object sender, RoutedEventArgs e)
        {
            Recipe.UpdateToMatchFilter(rec);
            UpdateRecipeListview();
        }

        public void Search()
        {
            if (tbSearch.Text == string.Empty || tbSearch.Text == null)
            {
                Recipe.AllRecipes = new List<Recipe>();
                ReadRecipesFromFile();
            }
            else
            {
                List<Recipe> temp = Recipe.AllRecipes;
                Recipe.AllRecipes = new List<Recipe>();
                foreach (var item in temp)
                {
                    foreach (var i in item.Ingrediences)
                    {
                        if (i.IName.Contains(tbSearch.Text))
                        {
                            Recipe.AllRecipes.Add(item);
                        }
                    }
                }
            }
            UpdateRecipeListview();
        }

        public void HideRecipeEditRead()
        {
            lbInstructions.Visibility = Visibility.Hidden;
            lbRecipeName.Visibility = Visibility.Hidden;
            tbInstructions.Visibility = Visibility.Hidden;
            tbRecipeName.Visibility = Visibility.Hidden;
            lvIngrediencesEdit.Visibility = Visibility.Hidden;
            lvIngrediencesRead.Visibility = Visibility.Hidden;
            btnAddIngredience.Visibility = Visibility.Hidden;
        }
        public void ShowEditRecipe()
        {
            tbInstructions.Visibility = Visibility.Visible;
            tbRecipeName.Visibility = Visibility.Visible;
            lvIngrediencesEdit.Visibility = Visibility.Visible;
            btnAddIngredience.Visibility = Visibility.Visible;
        }
        public void ShowReadRecipe()
        {
            lbInstructions.Visibility = Visibility.Visible;
            lbRecipeName.Visibility = Visibility.Visible;
            lvIngrediencesRead.Visibility = Visibility.Visible;
        }

        public void UpdateRecipeListview()
        {
            lvRecipes.ItemsSource = null;
            lvRecipes.ItemsSource = Recipe.RecipesToShow;
        }

        public void UpdateIngredienceListview()
        {
            lvIngrediencesEdit.ItemsSource = null;
            lvIngrediencesEdit.ItemsSource = rec.Ingrediences;
            lvIngrediencesRead.ItemsSource = null;
            lvIngrediencesRead.ItemsSource = rec.Ingrediences;
        }
        public static void WriteRecipesToFile()
        {
            using (StreamWriter sw = new StreamWriter(@".\Recipes.txt"))
            {
                foreach (var item in Recipe.AllRecipes)
                {
                    sw.WriteLine(Recipe.SerializeRecipe(item));
                }
            }
        }
        public void ReadRecipesFromFile()
        {
            try
            {
                string[] recipes = File.ReadAllLines(@".\Recipes.txt");
                foreach (var item in recipes)
                {
                    Recipe.AllRecipes.Add(Recipe.DeserializeRecipe(item));
                }
                UpdateRecipeListview();
            }
            catch (Exception) { }
        }
    }

    class Recipe : INotifyPropertyChanged
    {
        private string? name, instructions, ingrediencesStr, search;
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
        public string Search
        {
            get
            {
                return search;
            }
            set 
            { 
                search = value;
                OnPropertyChanged("Search");
            }
        }

        [JsonIgnore]
        public static List<Recipe> AllRecipes { get; set; } = new List<Recipe>();

        [JsonIgnore]
        public static List<Recipe> RecipesToShow { get; set; } = new List<Recipe>();
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
            for (int i = 0; i < ingrediences.Count; i++)
            {
                if (i == ingrediences.Count - 1)
                {
                    sb.Append(Ingredience.SerializeIngredience(ingrediences[i]));
                }
                else
                {
                    sb.Append($"{Ingredience.SerializeIngredience(ingrediences[i])} ");
                }
            }
            IngrediencesStr = sb.ToString();
        }
        public static Recipe CopyRecipe(Recipe recipe)
        {
            return new Recipe(recipe.Name, recipe.Instructions, recipe.ID, recipe.Ingrediences);
        }

        public bool IsOK()
        {
            bool nameOK = Name != string.Empty;
            bool instructionsOK = Instructions != string.Empty;
            bool ingrediencesOK = false;
            foreach (var item in Ingrediences)
            {
                ingrediencesOK = item.IName != string.Empty && item.RegexAmount.IsMatch(item.Amount);
                if (!ingrediencesOK)
                {
                    break;
                }
            }
            return nameOK && instructionsOK && ingrediencesOK;
        }


        public void ClearRecipe()
        {
            Name = string.Empty;
            Instructions = string.Empty;
            ID = Guid.NewGuid();
            Ingrediences = new List<Ingredience>();
            IngrediencesStr = string.Empty;
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public static List<Ingredience> StrIngredsToListIngreds(string str)
        {
            string[] ingreds = str.Split(' ');
            List<Ingredience> returnList = new List<Ingredience>();
            foreach (var item in ingreds)
            {
                returnList.Add(Ingredience.DeserializeIngredience(item));
            }
            return returnList;
        }
        public static string SerializeRecipe(Recipe recipe)
        {
            return JsonSerializer.Serialize(recipe);
        }
        public static Recipe DeserializeRecipe(string recipe) 
        {
            return JsonSerializer.Deserialize<Recipe>(recipe);
        }
        public bool IsEmpty() 
        {
            return Name != string.Empty && Name != null && instructions != string.Empty && instructions != null && ingrediencesStr != string.Empty & ingrediencesStr != null;
        }
        public static void UpdateToMatchFilter(Recipe recipe)
        {
            RecipesToShow.Clear();
            if (recipe.Search == null || recipe.Search == string.Empty)
            {
                foreach (var item in AllRecipes)
                {
                    RecipesToShow.Add(item);
                }
            }
            else
            {
                foreach (var item in AllRecipes)
                {
                    if (item.Name.Contains(recipe.Search))
                    {
                        RecipesToShow.Add(item);
                    }
                }
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
        public Regex RegexAmount = new Regex("^(\\d{1,4})( )?([a-zA-Z]{1,4})$");
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
        public static List<Ingredience> DeserializeInstructsStr(string ingrediences)
        {
            string[] ings = ingrediences.Split(' ');
            List<Ingredience> retIngs = new List<Ingredience>();
            foreach (var item in ings)
            {
                retIngs.Add(Ingredience.DeserializeIngredience(item)); 
            }
            return retIngs;
        }
        public Ingredience()
        {
            IName = string.Empty;
            amount = string.Empty;
        }
    }
}
