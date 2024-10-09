using SeoulStayS4.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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

namespace SeoulStayS4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadAreas();
            LoadAmenities();
            LoadPropertyTypes();
        }

        private void LoadPropertyTypes()
        {
            using (var context = new SeoulStayS4Entities())
            {
                var property = context.ItemTypes.Select(n => new
                {
                    n.ID,
                    n.Name
                }).ToList();

                propertyTypeCombo.ItemsSource = property;
                propertyTypeCombo.DisplayMemberPath = "Name";
                propertyTypeCombo.SelectedValuePath = "ID";
            }
        }

        private void LoadAreas()
        {
            using (var context = new SeoulStayS4Entities())
            {
                areaCombo.ItemsSource = context.Areas.Select(a => a.Name).ToList();
            }
        }

        private void LoadAmenities()
        {
            using (var context = new SeoulStayS4Entities())
            {
                var amenity = context.Amenities.Select(a => a.Name).ToList();
                amenityCombo.ItemsSource = amenity;
                amenityCombo2.ItemsSource = amenity;
                amenityCombo3.ItemsSource = amenity;
            }
        }

        private void areaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedArea = areaCombo.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedArea))
            {
                titleTextBox.Visibility = Visibility.Hidden;
                titleCombo.Visibility = Visibility.Visible;
                attractionCombo.Visibility = Visibility.Visible;
                attractionTextBox.Visibility = Visibility.Hidden;
                using (var context = new SeoulStayS4Entities())
                {
                    var areaId = context.Areas.FirstOrDefault(a => a.Name == selectedArea).ID;
                    attractionCombo.ItemsSource = context.Attractions.
                        Where(at => at.AreaID == areaId).
                        Select(i => i.Name).ToList();

                    titleCombo.ItemsSource = context.Items.
                        Where(i => i.AreaID == areaId).
                        Select(i => i.Title).ToList();
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            simpleSearchGrid.Visibility = Visibility.Hidden;
            advancedSearchGrid.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            simpleSearchGrid.Visibility = Visibility.Visible;
            advancedSearchGrid.Visibility = Visibility.Hidden;
        }

        private void searchProperties_KeyUp(object sender, KeyEventArgs e) //Jattau kerek mynany, uirenip
        {
            string query = searchProperties.Text;
            if (query.Length >= 3) 
            {
                using (var context = new SeoulStayS4Entities())
                {
                    var suggestions = context.Items
                        .Where(i => i.Title.Contains(query) ||
                                    i.Areas.Name.Contains(query) ||
                                    i.ItemAttractions.Any(at => at.Attractions.Name.Contains(query)) ||
                                    i.ItemTypes.Name.Contains(query) ||
                                    i.ItemAmenities.Any(a => a.Amenities.Name.Contains(query)))
                        .Select(i => new
                        {
                            Name = i.Title,
                        }).ToList();

                    searchProperties.ItemsSource = suggestions;
                    searchProperties.DisplayMemberPath = "Name";
                    searchProperties.SelectedValuePath = "Type";
                    searchProperties.IsDropDownOpen = true; 
                }
            }
            else
            {
                searchProperties.IsDropDownOpen = false; // Hide suggestions if less than 3 characters
            }
        }


        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searching = searchProperties.Text;
            DateTime? dateTime = fromDate.SelectedDate;
            int? nights = string.IsNullOrEmpty(nightNum.Text) ? (int?)null : int.Parse(nightNum.Text);
            int? people = string.IsNullOrEmpty(peopleNum.Text) ? (int?)null : int.Parse(peopleNum.Text);
            DateTime current = DateTime.Now;

            if (dateTime.HasValue)
            {
                if (dateTime.Value.Date < current.Date)
                {
                    MessageBox.Show("Date can not be earlier than current time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            

            using (var context = new SeoulStayS4Entities())
            {
                var result = context.Items.AsQueryable();

                if (string.IsNullOrEmpty(searching))
                {
                    if (nights.HasValue)
                    {
                        result = result.Where(r => r.MinimumNights <= nights.Value);
                    }
                    if (people.HasValue)
                    {
                        result = result.Where(i => i.Capacity >= people.Value);
                    }
                }

                else
                {
                    result = result.Where(i => i.Title.Contains(searching) || i.Areas.Name.Contains(searching));
                    if (nights.HasValue)
                    {
                        result = result.Where(r => r.MaximumNights >= nights.Value);
                    }
                    if (people.HasValue)
                    {
                        result = result.Where(i => i.Capacity >= people.Value);
                    }
                }
                var results = result.Select(i => new
                    {
                    Property = i.Title,
                    Area = i.Areas.Name,
                    AverageScore = i.ItemScores.Any() ? i.ItemScores.Average(s => s.Value) : (double?)null, // Handle nullable AverageScores
                    TotalReservations = i.Users.Bookings.Any() ? i.Users.Bookings.Count() : 0, // If no reservations, return 0
                    AmountPayable = i.ItemPrices.FirstOrDefault() != null ? i.ItemPrices.FirstOrDefault().Price : (decimal?)null // Handle nullable Price
                }).ToList();

                simpleSearchDataGrid.ItemsSource = results;

                displayingOptions.Text = $"Displaying {results.Count} options";
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            areaCombo.SelectedIndex = -1;
            nightsNum2.Text = string.Empty;
            attractionCombo.SelectedIndex = -1;
            titleCombo.SelectedIndex = -1;
            amenityCombo.SelectedIndex = -1;
            fromDate2.SelectedDate = null;
            toDate.SelectedDate = null;
            peopleNum2.Text = string.Empty;
            strartPriceNum.Text = string.Empty;
            maxPriceNum.Text = string.Empty;
            amenityCombo.SelectedIndex = -1;
            amenityCombo2.SelectedIndex = -1;
            amenityCombo3.SelectedIndex = -1;
            propertyTypeCombo.SelectedIndex = -1;
        }

        private void searchPropertyBtn_Click(object sender, RoutedEventArgs e)
        {
            string selectedArea = areaCombo.SelectedItem?.ToString();
            string selectedTitle = titleCombo.SelectedItem?.ToString();
            string selectedAttraction = attractionCombo.SelectedItem?.ToString();
            string amenity1 = amenityCombo.SelectedItem?.ToString();
            string amenity2 = amenityCombo2.SelectedItem?.ToString();
            string amenity3 = amenityCombo3.SelectedItem?.ToString();
            string property2 = propertyTypeCombo.SelectedItem?.ToString();
            DateTime? fromDate = fromDate2.SelectedDate;
            DateTime? finishDate = toDate.SelectedDate;
            decimal? nights = string.IsNullOrEmpty(nightsNum2.Text) ? (decimal?)null : int.Parse(nightsNum2.Text);
            decimal? capacity = string.IsNullOrEmpty(peopleNum2.Text) ? (decimal?)null : int.Parse(peopleNum2.Text);
            int? startPrice = string.IsNullOrEmpty(strartPriceNum.Text) ? (int?)null : int.Parse(strartPriceNum.Text);
            int? maxPrice = string.IsNullOrEmpty(maxPriceNum.Text) ? (int?)null : int.Parse(maxPriceNum.Text);
            DateTime now = DateTime.Now;

            using (var context = new SeoulStayS4Entities())
            {
                var query = context.Items.AsQueryable();

                if (!string.IsNullOrEmpty(selectedArea))
                {
                    query = query.Where(a => a.Areas.Name == selectedArea);
                }

                if (!string.IsNullOrEmpty(selectedAttraction))
                {
                    query = query.Where(a => a.ItemAttractions.Any(i => i.Items.Title == selectedAttraction));
                }

                if (!string.IsNullOrEmpty(selectedTitle))
                {
                    query = query.Where(a => a.Title == selectedTitle); 
                }

                if (fromDate.HasValue && finishDate.HasValue)
                {
                    if (fromDate.Value.Date < now.Date)
                    {
                        MessageBox.Show("Date can not be earlier than current time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (finishDate.Value.Date < fromDate.Value.Date)
                    {
                        MessageBox.Show("Date can not be earlier than start time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                } 
                else

                {
                    MessageBox.Show("Choose date", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (titleTextBox.IsVisible)
                {
                    string title = titleTextBox?.Text;

                    query = query.Where(a => a.Title.Contains(title));
                }

                if (attractionTextBox.IsVisible)
                {
                    string attraction = attractionTextBox?.Text;

                    query = query.Where(a => a.ItemAttractions.Any(i => i.Attractions.Name.Contains(attraction)));
                }

                if(capacity.HasValue && capacity > 0)
                {
                    query = query.Where(a => a.Capacity >= capacity);

                }
                else
                {
                    MessageBox.Show("Capacity should be at least 1", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!nights.HasValue || nights < 1 || nights > 15)
                {
                    MessageBox.Show("Nights should be at least 1 and maximum 15 ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                
                {
                    query = query.Where(a => a.MinimumNights <= nights && a.MaximumNights >= nights);
                }

                if (!string.IsNullOrEmpty(property2))
                {
                    query = query.Where(i => i.ItemTypes.Name == property2);
                }

                if (!string.IsNullOrEmpty(amenity1))
                {
                    query = query.Where(i => i.ItemAmenities.Any(a => a.Amenities.Name == amenity1));
                }

                if (!string.IsNullOrEmpty(amenity2))
                {
                    query = query.Where(i => i.ItemAmenities.Any(a => a.Amenities.Name == amenity2));
                }
                if (!string.IsNullOrEmpty(amenity3))
                {
                    query = query.Where(i => i.ItemAmenities.Any(a => a.Amenities.Name == amenity3));
                }
            
                var result = query.Select(a => new
                {
                    Property = a.Title,
                    Area = a.Areas.Name,
                    AverageScore = a.ItemScores.Any() ? a.ItemScores.Average(s =>  s.Value) : (double?)null,
                    TotalReservations = a.Users.Bookings.Any() ? a.Users.Bookings.Count() : 0, 
                    AmountPayable = a.ItemPrices.FirstOrDefault() != null ? a.ItemPrices.FirstOrDefault().Price : (decimal?)null,

                }).ToList();

                advancedSearchDataGrid.ItemsSource = result;
                int san = result.Select(a => a.Property).Distinct().Count();
                displayingOptions.Text = $"Displaying {result.Count} options from {san} properties";
            }
        }

        
    }
}
